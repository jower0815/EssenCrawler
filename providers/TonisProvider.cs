using EssenCrawler.Core;

namespace EssenCrawler.Providers;

public class TonisProvider : IMenuProvider
{
    public string Name => "Toni's";
    private const string Url = "https://www.toni-s.at/";
    private const string Address = "Toni's Restaurant, Salzgries 6, 1010 Wien";
    private const string PdfUrl = "https://www.toni-s.at/Wochenmenue.pdf";

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

        // optional: Section-Text dazu

        return Task.FromResult(r);
    }
}