using System.Text.Json;
using EssenCrawler.Core;
using EssenCrawler.Providers;
using EssenCrawler.Output;
using System.IO;
using System.Windows.Markup;
using Microsoft.Extensions.Configuration;

var date = DateTime.Today;

var fetcher = new Fetcher();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var destPath = config["Output:Path"];

//check config Values

if (string.IsNullOrWhiteSpace(destPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("FEHLER: 'Output:Path' ist in der appsettings.json nicht gesetzt.");
    Console.WriteLine("Drücken Sie Enter um das Programm zu beenden.");
    Console.ResetColor();
    Console.ReadLine();

    Environment.ExitCode = 1;
    return;
}

var providers = new List<IMenuProvider>
{
    //new DummyProvider(fetcher),
    new AchilleusProvider(fetcher),
    //new AkakikoProvider(fetcher),
    new BieradiesProvider(fetcher),
    new DolcePensieroProvider(fetcher),
    new EllasProvider(fetcher),
    new FaerberProvider(fetcher),
    new FladereiProvider(fetcher),
    new HiddenKitchenProvider(),
    new HoyeProvider(),
    new IKOProvider(),
    new KarmaFoodProvider(fetcher),
    new KitchAProvider(),
    new KrahKrahProvider(fetcher),
    new MaeAurelProvider(fetcher),
    new MisoUProvider(),
    new NirvanaProvider(fetcher),
    new PhoLinhProvider(fetcher),
    new QeroProvider(fetcher),
    new RadatzProvider(fetcher),
    new SchachtelwirtProvider(fetcher),
    //new SchoenScharfProvider(fetcher),
    //new SubwayProvider(fetcher),
    new TonisProvider(),
    new TopfUDeckelProvider(fetcher),
    new TopLokalProvider(fetcher),
    new WrenkhProvider(fetcher)

};

var results = new List<MenuResult>();

foreach (var p in providers)
{
    try
    {
        var res = await p.GetMenuAsync(date);
        results.Add(res);
    }
    catch (Exception ex)
    {
        results.Add(new MenuResult
        {
            Restaurant = p.Name,
            Date = date.Date,
            Source = "",
            Status = "ERROR",
            Error = ex.Message
        });
    }
}

Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions
{
    WriteIndented = true
}));



//create HTML
var html = HtmlOutput.BuildPage(date, results);

if(!Directory.Exists(destPath))
{
    Directory.CreateDirectory(destPath);
}

var htmlPath = Path.Combine(destPath,$"mittagsmenu.html");

File.WriteAllText(htmlPath, html);
Console.WriteLine($"HTML geschrieben nach: {htmlPath}");

//create css
var cssSource = Path.Combine(AppContext.BaseDirectory, "output", "style.css");
var cssPath = Path.Combine(destPath,$"style.css");
File.Copy(cssSource, cssPath, overwrite: true);
Console.WriteLine($"CSS : {cssPath}");

//copy Background
var bgSource = Path.Combine(AppContext.BaseDirectory, "output", "cooking-banner.jpg");
var bgPath = Path.Combine(destPath,$"cooking-banner.jpg");
File.Copy(bgSource,bgPath,overwrite: true);
Console.WriteLine($"Banner : {bgPath}");

//copy Icon
var iconSource = Path.Combine(AppContext.BaseDirectory, "output", "steak.png");
var iconPath = Path.Combine(destPath,$"steak.png");
File.Copy(iconSource,iconPath,overwrite: true);
Console.WriteLine($"Icon : {iconPath}");

//copy JavaScript
var JSSource = Path.Combine(AppContext.BaseDirectory, "output", "script.js");
var JSPath = Path.Combine(destPath,$"script.js");
File.Copy(JSSource,JSPath,overwrite: true);
Console.WriteLine($"Script : {JSPath}");

//copy AllergenePdf
var AllergeneSource = Path.Combine(AppContext.BaseDirectory, "output", "Allergene.pdf");
var AllergenePath = Path.Combine(destPath,$"Allergene.pdf");
File.Copy(AllergeneSource,AllergenePath,overwrite: true);
Console.WriteLine($"Script : {AllergenePath}");