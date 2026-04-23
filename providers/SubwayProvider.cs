using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class SubwayProvider : IMenuProvider
{
    public string Name => "Dummy";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://example.com";
    private const string Address = "Example, Lange Gasse 123, 1010 Wien";

    public SubwayProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "internal",
            Address = Address
        };

        try {
        var html = await _fetcher.GetStringAsync(Url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();
        var h1 = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim();
        var p1 = doc.DocumentNode.SelectSingleNode("//p1")?.InnerText?.Trim();

        if (!string.IsNullOrWhiteSpace(title))
                result.Items.Add($"Title: {title}");

            if (!string.IsNullOrWhiteSpace(h1))
                result.Items.Add($"H1: {h1}");

            if (!string.IsNullOrWhiteSpace(p1))
                result.Items.Add($"Text: {p1}");

        
        if (result.Items.Count == 0)
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
}