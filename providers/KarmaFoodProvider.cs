using EssenCrawler.Core;
using HtmlAgilityPack;
using UglyToad.PdfPig.Fonts.Type1;

namespace EssenCrawler.Providers;

public class KarmaFoodProvider : IMenuProvider
{
    public string Name => "KarmaFood";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://karmafood.at/";
    private const string Address = "Karma Food Laurenzerberg, Laurenzerberg 3, 1010 Wien";

    public KarmaFoodProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var r = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = Url,
            EmbedUrl = "",
            EmbedType = "pdf",
            Status = "OK",
            Address = Address
        };

        try {
        var html = await _fetcher.GetStringAsync(Url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var xpath = $"//a[contains(@href,'Mittagessen')]";
        var link = doc.DocumentNode.SelectSingleNode(xpath)?.GetAttributeValue("href","");

        r.EmbedUrl = link;

        return r;
        
        }
        catch (Exception ex)
        {
            r.Status = "ERROR";
            r.Error = ex.Message;
            return r;
        }
    }
}