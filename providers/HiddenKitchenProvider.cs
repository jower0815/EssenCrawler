using EssenCrawler.Core;
using HtmlAgilityPack;
using UglyToad.PdfPig.Fonts.Type1;

namespace EssenCrawler.Providers;

public class HiddenKitchenProvider : IMenuProvider
{
    public string Name => "Hiddenkitchen";
    private const string Url = "https://www.hiddenkitchen.at/city";
    private const string Address = "hiddenkitchen, Färbergasse 3, 1010 Wien";
    private const string PdfUrl = "https://www.hiddenkitchen.at/s/City.pdf#toolbar=0&navpanes=0";


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
            Address = Address
        };

        return Task.FromResult(r);
    }
}