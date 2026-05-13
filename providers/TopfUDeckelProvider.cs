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

            foreach (var MenuCard in MenuSections)
            {
                switch(MenuCard.InnerText)
                {
                    case var naming when naming.StartsWith("Starters"):
                    {
                        var EssensItems = MenuCard.SelectNodes(".//h3");
                        foreach (var EssensItem in EssensItems)
                        {
                            var CleanedItem = WebUtility.HtmlDecode(EssensItem.InnerText);
                            starters.Items.Add(CleanedItem);
                        }
                        break;
                    }
                    case var naming when naming.StartsWith("Main"):
                    {
                        var EssensItems = MenuCard.SelectNodes(".//h3");
                        foreach (var EssensItem in EssensItems)
                        {
                            var CleanedItem = WebUtility.HtmlDecode(EssensItem.InnerText);
                            main.Items.Add(CleanedItem);
                        }
                        break;
                    }
                    case var naming when naming.StartsWith("Dessert"):
                    {
                        var EssensItems = MenuCard.SelectNodes(".//h3");
                        foreach (var EssensItem in EssensItems)
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

    private static void AddSection(MenuResult result, string title, List<TopfItem>? items)
    {
        if (items == null || items.Count == 0)
            return;

        var section = new MenuSection { Title = title };

        foreach (var item in items)
        {
            if (item == null)
                continue;

            var name = item.Name?.Trim() ?? "";
            var description = item.Description?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
                continue;

            var line = name;

            if (!string.IsNullOrWhiteSpace(description))
                line = string.IsNullOrWhiteSpace(line) ? description : $"{line} — {description}";

            section.Items.Add(line);
        }

        if (section.Items.Count > 0)
            result.Sections.Add(section);
    }

    private class TopfMenu
    {
        [JsonConverter(typeof(SingleOrArrayConverter<TopfItem>))]
        public List<TopfItem>? Starters { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<TopfItem>))]
        public List<TopfItem>? Salad { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<TopfItem>))]
        public List<TopfItem>? MeatMains { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<TopfItem>))]
        public List<TopfItem>? VegetarianMains { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<TopfItem>))]
        public List<TopfItem>? Dessert { get; set; }
    }

    private class TopfItem
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? Dietary { get; set; }
    }

    private class SingleOrArrayConverter<T> : JsonConverter<List<T>>
    {
        public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return JsonSerializer.Deserialize<List<T>>(ref reader, options);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var item = JsonSerializer.Deserialize<T>(ref reader, options);
                return item != null ? new List<T> { item } : new List<T>();
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<T>();
            }

            throw new JsonException($"Unexpected token {reader.TokenType} while parsing {typeof(T).Name} list.");
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}