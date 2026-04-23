using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class IKOProvider : IMenuProvider
{
    public string Name => "IKO";
    private const string Url = "https://iko.wien/";
    private const string Address = "IKO Kitchen & BAR, Wipplingerstraße 6, 1010 Wien";
    private const string PdfUrl = "https://iko.wien/wp-content/uploads/dd/DDIKO.pdf";

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