using System.Diagnostics;
using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class WrenkhProvider : IMenuProvider
{
    public string Name => "Wrenkh";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://wrenkh-wien.at/site/de/restaurant/mittagsmenue";
    private const string Address = "WRENKH, Bauernmarkt 10, 1010 Wien";

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
            Source = "https://wrenkh-wien.at/",
            Address = Address
        };

         try {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var datum = DateTime.Today.ToString("ddd")
                .ToUpper()
                .Replace(".", "");
            
            var dayTitle = doc.DocumentNode.SelectSingleNode(
                $"//p[contains(concat(' ', normalize-space(@class), ' '), ' title ') and normalize-space(.) = '{datum}']"
            );
            
            if (dayTitle == null)
                return result;
            
            var itemBlock = dayTitle.SelectSingleNode(
                "./ancestor::div[starts-with(@class, 'item_')][1]"
            );
            
            if (itemBlock == null)
                return result;
            
            var menuItems = itemBlock.SelectNodes(
                ".//div[contains(concat(' ', normalize-space(@class), ' '), ' inner_it ')]"
            );
            
            if (menuItems == null)
                return result;
            
            var sections = new[]
            {
                result.GetOrAddSection("Vorspeise"),
                result.GetOrAddSection("Salat"),
                result.GetOrAddSection("Hauptspeise")
            };
            
            for (int i = 0; i < menuItems.Count && i < sections.Length; i++)
            {
                var textParts = menuItems[i].SelectNodes(
                    ".//div[contains(concat(' ', normalize-space(@class), ' '), ' text_wrapper ')]//p"
                );
            
                if (textParts == null)
                    continue;
            
                var item = string.Join(" ",
                    textParts
                        .Select(p => HtmlEntity.DeEntitize(p.InnerText.Trim()))
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                );
            
                sections[i].Items.Add(item);
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