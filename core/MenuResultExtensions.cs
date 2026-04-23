namespace EssenCrawler.Core;

public static class MenuResultExtensions
{
    public static MenuSection GetOrAddSection(this MenuResult r, string title)
    {
        var s = r.Sections.FirstOrDefault(x => x.Title == title);
        if (s == null)
        {
            s = new MenuSection { Title = title };
            r.Sections.Add(s);
        }
        return s;
    }
}