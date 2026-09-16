using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace EssenCrawler.Core;

// Prüft bei jedem Start, ob auf GitHub ein neueres Release liegt, tauscht bei
// Bedarf die Programmdateien aus und startet die App mit der neuen Version neu.
public static class SelfUpdater
{
    private const string RepoOwner = "jower0815";
    private const string RepoName = "EssenCrawler";

    public static async Task<bool> CheckAndUpdateAsync(string[] args)
    {
        try
        {
            var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EssenCrawler", currentVersion.ToString()));
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await http.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var json = await JsonDocument.ParseAsync(stream);
            var root = json.RootElement;

            var tagName = root.GetProperty("tag_name").GetString() ?? "";
            var versionText = tagName.TrimStart('v', 'V');
            if (!Version.TryParse(versionText, out var latestVersion) || latestVersion <= currentVersion)
            {
                return false;
            }

            var asset = root.GetProperty("assets").EnumerateArray()
                .FirstOrDefault(a => a.GetProperty("name").GetString()?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true);

            if (asset.ValueKind == JsonValueKind.Undefined)
            {
                return false;
            }

            var downloadUrl = asset.GetProperty("browser_download_url").GetString();
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                return false;
            }

            Console.WriteLine($"Update verfügbar: {currentVersion} -> {latestVersion}. Lade herunter...");

            var updateRoot = Path.Combine(Path.GetTempPath(), "EssenCrawler_update_" + Guid.NewGuid().ToString("N"));
            var extractDir = Path.Combine(updateRoot, "extracted");
            Directory.CreateDirectory(extractDir);
            var zipPath = Path.Combine(updateRoot, "update.zip");

            var zipBytes = await http.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(zipPath, zipBytes);
            ZipFile.ExtractToDirectory(zipPath, extractDir);

            var scriptPath = Path.Combine(updateRoot, "apply-update.ps1");
            await File.WriteAllTextAsync(scriptPath, UpdateScript);

            var targetDir = AppContext.BaseDirectory.TrimEnd('\\', '/');
            var exePath = Path.Combine(targetDir, "EssenCrawler.exe");
            var forwardedArgs = string.Join(' ', args.Select(a => $"\"{a}\""));

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-ExecutionPolicy");
            psi.ArgumentList.Add("Bypass");
            psi.ArgumentList.Add("-File");
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add("-SourceDir");
            psi.ArgumentList.Add(extractDir);
            psi.ArgumentList.Add("-TargetDir");
            psi.ArgumentList.Add(targetDir);
            psi.ArgumentList.Add("-ProcessId");
            psi.ArgumentList.Add(Environment.ProcessId.ToString());
            psi.ArgumentList.Add("-ExePath");
            psi.ArgumentList.Add(exePath);
            psi.ArgumentList.Add("-Arguments");
            psi.ArgumentList.Add(forwardedArgs);

            Process.Start(psi);
            Console.WriteLine("Update wird angewendet, Programm wird neu gestartet...");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auto-Update fehlgeschlagen, fahre mit aktueller Version fort: {ex.Message}");
            return false;
        }
    }

    private const string UpdateScript = @"
param(
    [string]$SourceDir,
    [string]$TargetDir,
    [int]$ProcessId,
    [string]$ExePath,
    [string]$Arguments = """"
)

try { Wait-Process -Id $ProcessId -Timeout 30 -ErrorAction SilentlyContinue } catch {}
Start-Sleep -Seconds 1

Get-ChildItem -Path $SourceDir -Recurse -File | ForEach-Object {
    $relative = $_.FullName.Substring($SourceDir.Length).TrimStart('\')
    if ($relative -ieq 'appsettings.json') { return }
    $dest = Join-Path $TargetDir $relative
    $destFolder = Split-Path $dest -Parent
    if (!(Test-Path $destFolder)) { New-Item -ItemType Directory -Path $destFolder -Force | Out-Null }
    Copy-Item -Path $_.FullName -Destination $dest -Force
}

Remove-Item -Path $SourceDir -Recurse -Force -ErrorAction SilentlyContinue

if ([string]::IsNullOrWhiteSpace($Arguments)) {
    Start-Process -FilePath $ExePath -WorkingDirectory $TargetDir
} else {
    Start-Process -FilePath $ExePath -ArgumentList $Arguments -WorkingDirectory $TargetDir
}
";
}
