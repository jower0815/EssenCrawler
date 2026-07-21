using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class PhoLinhProvider : IMenuProvider
{
    public string Name => "Pho Linh";

    private readonly Fetcher _fetcher;

    private const string ApiUrl =
        "https://admin.pholinh.at/pho-linh-cms/items/lunch_menu";

    private const string SourceUrl =
        "https://www.pholinh.at/";

    private const string Address =
        "Pho Linh, Fleischmarkt 16 im Hof, 1010 Wien";

    public PhoLinhProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = SourceUrl,
            Address = Address
        };

        try
        {
            var json = await _fetcher.GetStringAsync(ApiUrl);

            var response = JsonSerializer.Deserialize<PhoLinhResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var lunchMenu = response?.Data?.FirstOrDefault();

            if (lunchMenu == null || lunchMenu.Menu.Count == 0)
            {
                result.Status = "NO_MENU";
                return result;
            }

            var soups = new HashSet<string>();

            foreach (var menu in lunchMenu.Menu)
            {
                var lines = ExtractParagraphs(menu.Name);

                // Aktuell:
                // 0 = Suppe
                // 1 = deutsche Hauptspeise
                // 2 = englische Übersetzung
                var soup = lines.ElementAtOrDefault(0);
                var mainCourse = lines.ElementAtOrDefault(1);

                if (!string.IsNullOrWhiteSpace(soup))
                    soups.Add(soup);

                if (!string.IsNullOrWhiteSpace(mainCourse))
                {
                    var section = result.GetOrAddSection("Hauptspeise");
                    var price = NormalizePrice(menu.Price);

                    var text = $"{menu.Id}: {mainCourse}";

                    if (!string.IsNullOrWhiteSpace(price))
                        text += $" - {price}";

                    section.Items.Add(text);
                }
            }

            if (soups.Count > 0)
            {
                var soupSection = result.GetOrAddSection("Vorspeise");

                foreach (var soup in soups)
                    soupSection.Items.Add(soup);
            }

            // Vorspeise vor der Hauptspeise anzeigen
            result.Sections = result.Sections
                .OrderBy(section => section.Title == "Vorspeise" ? 0 : 1)
                .ToList();

            var prices = lunchMenu.Menu
                .Select(menu => NormalizePrice(menu.Price))
                .Where(price => !string.IsNullOrWhiteSpace(price))
                .Distinct()
                .ToList();

            if (prices.Count == 1)
                result.PriceInfo = $"Menüpreis: {prices[0]}";

            result.Status = result.Sections.Count > 0
                ? "OK"
                : "NO_MENU";

            return result;
        }
        catch (Exception ex)
        {
            result.Status = "ERROR";
            result.Error = ex.Message;
            return result;
        }
    }

    private static List<string> ExtractParagraphs(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return new List<string>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var paragraphs = doc.DocumentNode.SelectNodes("//p");

        if (paragraphs == null)
        {
            var text = NormalizeWhitespace(
                HtmlEntity.DeEntitize(doc.DocumentNode.InnerText));

            return string.IsNullOrWhiteSpace(text)
                ? new List<string>()
                : new List<string> { text };
        }

        return paragraphs
            .Select(node => NormalizeWhitespace(
                HtmlEntity.DeEntitize(node.InnerText)))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();
    }

    private static string NormalizePrice(string? price)
    {
        if (string.IsNullOrWhiteSpace(price))
            return "";

        var normalized = price.Trim().Replace(',', '.');

        if (decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return $"{value.ToString("0.00", CultureInfo.GetCultureInfo("de-AT"))} €";
        }

        return $"{price.Trim()} €";
    }

    private static string NormalizeWhitespace(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        return string.Join(
            " ",
            input.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries))
            .Trim();
    }

    private sealed class PhoLinhResponse
    {
        [JsonPropertyName("data")]
        public List<PhoLinhLunchMenu> Data { get; set; } = new();
    }

    private sealed class PhoLinhLunchMenu
    {
        [JsonPropertyName("menu")]
        public List<PhoLinhMenuItem> Menu { get; set; } = new();

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    private sealed class PhoLinhMenuItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}