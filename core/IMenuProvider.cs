namespace EssenCrawler.Core;

public interface IMenuProvider
{
    string Name { get; }
    Task<MenuResult> GetMenuAsync(DateTime date);
}