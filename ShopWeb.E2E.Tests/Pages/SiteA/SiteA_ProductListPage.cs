using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace ShopWeb.E2E.Tests.Pages.SiteA;

public class SiteA_ProductListPage : IProductListPage
{
    private readonly IPage _page;

    public SiteA_ProductListPage(IPage page)
    {
        _page = page;
    }

    private ILocator ProductCards => _page.Locator(".card.h-100");
    private ILocator ProductTitles => _page.Locator(".card-title a");
    private ILocator CategoryButtons => _page.Locator("a[onclick*='byCat']");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await ProductCards.First.WaitForAsync(new LocatorWaitForOptions
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
        await ProductTitles.First.WaitForAsync();
        var titles = await ProductTitles.AllTextContentsAsync();
        return titles.ToList();
    }

    public async Task<IProductDetailPage> SelectProductAsync(string productName)
    {
        var productLink = _page.Locator($"a:has-text('{productName}')").First;
        await productLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return new SiteA_ProductDetailPage(_page);
    }

    public async Task<IProductDetailPage> SelectProductByIndexAsync(int index)
    {
        var productLinks = ProductTitles;
        var count = await productLinks.CountAsync();
        
        if (index >= count || index < 0)
            throw new ArgumentOutOfRangeException($"Index {index} is out of range. Available products: {count}");
        
        await productLinks.Nth(index).ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return new SiteA_ProductDetailPage(_page);
    }

    public async Task FilterByCategoryAsync(string category)
    {
        var categorySelector = category.ToLowerInvariant() switch
        {
            "phones" => "a[onclick=\"byCat('phone')\"]",
            "laptops" => "a[onclick=\"byCat('notebook')\"]",
            "monitors" => "a[onclick=\"byCat('monitor')\"]",
            _ => throw new ArgumentException($"Unsupported category: {category}")
        };

        await _page.ClickAsync(categorySelector);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for products to update
        await Task.Delay(1000);
    }

    public async Task<int> GetProductCountAsync()
    {
        await ProductCards.First.WaitForAsync();
        return await ProductCards.CountAsync();
    }
}

public class SiteA_ProductDetailPage : IProductDetailPage
{
    private readonly IPage _page;

    public SiteA_ProductDetailPage(IPage page)
    {
        _page = page;
    }

    private ILocator ProductName => _page.Locator("h2.name");
    private ILocator ProductPrice => _page.Locator("h3.price-container");
    private ILocator ProductDescription => _page.Locator("#more");
    private ILocator AddToCartButton => _page.Locator("a[onclick='addToCart(1)']");
    private ILocator BackButton => _page.Locator("text=Home");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await ProductName.WaitForAsync(new LocatorWaitForOptions
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

    public async Task<string> GetProductNameAsync()
    {
        return await ProductName.TextContentAsync() ?? string.Empty;
    }

    public async Task<decimal> GetProductPriceAsync()
    {
        var priceText = await ProductPrice.TextContentAsync() ?? "0";
        
        // Extract price from text like "*$790 *includes tax"
        var match = Regex.Match(priceText, @"\$(\d+(?:\.\d{2})?)");
        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var price))
        {
            return price;
        }
        
        return 0;
    }

    public async Task<string> GetProductDescriptionAsync()
    {
        return await ProductDescription.TextContentAsync() ?? string.Empty;
    }

    public async Task<ICartPage> AddToCartAsync()
    {
        await AddToCartButton.ClickAsync();
        
        // Handle the alert that appears after adding to cart
        _page.Dialog += async (_, dialog) =>
        {
            if (dialog.Message.Contains("Product added"))
                await dialog.AcceptAsync();
        };
        
        // Wait for the alert to be handled
        await Task.Delay(1000);
        
        // Navigate to cart
        await _page.ClickAsync("#cartur");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        return new SiteA_CartPage(_page);
    }

    public async Task<IProductListPage> GoBackToProductsAsync()
    {
        await BackButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return new SiteA_ProductListPage(_page);
    }
}