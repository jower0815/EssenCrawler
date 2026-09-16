using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class DolcePensieroProvider : IMenuProvider
{
    public string Name => "Dolce Pensiero";
    private readonly Fetcher _fetcher;

    private const string ApiUrl =
        "https://api.dolcepensiero.at/api/v1/menu/menu-der-woche/?fields=name,slug,tree";

    private const string Address = "Dolce Pensiero, Salzgries 9b, 1010 Wien";

    public DolcePensieroProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://dolcepensiero.at/menu/menu-der-woche",
            Address = Address
        };

        try
        {
            var json = await _fetcher.GetStringAsync(ApiUrl);

            var response = JsonSerializer.Deserialize<TreeResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var dayNodes = response?.Tree?.FirstOrDefault()?.Children ?? new List<TreeNode>();

            // "Jeden Tag" gilt immer, unabhängig vom Wochentag
            var everyDay = dayNodes.FirstOrDefault(n => n.Data.Name == "Jeden Tag");
            if (everyDay != null && everyDay.Dishes.Count > 0)
            {
                var section = result.GetOrAddSection("Jeden Tag");
                foreach (var dish in everyDay.Dishes)
                    section.Items.Add(FormatDish(dish));
            }

            var weekday = date.ToString("dddd", new CultureInfo("de-AT"));

            // z.B. "Montag 24.8.2026" -> per StartsWith matchen, damit das genaue Datumsformat egal ist
            var dayNode = dayNodes.FirstOrDefault(n =>
                n.Data.Name.StartsWith(weekday, StringComparison.OrdinalIgnoreCase));

            if (dayNode != null && dayNode.Dishes.Count > 0)
            {
                var section = result.GetOrAddSection("Tagesgerichte");
                foreach (var dish in dayNode.Dishes)
                    section.Items.Add(FormatDish(dish));
            }

            if (result.Sections.Count == 0)
            {
                result.Status = "NO_DATA";
                result.Notes = $"Kein Menü für '{weekday}' gefunden.";
            }
            else
            {
                result.Status = "OK";
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

    private static string FormatDish(Dish dish)
    {
        var price = NormalizePrice(dish.Price);
        return string.IsNullOrWhiteSpace(price) ? dish.Title : $"{dish.Title} - {price}";
    }

    private static string NormalizePrice(string? price)
    {
        if (string.IsNullOrWhiteSpace(price))
            return "";

        if (decimal.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return $"{value.ToString("0.00", CultureInfo.GetCultureInfo("de-AT"))} €";

        return $"{price} €";
    }

    private sealed class TreeResponse
    {
        [JsonPropertyName("tree")]
        public List<TreeNode> Tree { get; set; } = new();
    }

    private sealed class TreeNode
    {
        [JsonPropertyName("data")]
        public TreeNodeData Data { get; set; } = new();

        [JsonPropertyName("children")]
        public List<TreeNode> Children { get; set; } = new();

        [JsonPropertyName("dishes")]
        public List<Dish> Dishes { get; set; } = new();
    }

    private sealed class TreeNodeData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    private sealed class Dish
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("price")]
        public string? Price { get; set; }
    }
}
