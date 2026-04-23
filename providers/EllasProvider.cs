using EssenCrawler.Core;
using HtmlAgilityPack;
using UglyToad.PdfPig.Fonts.Type1;

namespace EssenCrawler.Providers;

public class EllasProvider : IMenuProvider
{
    public string Name => "Ellas";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://www.ellas.at/";
    private const string Address = "Ellas, Judenplatz 9, 1010 Wien";

    public EllasProvider(Fetcher fetcher)
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


        var xpath = $"//a[contains(@href,'Mittagsmenue')]";
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