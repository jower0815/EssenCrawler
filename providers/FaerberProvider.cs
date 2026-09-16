using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class FaerberProvider : IMenuProvider
{
    public string Name => "Faerber";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://faerberwien.at/our-menus/";
    private const string Address = "Faerber Cafe & Restaurant, Färbergasse 8/3, 1010 Wien";

    public FaerberProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://faerberwien.at/",
            Address = Address,
            ImageUrl = ""
        };

        try {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var datum = date.ToString("yyyy'/'MM");
            var img = doc.DocumentNode.SelectSingleNode($"//img[contains(@src, '{datum}')]");

            if (img == null)
            {
                result.Status = "NO_DATA";
                result.Notes = $"Kein Bild für '{datum}' gefunden.";
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