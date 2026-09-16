using System.Text.Json;
using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class AchilleusProvider : IMenuProvider
{
    public string Name => "Achilleus";
    private readonly Fetcher _fetcher;
    private const string Url = "https://www.restaurant-achilleus.at/wochenmenue";
    private const string Address = "Achilleus, Köllnerhofgasse, 1010 Wien";

    public AchilleusProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://www.restaurant-achilleus.at/",
            EmbedType = "pdf",
            Address = Address
        };

        try
        {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Wochenmenü liegt als PDF in einem pdf-flipbook-Widget, die PDF-URL steckt
            // als JSON im "widget-data"-Attribut (kein serverseitig gerendeter Text auf der Wix-Seite).
            var widgetData = doc.DocumentNode.SelectSingleNode("//pdf-flipbook")?.GetAttributeValue("widget-data", "");

            if (string.IsNullOrWhiteSpace(widgetData))
            {
                result.Status = "NO_DATA";
                result.Notes = "Kein PDF-Widget gefunden.";
                return result;
            }

            using var jsonDoc = JsonDocument.Parse(HtmlEntity.DeEntitize(widgetData));
            var fileUrl = jsonDoc.RootElement.TryGetProperty("fileUrl", out var fileUrlProp)
                ? fileUrlProp.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                result.Status = "NO_DATA";
                result.Notes = "Keine 'fileUrl' im PDF-Widget gefunden.";
                return result;
            }

            result.EmbedUrl = fileUrl;
            result.Status = "OK";
            return result;
        }
        catch (Exception ex)
        {
            result.Status = "ERROR";
            result.Error = ex.Message;
            return result;
        }
    }
}
