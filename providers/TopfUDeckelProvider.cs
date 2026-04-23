using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using EssenCrawler.Core;

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

            var scripts = doc.DocumentNode.SelectNodes("//script");
            if (scripts == null || scripts.Count == 0)
            {
                result.Status = "NO_DATA";
                result.Notes = "Keine Script-Tags gefunden.";
                return result;
            }

            var scriptNode = scripts.FirstOrDefault(s =>
                (s.InnerText ?? "").Contains("todaysMenu", StringComparison.OrdinalIgnoreCase));

            if (scriptNode == null)
            {
                result.Status = "NO_DATA";
                result.Notes = "Kein Script mit 'todaysMenu' gefunden.";
                return result;
            }

            var js = scriptNode.InnerText ?? "";

            js = Regex.Replace(js, @";.*", "", RegexOptions.Singleline);
            js = Regex.Replace(js, @"const\s+todaysMenu\s*=\s*", "");
            js = Regex.Replace(js, @"^\s*\{\s*// Dietary info[\s\S]*?starters:", "{ \"starters\":");
            js = Regex.Replace(js, @"\r?\n", " ");
            js = Regex.Replace(js, @"(\s*)(\w+)\s*:", "$1\"$2\":");
            js = Regex.Replace(js, @",(\s*[\]}])", "$1");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var menu = JsonSerializer.Deserialize<TopfMenu>(js, options);

            if (menu == null)
            {
                result.Status = "ERROR";
                result.Error = "JSON konnte nicht deserialisiert werden.";
                return result;
            }

            AddSection(result, "Vorspeise", menu.Starters);
            AddSection(result, "Salat", menu.Salad);
            AddSection(result, "Fleisch", menu.MeatMains);
            AddSection(result, "Vegetarisch", menu.VegetarianMains);
            AddSection(result, "Dessert", menu.Dessert);

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