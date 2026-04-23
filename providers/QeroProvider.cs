using EssenCrawler.Core;
using HtmlAgilityPack;
using UglyToad.PdfPig.Fonts.Type1;

namespace EssenCrawler.Providers;

public class QeroProvider : IMenuProvider
{
    public string Name => "Qero";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://qero-viena.at/home-2/";
    private const string Address = "QERO Peruvian Cuisine & Bar, Börsegasse 9/16, 1010 Wien";

    public QeroProvider(Fetcher fetcher)
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

        var xpath = $"//a[contains(@href,'Mittag-')]";
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