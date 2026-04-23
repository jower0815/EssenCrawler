namespace EssenCrawler.Core;

public class MenuResult
{
    public string Restaurant { get; set; } = "";
    public DateTime Date { get; set; }
    public string Source { get; set; } = "";

    public List<MenuSection> Sections { get; set; } = new();

    public List<string> Items { get; set; } = new();

    public string? PriceInfo { get; set; }
    public string? Notes { get; set; }
    public string? Address { get; set; }
    public string? ID {get; set;}

    // work around für PDFs
    public string? EmbedUrl { get; set; }  // z.B. PDF URL
    public string? EmbedType { get; set; } // optional: "pdf"

    // Helper for Images
    public string? ImageUrl { get; set; }
    public string? ImageTitle { get; set; }

    public string Status { get; set; } = "OK";   // OK | NO_DATA | ERROR
    public string? Error { get; set; }
}