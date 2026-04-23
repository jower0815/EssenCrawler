using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class MisoUProvider : IMenuProvider
{
    public string Name => "MisoU";
    private const string Url = "https://misou.online";
    private const string Address = "Miso U, Marc-Aurel-Straße 2a, 1010 Wien";
    private const string PdfUrl = "https://misou.online/wp-content/uploads/2025/12/LUNCH-MENU-WEEK-2.pdf#toolbar=0&navpanes=0";

    private static readonly List<string> FixedItems = new()
    {
        "Udon Tofu",
        "Korean Fried Chicken",
        "Bulgogi",
        "Sake Sushi Set",
        "Miso-U Roll",
        "Maki Mix (18pcs.)",
        "Sake Bowl"
    };
    private static readonly List<string> MenuInfo = new()
    {
      "All menus for 12 €",  
      "inc. sweet sour soup &",
      "1 pc Summer roll &",
      "homemade dessert"
    };


    public Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var r = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = Url,
            Address = Address,
            Status ="OK",
            Sections = new List<MenuSection>
            {
                new MenuSection
                {
                    Title = "Mittagsangebot",
                    Items = new List<string>(FixedItems)
                },
                new MenuSection
                {
                  Title = "Info",
                  Items = new List<string>(MenuInfo)
                }
            }
        };



        return Task.FromResult(r);
    }
}