using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class SchachtelwirtProvider : IMenuProvider
{
    public string Name => "Schachtelwirt";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://dievom.wixsite.com/website";

    private const string Address = "Schachtelwirt, Judengasse 5, 1010 Wien";

    public SchachtelwirtProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://www.schachtelwirt.at/",
            Address = Address,
            ImageUrl = ""
        };

        try {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var img = doc.DocumentNode.SelectSingleNode("//img[@id='img_comp-l2cdzq1o']");

            if (img == null)
            {
                result.Status = "NO_DATA";
                result.Notes = "Kein Bild-Element gefunden.";
                return result;
            }

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