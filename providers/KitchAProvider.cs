using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class KitchAProvider : IMenuProvider
{
    public string Name => "KitchA Sticks & Rolls";
    private const string Url = "https://kitcha.at/menu-sr/";
    private const string Address = "KitchA Sticks & Rolls, Vorlaufstraße 2, 1010 Wien";
    private const string PdfUrl = "https://kitcha.at/wp-content/uploads/2025/12/kitcha_menu_mittag_2025_v1.pdf";

    public Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var r = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = Url,
            EmbedUrl = PdfUrl,
            EmbedType = "pdf",
            Status = "OK",
            Address = Address,
            ID = "Kitcha"
        };

        // optional: Section-Text dazu
        return Task.FromResult(r);
    }
}