using System.Globalization;
using HtmlAgilityPack;
using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class KrahKrahProvider : IMenuProvider
{
    public string Name => "Krah-Krah";

    private readonly Fetcher _fetcher;
    private const string Url = "https://www.krah-krah.at/";

    // Optional: fix hinterlegen (wenn du das im HTML anzeigen willst)
    private const string Address = "Krah-Krah, Wien Rabensteig 8";

    public KrahKrahProvider(Fetcher fetcher)
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

            // Wochentag deutsch (passt zu deiner bisherigen PS-Logik)
            var weekday = date.ToString("dddd", new CultureInfo("de-AT"));
            weekday = CapitalizeFirst(weekday); // “montag” -> “Montag” (je nach Culture)

            // 1) Menü-Container (wie in deinem PS-Snippet)
            var menuContainer =
                doc.DocumentNode.SelectSingleNode(
                    "//*[contains(@class,'menu') and contains(@class,'bg-beige') and contains(@class,'max-w-4xl')]"
                );

            if (menuContainer == null)
            {
                result.Status = "ERROR";
                result.Error = "Menü-Container nicht gefunden (HTML hat sich evtl. geändert).";
                return result;
            }

            // 2) Richtigen Tag (h3) finden
            var dayH3 = menuContainer.SelectSingleNode(
                $".//h3[normalize-space()='{weekday}']"
            );

            if (dayH3 == null)
            {
                result.Status = "NO_DATA";
                result.Notes = $"Kein Eintrag für '{weekday}' gefunden.";
                return result;
            }

            // 3) Nächste sinnvolle Inhaltselemente nach dem h3 einsammeln
            var items = new List<string>();
            var node = dayH3.NextSibling;

            while (node != null && items.Count < 2)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
                    text = NormalizeWhitespace(text);

                    if (!string.IsNullOrWhiteSpace(text))
                        items.Add(text);
                }

                node = node.NextSibling;
            }

            if (items.Count == 0)
            {
                result.Status = "NO_DATA";
                result.Notes = $"'{weekday}' gefunden, aber keine Menütexte danach extrahiert.";
                return result;
            }

            var daily = result.GetOrAddSection("Menü des Tages");

            foreach(var item in items)
            {
                daily.Items.Add(item);
            }

            var dauerBrenner = result.GetOrAddSection("Dauerbrenner");
            dauerBrenner.Items.Add("Wiener Schnitzel vom Schwein mit Kartoffelsalat");

            // 4) Preis (optional) – div mit text-turquoise + italic
            var priceNode = doc.DocumentNode.SelectSingleNode(
                "//*[self::div or self::p][contains(@class,'text-turquoise') and contains(@class,'italic')]"
            );

            if (priceNode != null)
            {
                var priceText = NormalizeWhitespace(HtmlEntity.DeEntitize(priceNode.InnerText).Trim());
                if (!string.IsNullOrWhiteSpace(priceText))
                    result.PriceInfo = priceText;
            }

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