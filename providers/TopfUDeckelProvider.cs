using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using EssenCrawler.Core;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace EssenCrawler.Providers;

public class TopfUDeckelProvider : IMenuProvider
{
    public string Name => "Topf & Deckel";

    private readonly Fetcher _fetcher;
    private const string Url = "https://topfdeckel.at/";
    private const string Address = "Topf & Deckel, Schottengasse 3A, 1010 Wien";

    public TopfUDeckelProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = Url,
            Address = Address,
            ID = "Topf-Und-Deckel"
        };

        try
        {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //Add Sections
            var starters = result.GetOrAddSection("Starters");
            var main = result.GetOrAddSection("Main");
            var dessert = result.GetOrAddSection("Dessert");

            

            var MenuSections = doc.DocumentNode.SelectNodes("//div[contains(@class, 'mb-4')]");

            if (MenuSections == null)
            {
                result.Status = "NO_DATA";
                result.Notes = "Keine Menu-Sections gefunden.";
                return result;
            }

            foreach (var MenuCard in MenuSections)
            {
                switch(MenuCard.InnerText)
                {
                    case var naming when naming.StartsWith("Starters"):
                    {
                        var EssensItems = MenuCard.SelectNodes(".//h3");
                        foreach (var EssensItem in EssensItems ?? Enumerable.Empty<HtmlNode>())
                        {
                            var CleanedItem = WebUtility.HtmlDecode(EssensItem.InnerText);
                            starters.Items.Add(CleanedItem);
                        }
                        break;
                    }
                    case var naming when naming.StartsWith("Main"):
                    {
                        var EssensItems = MenuCard.SelectNodes(".//h3");
                        foreach (var EssensItem in EssensItems ?? Enumerable.Empty<HtmlNode>())
                        {
                            var CleanedItem = WebUtility.HtmlDecode(EssensItem.InnerText);
                            main.Items.Add(CleanedItem);
                        }
                        break;
                    }
                    case var naming when naming.StartsWith("Dessert"):
                    {
                        var EssensItems = MenuCard.SelectNodes(".//h3");
                        foreach (var EssensItem in EssensItems ?? Enumerable.Empty<HtmlNode>())
                        {
                            var CleanedItem = WebUtility.HtmlDecode(EssensItem.InnerText);
                            dessert.Items.Add(CleanedItem);
                        }
                        break;
                    }
                }
            }

            

            //AddSection(result, "Vorspeise", menu.Starters);
            //AddSection(result, "Salat", menu.Salad);
            //AddSection(result, "Fleisch", menu.MeatMains);
            //AddSection(result, "Vegetarisch", menu.VegetarianMains);
            //AddSection(result, "Dessert", menu.Dessert);

            result.Status = result.Sections.Sum(s => s.Items.Count) > 0 ? "OK" : "NO_DATA";
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