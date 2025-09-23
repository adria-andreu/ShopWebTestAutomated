using Microsoft.Playwright;
using ShopWeb.E2E.Tests.Pages.SiteA;

namespace ShopWeb.E2E.Tests.Pages;

public static class PageFactory
{
    public static IHomePage CreateHomePage(IPage page)
    {
        return new SiteA_HomePage(page);
    }

    public static IProductListPage CreateProductListPage(IPage page)
    {
        return new SiteA_ProductListPage(page);
    }

    public static ICartPage CreateCartPage(IPage page)
    {
        return new SiteA_CartPage(page);
    }

    public static ILoginPage CreateLoginPage(IPage page)
    {
        return new SiteA_LoginPage(page);
    }
}