using EssenCrawler.Core;
using HtmlAgilityPack;

namespace EssenCrawler.Providers;

public class SparkysProvider : IMenuProvider
{
    public string Name => "Sparkys";
    private readonly Fetcher _fetcher; 
    private const string Url = "https://www.sparkys.at/startseite";

    private const string Address = "Sparky’s unlimited, Goldschmiedgasse 8, 1010 Wien";

    public SparkysProvider(Fetcher fetcher)
    {
        _fetcher = fetcher;
    }

    public async Task<MenuResult> GetMenuAsync(DateTime date)
    {
        var result = new MenuResult
        {
            Restaurant = Name,
            Date = date.Date,
            Source = "https://www.sparkys.at/startseite",
            Address = Address,
            ImageUrl = ""
        };

        try {
            //var html = await _fetcher.GetStringAsync(Url);

            //var doc = new HtmlDocument();
            //doc.LoadHtml(html);
            //var img = doc.DocumentNode.SelectSingleNode("//img[@id='img_comp-l2cdzq1o']");

            
            //result.ImageUrl = "https://lh3.googleusercontent.com/sitesv/AA5AbUAbnkiQqrYw8tZuWdosHd223vs_klkXIo8l9blw41b-bXRdFiR-kLnA5sjYU5u4AH0MwQf3AkjI89lQpSxl8wt2nmiMYAmw-fCmvQYMiUQ7rsdiSP1JbtJEdUoXefG9fakYTCvoK34I5_LH-1EG4BkOz4PIywSQUAcmWT-ZYePHEUxEfO9efK6MiW5B9PUJ9R1PDRigNqCjdEOB8VZ4D2rKvJu3l0wuEqifGqMH=w1280";
            result.ImageUrl = "https://lh3.googleusercontent.com/sitesv/AA5AbUCJbxwUfeP7iKXbB1aqDA3Gtcb35PhSKm9DboLvjFzFanv2KyDkZZMVKrKJAZphhg8VF92qFwOEukJrqMunA-s3UtHduNN2sB-ABp_JKMGdVRsr0QWeM46l0f2WrZb3zr9AoQk2Pcd5Njo2DR_oaltDcT_pOZfVgB0F_xbP58qD8qy6g0Ix2vEZ7opX27IYEMfqTW_hJfD5OGPLFOSVCwI192wFVrhFL7dqPsFP=w1280";


            return result;

        }
        catch (Exception ex)
        {
            result.Status = "ERROR";
            result.Error = ex.Message;
            return result;
        }
    }
}