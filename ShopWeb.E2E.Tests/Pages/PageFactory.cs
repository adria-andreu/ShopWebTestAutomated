using Microsoft.Playwright;
using ShopWeb.E2E.Tests.Pages.SiteA;
using ShopWeb.E2E.Tests.Config;

namespace ShopWeb.E2E.Tests.Pages;

public static class PageFactory
{
    public static IHomePage CreateHomePage(IPage page)
    {
        var settings = ConfigurationManager.TestSettings;
        return settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_HomePage(page),
            "B" => throw new NotImplementedException("Site B implementation not available yet"),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }

    public static IProductListPage CreateProductListPage(IPage page)
    {
        var settings = ConfigurationManager.TestSettings;
        return settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_ProductListPage(page),
            "B" => throw new NotImplementedException("Site B implementation not available yet"),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }

    public static ICartPage CreateCartPage(IPage page)
    {
        var settings = ConfigurationManager.TestSettings;
        return settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_CartPage(page),
            "B" => throw new NotImplementedException("Site B implementation not available yet"),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }

    public static ILoginPage CreateLoginPage(IPage page)
    {
        var settings = ConfigurationManager.TestSettings;
        return settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_LoginPage(page),
            "B" => throw new NotImplementedException("Site B implementation not available yet"),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }
}