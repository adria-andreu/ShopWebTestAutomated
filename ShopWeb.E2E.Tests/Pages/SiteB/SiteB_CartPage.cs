using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteB;

public class SiteB_CartPage : ICartPage
{
    private readonly IPage _page;

    public SiteB_CartPage(IPage page)
    {
        _page = page;
    }

    private ILocator CartItems => _page.Locator(".cart_item");
    private ILocator CartItemNames => _page.Locator(".inventory_item_name");
    private ILocator CartItemPrices => _page.Locator(".inventory_item_price");
    private ILocator RemoveButtons => _page.Locator("[data-test*='remove']");
    private ILocator CheckoutButton => _page.Locator("[data-test='checkout']");
    private ILocator ContinueShoppingButton => _page.Locator("[data-test='continue-shopping']");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await _page.WaitForURLAsync("**/cart.html", new PageWaitForURLOptions
            {
                Timeout = 10000
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var cartItems = new List<CartItem>();
        var itemCount = await CartItems.CountAsync();

        for (int i = 0; i < itemCount; i++)
        {
            var nameElement = CartItemNames.Nth(i);
            var priceElement = CartItemPrices.Nth(i);

            var name = await nameElement.TextContentAsync() ?? "";
            var priceText = await priceElement.TextContentAsync() ?? "0";
            var cleanPrice = priceText.Replace("$", "").Trim();

            if (decimal.TryParse(cleanPrice, out var price))
            {
                cartItems.Add(new CartItem
                {
                    Name = name,
                    Price = price,
                    Quantity = 1 // SauceDemo always shows quantity 1 per item
                });
            }
        }

        return cartItems;
    }

    public async Task RemoveItemAsync(string productName)
    {
        var cartItem = CartItems.Filter(new LocatorFilterOptions
        {
            Has = _page.Locator(".inventory_item_name").Filter(new LocatorFilterOptions { HasText = productName })
        });

        var removeButton = cartItem.Locator("[data-test*='remove']");
        await removeButton.ClickAsync();

        // Wait for the item to be removed
        await Task.Delay(500);
    }


    public async Task<decimal> GetTotalPriceAsync()
    {
        var cartItems = await GetCartItemsAsync();
        return cartItems.Sum(item => item.Price * item.Quantity);
    }

    public async Task<ICheckoutPage> ProceedToCheckoutAsync()
    {
        await CheckoutButton.ClickAsync();
        await _page.WaitForURLAsync("**/checkout-step-one.html");
        return new SiteB_CheckoutPage(_page);
    }

    public async Task ClearCartAsync()
    {
        var cartItems = await GetCartItemsAsync();

        // Remove all items one by one
        foreach (var item in cartItems)
        {
            await RemoveItemAsync(item.Name);
        }
    }

    public async Task<bool> IsEmptyAsync()
    {
        var itemCount = await CartItems.CountAsync();
        return itemCount == 0;
    }
}