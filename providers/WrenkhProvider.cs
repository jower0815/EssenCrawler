using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class WrenkhProvider : IMenuProvider
{
    public string Name => "Dummy";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://example.com";
    private const string Address = "Example, Lange Gasse 123, 1010 Wien";

    public WrenkhProvider(Fetcher fetcher)
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
            var datum = DateTime.Today.ToString("ddd").ToUpper();
            var DayDiv = doc.DocumentNode.SelectSingleNode($".//p[normalize-space(text()) = 'DI']");

            
            

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