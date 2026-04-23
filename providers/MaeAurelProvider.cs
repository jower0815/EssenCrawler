using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class MaeAurelProvider : IMenuProvider
{
    public string Name => "MaeAurel";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://maeaurel.com/lunchkarte/";
    private const string Address = "MAE AUREL, Salzgries 3, 1010 Wien";

    public MaeAurelProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
         var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://maeaurel.com/",
            Address = Address,
            ImageUrl = ""
        };

        try {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var datum = DateTime.Today.ToString("yyyy");
            var img = doc.DocumentNode.SelectSingleNode($"//img[contains(@src, '{datum}')]");

            
            result.ImageUrl = img.GetAttributeValue("src", "./404.png");

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