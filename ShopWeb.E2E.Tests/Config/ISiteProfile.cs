namespace ShopWeb.E2E.Tests.Config;

public interface ISiteProfile
{
    string SiteId { get; }
    string Name { get; }
    string BaseUrl { get; }
    string CurrencySymbol { get; }
}

public class SiteAProfile : ISiteProfile
{
    public string SiteId => "A";
    public string Name => "DemoBlaze (Site A)";
    public string BaseUrl => "https://www.demoblaze.com/";
    public string CurrencySymbol => "$";
}

public class SiteBProfile : ISiteProfile
{
    public string SiteId => "B";
    public string Name => "Alternative Shop (Site B)";
    public string BaseUrl => "https://www.saucedemo.com/";
    public string CurrencySymbol => "$";
}

public static class SiteRegistry
{
    private static readonly IReadOnlyDictionary<string, Func<ISiteProfile>> _siteMap = 
        new Dictionary<string, Func<ISiteProfile>>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = () => new SiteAProfile(),
            ["B"] = () => new SiteBProfile()
        };

    public static ISiteProfile GetProfile(string siteId) =>
        _siteMap.TryGetValue(siteId, out var factory) 
            ? factory() 
            : throw new ArgumentException($"Unknown site ID: {siteId}");
}