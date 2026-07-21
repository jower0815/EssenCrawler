using EssenCrawler.Core;
using HtmlAgilityPack;
using UglyToad.PdfPig.Fonts.Type1;

namespace EssenCrawler.Providers;

public class BieradiesProvider : IMenuProvider
{
    public string Name => "Bieradies";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://www.bieradies.co.at/";
    private const string MittagsURL = "https://www.bieradies.co.at/speisekarte";
    private const string Address = "Bieradies, Judenplatz 1, 1010 Wien";


    public BieradiesProvider(Fetcher fetcher)
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
            EmbedType = "pdf",
            Status = "OK",
            Address = Address
        };

        try {
        var html = await _fetcher.GetStringAsync(MittagsURL);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var Wochenmenue = GetMonday(date).ToString("dd.M.");
        var xpath = $"//a[contains(@href,'{Wochenmenue}')]";
        var link = doc.DocumentNode.SelectSingleNode(xpath)?.GetAttributeValue("href","");

        if (string.IsNullOrWhiteSpace(link))
            {
                xpath = $"//a[contains(@href,'wochenkarte')]";
                link = doc.DocumentNode.SelectSingleNode(xpath)?.GetAttributeValue("href",""); 
            }


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

    public static DateTime GetMonday (DateTime date)
    {
        int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return date.Date.AddDays(-diff);
    }
}