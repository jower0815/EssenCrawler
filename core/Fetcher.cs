using System.Net.Http;

namespace EssenCrawler.Core;

public class Fetcher
{
    private readonly HttpClient _client;

    public Fetcher()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("EssenCrawler/1.5");
        _client.Timeout = TimeSpan.FromSeconds(20);
    }

    public async Task<string> GetStringAsync(string url)
    {
        return await _client.GetStringAsync(url);
    }

    public async Task<byte[]> GetBytesAsync(string url)
    {
        return await _client.GetByteArrayAsync(url);
    }
}