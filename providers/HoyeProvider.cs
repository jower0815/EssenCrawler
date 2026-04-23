using System.IO.Pipelines;
using System.Windows.Markup;
using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class HoyeProvider : IMenuProvider
{
    public string Name => "Hoye";
    private const string Url = "https://example.com";
    private const string Address = "Hoye, Vorlaufstraße 5, 1010 Wien";

    private static readonly List<string> FixedItems = new()
    {
        "M1 Black Pepper Beef 12,50-",
        "M2 Sweet Sour Chicken 11,50,-",
        "M3 Lemongrass Chicken Bowl 12,-",
        "M4 Veggie Bowl 10,50",
        "M5 Slamon Teriyaki Bowl 12,50",
        "M6 Crispy Duck Bowl with Hoye Sauce 12,90-",
        "M7 HomeMade Cantonees Fried Pho Noodles 11,50-",
        "M8 Egg fried rice with chicken & veggie 11,50-"
    };

    public Task<MenuResult> GetMenuAsync(DateTime date)
    {
        
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Address = Address,
            Source = "",
            Status ="OK",
            Sections = new List<MenuSection>
            {
                new MenuSection
                {
                    Title = "Mittagsangebot",
                    Items = new List<string>(FixedItems)
                }
            }
        };
        

        return Task.FromResult(result);
    }
}