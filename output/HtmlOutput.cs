using System.Text;
using EssenCrawler.Core;

namespace EssenCrawler.Output;

public static class HtmlOutput
{
    public static string BuildPage(
        DateTime date,
        IEnumerable<MenuResult> results,
        string version = "1.5"
    )
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='de'>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8' />");
        sb.AppendLine("<title>Mittagsmenüs</title>");
        sb.AppendLine("<link rel='icon' type='image/png' href='steak.png'>");
        sb.AppendLine("<link rel='stylesheet' href='style.css' type='text/css' media='screen' />");

        sb.AppendLine("</head>");
        sb.AppendLine("<body>");


        sb.AppendLine("<div class='fixed-header'>");
        sb.AppendLine("<a href='#' class='up-button'>🔼</a>");
        sb.AppendLine($"<h1>Mittagsmenüs für  – {date:dddd, dd.MMMM.yyyy}</h1>");
        sb.AppendLine("<b><a href='Allergene.pdf' style='color:White;' target='_blank'>Allergene</a></b>");
        sb.AppendLine("</div>");

        foreach (var r in results)
        {
            sb.AppendLine(BuildRestaurantCard(r));
        }

        sb.AppendLine("<script src='script.js'></script>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }


    private static string BuildRestaurantCard(MenuResult r)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(r.ID))
            sb.AppendLine($"<div id='{r.ID}' class='menu-card'>");
        else
            sb.AppendLine($"<div id='{r.Restaurant}' class='menu-card'>");
        sb.AppendLine($"<h2><a href='{r.Source}' target='_blank'>{Escape(r.Restaurant)}</a></h2>");

        if (r.Status == "OK")
        {
            if (!string.IsNullOrWhiteSpace(r.PriceInfo))
                sb.AppendLine($"<div class='price'>{Escape(r.PriceInfo)}</div>");

            foreach (var section in r.Sections)
            {
                if (!string.IsNullOrWhiteSpace(section.Title))
                    sb.AppendLine($"<h3 class='section-title'>{Escape(section.Title)}</h3>");

                sb.AppendLine("<ul>");
                foreach (var item in section.Items)
                    sb.AppendLine($"<li>{Escape(item)}</li>");
                sb.AppendLine("</ul>");
            }

            
        }
        else if (r.Status == "NO_DATA")
        {
            sb.AppendLine("<div class='nodata'>Kein Menü verfügbar</div>");
            if (!string.IsNullOrWhiteSpace(r.Notes))
                sb.AppendLine($"<div class='notes'>{Escape(r.Notes)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(r.EmbedUrl) && r.EmbedType == "pdf")
        {
            sb.AppendLine("<div class='embed'>");
            sb.AppendLine($"  <iframe src='{EscapeAttr(r.EmbedUrl)}' loading='lazy'></iframe>");
            sb.AppendLine("</div>");
        }

        if (!string.IsNullOrWhiteSpace(r.ImageUrl))
        {
            sb.AppendLine("<div class='embed'>");
            sb.AppendLine($"    <img src='{EscapeAttr(r.ImageUrl)}' loading='lazy'>");
            sb.AppendLine("</div>");
        }

        if (!string.IsNullOrWhiteSpace(r.Address))
            sb.AppendLine($"<div class='address'>{Escape(r.Address)}</div>");

        sb.AppendLine("</div>");

        return sb.ToString();
    }


    private static string EscapeAttr(string input)
    {
        return System.Net.WebUtility.HtmlEncode(input);
    }

    private static string Escape(string input)
    {
        return System.Net.WebUtility.HtmlEncode(input);
    }
}
