using System.Globalization;
using HtmlAgilityPack;
using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class TopLokalProvider : IMenuProvider
{
    public string Name => "Top-Lokal";

    private readonly Fetcher _fetcher;
    private const string Url = "https://top-lokal.at/lokal/mittagsmenue/";

    // Optional: fix hinterlegen (wenn du das im HTML anzeigen willst)
    private const string Address = "Top-Lokal Fleischmarkt 18, 1010 Wien";

    public TopLokalProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = Url,
            Address = Address
        };

        try
        {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            
            var weekday = date.ToString("dddd", new CultureInfo("en-US"));
            weekday = CapitalizeFirst(weekday);

            var xpath = $"//p[comment()[contains(.,'{weekday}')]]/following-sibling::div[1]";
            var dayRoot = doc.DocumentNode.SelectSingleNode(xpath);

            if (dayRoot == null)
            {
                result.Status = "NO_DATA";
                result.Notes = $"Kein Eintrag für '{weekday}' gefunden.";
                return result;
            }

            //Suppe
            var suppe = dayRoot.SelectSingleNode(".//div[contains(.,'Suppe')]/following-sibling::div[1]");
            if (suppe != null)
            {
                var SuppeSection = result.GetOrAddSection("Suppe");
                SuppeSection.Items.Add(suppe.InnerText);
            }

            //Menü
            var Menue1 = dayRoot.SelectSingleNode(".//div[contains(.,'Menüs')]/following-sibling::div[1]");
            var Menue2 = dayRoot.SelectSingleNode(".//div[contains(.,'Menüs')]/following-sibling::div[2]");
            if (Menue1 != null || Menue2 != null)
            {
                var menuSection = result.GetOrAddSection("Menue");
                if (Menue1 != null) menuSection.Items.Add(Menue1.InnerText);
                if (Menue2 != null) menuSection.Items.Add(Menue2.InnerText);
            }

            //Dessert
            var dessert = dayRoot.SelectSingleNode(".//div[contains(.,'Dessert')]/following-sibling::div[1]");
            if (dessert != null)
            {
                var dessertSection = result.GetOrAddSection("Dessert");
                dessertSection.Items.Add(dessert.InnerText);
            }

            result.Status = result.Sections.Count > 0 ? "OK" : "NO_DATA";
            return result;
        }
        catch (Exception ex)
        {
            result.Status = "ERROR";
            result.Error = ex.Message;
            return result;
        }
    }

    private static string NormalizeWhitespace(string input)
    {
        // entfernt Mehrfach-Leerzeichen/Zeilenumbrüche
        return string.Join(" ", input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        return char.ToUpper(s[0]) + s[1..];
    }
}