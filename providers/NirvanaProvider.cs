using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class NirvanaProvider : IMenuProvider
{
    public string Name => "Nirvana";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://restaurant-nirvana.at/karte/";
    private const string Address = "Nirvana, Rotenturmstraße 16-18, 1010 Wien";

    public NirvanaProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://restaurant-nirvana.at/",
            Address = Address
        };

        try {
        var html = await _fetcher.GetStringAsync(Url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var header  = doc.DocumentNode.SelectSingleNode("//h2[contains(., 'MITTAGSM')]");
        if (header == null)
        {
            result.Status = "NO_DATA";
            result.Notes = "Kein H2 'Mittagsmen...' gefunden.";
            return result;
        }

        var container = header.ParentNode?.ParentNode?.ParentNode;
        if (container == null)
        {
            result.Status = "ERROR";
            result.Error = "Container (3 Ebenen über H2) nicht gefunden.";
            return result;
        }

        var titleNodes = container.SelectNodes(".//*[contains(concat(' ', normalize-space(@class), ' '), ' elementor-price-list-title ')]");
        var priceNodes = container.SelectNodes(".//*[contains(concat(' ', normalize-space(@class), ' '), ' elementor-price-list-price ')]");
        var descNodes  = container.SelectNodes(".//*[contains(concat(' ', normalize-space(@class), ' '), ' elementor-price-list-description ')]");
        if (titleNodes == null || titleNodes.Count == 0)
        {
            result.Status = "NO_DATA";
            result.Notes = "Container gefunden, aber keine price-list Titel.";
            return result;
        }

        var section = result.GetOrAddSection("Mittagsmenü");

        for (int i = 0; i < titleNodes.Count; i++)
        {
            var title = NormalizeWhitespace(HtmlEntity.DeEntitize(titleNodes[i].InnerText));
            var price = (priceNodes != null && i < priceNodes.Count)
                ? NormalizeWhitespace(HtmlEntity.DeEntitize(priceNodes[i].InnerText))
                : "";

            var desc = (descNodes != null && i < descNodes.Count)
                ? NormalizeWhitespace(HtmlEntity.DeEntitize(descNodes[i].InnerText))
                : "";

            // Baue eine schöne Zeile
            var line = title;

            if (!string.IsNullOrWhiteSpace(desc))
                line += $" — {desc}";

            if (!string.IsNullOrWhiteSpace(price))
                line += $" ({price})";

            if (!string.IsNullOrWhiteSpace(line))
                section.Items.Add(line);
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
        if (string.IsNullOrWhiteSpace(input)) return "";
        return string.Join(" ", input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}