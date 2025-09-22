using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteB;

public class SiteB_ProductListPage : IProductListPage
{
    private readonly IPage _page;

    public SiteB_ProductListPage(IPage page)
    {
        _page = page;
    }

    private ILocator ProductItems => _page.Locator(".inventory_item");
    private ILocator AddToCartButtons => _page.Locator("[data-test*='add-to-cart']");
    private ILocator ProductNames => _page.Locator(".inventory_item_name");
    private ILocator ProductPrices => _page.Locator(".inventory_item_price");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await ProductItems.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> GetProductNamesAsync()
    {
        var names = await ProductNames.AllTextContentsAsync();
        return names.ToList();
    }

    public async Task<IProductDetailPage> SelectProductAsync(string productName)
    {
        var productNameLocator = ProductNames.Filter(new LocatorFilterOptions { HasText = productName });
        await productNameLocator.ClickAsync();
        await _page.WaitForURLAsync("**/inventory-item.html*");
        return new SiteB_ProductDetailPage(_page);
    }

    public async Task<IProductDetailPage> SelectProductByIndexAsync(int index)
    {
        var productNameLocator = ProductNames.Nth(index);
        await productNameLocator.ClickAsync();
        await _page.WaitForURLAsync("**/inventory-item.html*");
        return new SiteB_ProductDetailPage(_page);
    }

    public async Task FilterByCategoryAsync(string category)
    {
        // SauceDemo doesn't have category filtering, just implement as no-op
        await Task.CompletedTask;
    }

    public async Task<int> GetProductCountAsync()
    {
        return await ProductItems.CountAsync();
    }
}