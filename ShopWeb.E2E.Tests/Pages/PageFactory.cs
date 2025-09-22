using Microsoft.Playwright;
using ShopWeb.E2E.Tests.Pages.SiteA;
using ShopWeb.E2E.Tests.Pages.SiteB;
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
            "B" => new SiteB_HomePage(page),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }

    public static IProductListPage CreateProductListPage(IPage page)
    {
        var settings = ConfigurationManager.TestSettings;
        return settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_ProductListPage(page),
            "B" => new SiteB_ProductListPage(page),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }

    public static ICartPage CreateCartPage(IPage page)
    {
        var settings = ConfigurationManager.TestSettings;
        return settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_CartPage(page),
            "B" => new SiteB_CartPage(page),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }

    public static ILoginPage CreateLoginPage(IPage page)
    {
        var settings = ConfigurationManager.TestSettings;
        return settings.SiteId.ToUpperInvariant() switch
        {
            "A" => new SiteA_LoginPage(page),
            "B" => new SiteB_LoginPage(page),
            _ => throw new ArgumentException($"Unsupported SiteId: {settings.SiteId}")
        };
    }
}