using System.Globalization;
using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class FladereiProvider : IMenuProvider
{
    public string Name => "Fladerei";

    private readonly Fetcher _fetcher;
    private const string Url = "https://www.fladerei.com/dyn_inhalte/salzgries/tagesfladen_salzgries.html";
    private const string Address = "Fladerei, Salzgries 15, 1010 Wien";

    public FladereiProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://www.fladerei.com/salzgries/",
            Address = Address
        };

        try {
        var html = await _fetcher.GetStringAsync(Url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var MittagsPreis = NormalizeWhitespace(HtmlEntity.DeEntitize(doc.DocumentNode.SelectSingleNode("//h2[contains(normalize-space(.), 'Mittagsangebot')]")?.InnerText ?? ""));
        //var MittagsPreisText = NormalizeWhitespace(HtmlEntity.DeEntitize(MittagsPreis?.InnerText ?? ""));

        result.PriceInfo = MittagsPreis;

        var dateKey = date.ToString("dd.MM");

        var row = doc.DocumentNode.SelectSingleNode($".//tr[td[1][contains(normalize-space(.), '{dateKey}')]]");

        var tagesFlade = row.SelectSingleNode("./td[2]");

        // Allergene separat rausziehen (falls vorhanden)
        var allergenNode = tagesFlade.SelectSingleNode(".//smallblue");
        var allergens = allergenNode != null
            ? NormalizeWhitespace(HtmlEntity.DeEntitize(allergenNode.InnerText))
            : null;

        // Für den Menütext entfernen wir den smallblue Node temporär (damit er nicht im Text landet)
        allergenNode?.Remove();

        var tagesFladeText = NormalizeWhitespace(HtmlEntity.DeEntitize(tagesFlade.InnerText));

        if (!string.IsNullOrWhiteSpace(tagesFladeText))
        {
            var tagesFladeSection = result.GetOrAddSection("Tagesflade");
            tagesFladeSection.Items.Add($"Tagesfladen: {tagesFladeText}");
            result.Status = "OK";
        }

        var wochenTable = doc.DocumentNode.SelectSingleNode("//table[@title='Wochenfladen']");

        if (wochenTable != null){
            var wRow = wochenTable.SelectSingleNode(".//tr");

            var WochenFlade = wRow.SelectSingleNode("./td[2]");
            var WochenPrice = NormalizeWhitespace(HtmlEntity.DeEntitize(wRow.SelectSingleNode("./td[3]")?.InnerText ?? ""));
            var wallergenNode = WochenFlade.SelectSingleNode(".//smallblue");
            wallergenNode?.Remove();

            var WochenFladeText = NormalizeWhitespace(HtmlEntity.DeEntitize(WochenFlade?.InnerText ?? ""));
            WochenFladeText += $" - {WochenPrice}";


            if (!string.IsNullOrWhiteSpace(WochenFladeText))
            {
                var WochenFladeSection = result.GetOrAddSection("Wochenflade");
                WochenFladeSection.Items.Add($"Wochenflade: {WochenFladeText}");
                result.Status = "OK";
            }
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

    private static string NormalizeWhitespace(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        return string.Join(" ", input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}