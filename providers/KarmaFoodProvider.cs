using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class KarmaFoodProvider : IMenuProvider
{
    public string Name => "KarmaFood";

    private readonly Fetcher _fetcher;

    private const string Url =
        "https://karmafood.at/pages/wochenmenu-karma-food-wien";

    private const string Address =
        "Karma Food Laurenzerberg, Laurenzerberg 3, 1010 Wien";

    public KarmaFoodProvider(Fetcher fetcher)
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
            Address = Address
        };

        try
        {
            var html = await _fetcher.GetStringAsync(Url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Allgemeine Preisinfo
            var comboPrice = GetText(
                doc.DocumentNode.SelectSingleNode(
                    "//span[contains(@class,'menu-combo-badge__price')]"));

            var comboText = GetText(
                doc.DocumentNode.SelectSingleNode(
                    "//span[contains(@class,'menu-combo-badge__subtext')]"));

            if (!string.IsNullOrWhiteSpace(comboPrice))
            {
                result.PriceInfo = $"Lunch Combo: {comboPrice}";

                if (!string.IsNullOrWhiteSpace(comboText))
                    result.PriceInfo += $" – {comboText}";
            }

            var dayName = GetGermanDayName(date.DayOfWeek);

            if (dayName == null)
            {
                result.Status = "NO_MENU";
                return result;
            }

            var daySection = doc.DocumentNode.SelectSingleNode(
                $"//section[contains(@class,'menu-day')" +
                $" and .//h2[contains(@class,'menu-day__heading')" +
                $" and normalize-space(.)='{dayName}']]");

            if (daySection == null)
            {
                result.Status = "NO_MENU";
                return result;
            }

            var dishes = daySection.SelectNodes(
                ".//article[contains(@class,'menu-dish')]");

            if (dishes == null || dishes.Count == 0)
            {
                result.Status = "NO_MENU";
                return result;
            }

            var menuSection = result.GetOrAddSection("Hauptspeise");

            foreach (var dish in dishes)
            {
                var name = GetText(
                    dish.SelectSingleNode(
                        ".//*[contains(@class,'menu-dish__name')]"));

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var tags = dish.SelectNodes(
                                ".//*[contains(concat(' ', normalize-space(@class), ' '), ' menu-dish__tag ')]")
                            ?.Select(GetText)
                            .Where(tag => !string.IsNullOrWhiteSpace(tag))
                            .Distinct()
                            .ToList()
                            ?? new List<string>();
                var price = GetText(
                    dish.SelectSingleNode(
                        ".//*[contains(@class,'menu-dish__price')]"));

                // Im HTML stehen „Special“ und „€“ direkt nebeneinander
                price = price.Replace("Special€", "Special €");

                var item = name;

                if (tags.Count > 0)
                    item += $" ({string.Join(", ", tags)})";

                if (!string.IsNullOrWhiteSpace(price))
                    item += $" - {price}";

                menuSection.Items.Add(item);
            }

            result.Status = menuSection.Items.Count > 0
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

    private static string? GetGermanDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "Montag",
            DayOfWeek.Tuesday => "Dienstag",
            DayOfWeek.Wednesday => "Mittwoch",
            DayOfWeek.Thursday => "Donnerstag",
            DayOfWeek.Friday => "Freitag",
            _ => null
        };
    }

    private static string GetText(HtmlNode? node)
    {
        if (node == null)
            return "";

        return NormalizeWhitespace(
            HtmlEntity.DeEntitize(node.InnerText));
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
}