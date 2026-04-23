using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class RadatzProvider : IMenuProvider
{
    public string Name => "Radatz";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://www.radatz.at/wochenkarte/fleischerei-radatz-wipplinger-strasse-wien";
    private const string Address = "Radatz Wipplinger Straße 3 1010 Wien";

    public RadatzProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://www.radatz.at/wochenkarte/fleischerei-radatz-wipplinger-strasse-wien",
            Address = Address
        };

        try {
        var html = await _fetcher.GetStringAsync(Url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var weekday = date.ToString("dddd, dd.MM.", new CultureInfo("de-AT"));

        var wochenkarte = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'weekly-content-col-1')]");

        if (wochenkarte == null)
        {
            result.Status = "ERROR";
            result.Error = "weekly-content-col-1 nicht gefunden";
            return result;
        }

        var tageskarte = wochenkarte.SelectSingleNode($".//div[contains(., '{weekday}')]");
        if (tageskarte == null)
        {
            result.Status = "NO_DATA";
            result.Notes = $"Kein Block für '{weekday}' gefunden.";
            return result;
        }

        var tagesgerichte = tageskarte.SelectNodes(".//div[contains(@class, 'weekly-col-2-item-col-1')]");
        var tagespreise = tageskarte.SelectNodes(".//div[contains(@class, 'weekly-col-2-item-col-2')]");

        if (tagesgerichte == null || tagesgerichte.Count == 0)
        {
            result.Status = "NO_DATA";
            result.Notes = "Tagesblock gefunden, aber keine weekly-col-2-item Elemente.";
            return result;
        }

        var i = 0;

        var day = result.GetOrAddSection("Tages Menü");

        foreach (var gericht in tagesgerichte)
            {
                var allergene = gericht.SelectSingleNode(".//span");
                allergene?.Remove();

                var output = NormalizeWhitespace(HtmlEntity.DeEntitize(gericht.InnerText));
                output += " " + NormalizeWhitespace(HtmlEntity.DeEntitize(tagespreise[i].InnerText));

                day.Items.Add(output);

                i++;
            }

        var Saisonkarte = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'weekly-box-red')]");
        if (Saisonkarte == null)
        {
            result.Status = "NO_DATA";
            result.Notes = "Keine Saison Karte gefunden";
            return result;
        }

        var saisonsgerichte = Saisonkarte.SelectNodes(".//p");
        if (saisonsgerichte == null)
            {
                result.Status = "NO_DATA";
                result.Notes = "Keine Saison Gerichte gefunden";
                return result;
            }

        var saison = result.GetOrAddSection("Saisonales");
        foreach (var gericht in saisonsgerichte)
            {
                var allergene = gericht.SelectSingleNode(".//span");
                allergene?.Remove();
              
                saison.Items.Add(NormalizeWhitespace(HtmlEntity.DeEntitize(gericht.InnerText)));
            }




        if (result.Sections.Count == 0)
            {
                result.Status = "NO_DATA";
                result.Notes = "HTML geladen, aber keine relevanten Nodes gefunden.";
            }
            else
            {
                result.Status = "OK";
            }

            return result;

        }
        catch (Exception ex)
        {
            result.Status = "ERROR";
            result.Error = ex.Message;
            return result;
        }
    }
    static string NormalizeWhitespace(string input)
    {
        return string.Join(" ", input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}