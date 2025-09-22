using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace ShopWeb.E2E.Tests.Pages.SiteA;

public class SiteA_CartPage : ICartPage
{
    private readonly IPage _page;

    public SiteA_CartPage(IPage page)
    {
        _page = page;
    }

    private ILocator CartTable => _page.Locator("#tbodyid");
    private ILocator CartRows => CartTable.Locator("tr");
    private ILocator TotalPrice => _page.Locator("#totalp");
    private ILocator PlaceOrderButton => _page.Locator("button:has-text('Place Order')");
    private ILocator DeleteButtons => _page.Locator("a:has-text('Delete')");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await CartTable.WaitForAsync(new LocatorWaitForOptions
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

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var items = new List<CartItem>();
        var rowCount = await CartRows.CountAsync();

        for (int i = 0; i < rowCount; i++)
        {
            var row = CartRows.Nth(i);
            var cells = row.Locator("td");
            
            if (await cells.CountAsync() >= 3)
            {
                var name = await cells.Nth(1).TextContentAsync() ?? string.Empty;
                var priceText = await cells.Nth(2).TextContentAsync() ?? "0";
                
                if (decimal.TryParse(priceText, out var price))
                {
                    items.Add(new CartItem
                    {
                        Name = name.Trim(),
                        Price = price,
                        Quantity = 1 // Demoblaze doesn't show quantity, assuming 1
                    });
                }
            }
        }

        return items;
    }

    public async Task<decimal> GetTotalPriceAsync()
    {
        var totalText = await TotalPrice.TextContentAsync() ?? "0";
        if (decimal.TryParse(totalText, out var total))
            return total;
        return 0;
    }

    public async Task RemoveItemAsync(string productName)
    {
        // Find the row containing the product and click its delete button
        var rows = await CartRows.CountAsync();
        
        for (int i = 0; i < rows; i++)
        {
            var row = CartRows.Nth(i);
            var nameCell = row.Locator("td").Nth(1);
            var name = await nameCell.TextContentAsync();
            
            if (name?.Contains(productName, StringComparison.OrdinalIgnoreCase) == true)
            {
                var deleteButton = row.Locator("a:has-text('Delete')");
                await deleteButton.ClickAsync();
                await Task.Delay(1000); // Wait for removal to complete
                break;
            }
        }
    }

    public async Task<ICheckoutPage> ProceedToCheckoutAsync()
    {
        await PlaceOrderButton.ClickAsync();
        
        // Wait for checkout modal to appear
        await _page.WaitForSelectorAsync("#orderModal", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible
        });
        
        return new SiteA_CheckoutPage(_page);
    }

    public async Task ClearCartAsync()
    {
        var items = await GetCartItemsAsync();
        
        foreach (var item in items)
        {
            await RemoveItemAsync(item.Name);
        }
    }

    public async Task<bool> IsEmptyAsync()
    {
        var items = await GetCartItemsAsync();
        return items.Count == 0;
    }
}

public class SiteA_CheckoutPage : ICheckoutPage
{
    private readonly IPage _page;

    public SiteA_CheckoutPage(IPage page)
    {
        _page = page;
    }

    private ILocator NameField => _page.Locator("#name");
    private ILocator CountryField => _page.Locator("#country");
    private ILocator CityField => _page.Locator("#city");
    private ILocator CreditCardField => _page.Locator("#card");
    private ILocator MonthField => _page.Locator("#month");
    private ILocator YearField => _page.Locator("#year");
    private ILocator PurchaseButton => _page.Locator("button:has-text('Purchase')");
    private ILocator TotalAmount => _page.Locator("#totalm");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await NameField.WaitForAsync(new LocatorWaitForOptions
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

    public async Task FillShippingInfoAsync(ShippingInfo shippingInfo)
    {
        await NameField.FillAsync(shippingInfo.Name);
        await CountryField.FillAsync(shippingInfo.Country);
        await CityField.FillAsync(shippingInfo.City);
        await CreditCardField.FillAsync(shippingInfo.CreditCard);
        await MonthField.FillAsync(shippingInfo.Month);
        await YearField.FillAsync(shippingInfo.Year);
    }

    public async Task<IOrderConfirmationPage> PlaceOrderAsync()
    {
        await PurchaseButton.ClickAsync();
        
        // Wait for confirmation dialog
        await _page.WaitForSelectorAsync(".sweet-alert", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible
        });
        
        return new SiteA_OrderConfirmationPage(_page);
    }

    public async Task<decimal> GetOrderTotalAsync()
    {
        var totalText = await TotalAmount.TextContentAsync() ?? "0";
        if (decimal.TryParse(totalText.Replace("Total: ", ""), out var total))
            return total;
        return 0;
    }
}

public class SiteA_OrderConfirmationPage : IOrderConfirmationPage
{
    private readonly IPage _page;

    public SiteA_OrderConfirmationPage(IPage page)
    {
        _page = page;
    }

    private ILocator ConfirmationDialog => _page.Locator(".sweet-alert");
    private ILocator ConfirmationText => ConfirmationDialog.Locator("p");
    private ILocator OkButton => _page.Locator("button:has-text('OK')");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await ConfirmationDialog.WaitForAsync(new LocatorWaitForOptions
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

    public async Task<string> GetOrderIdAsync()
    {
        var text = await ConfirmationText.TextContentAsync() ?? string.Empty;
        var match = Regex.Match(text, @"Id:\s*(\d+)");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    public async Task<decimal> GetOrderTotalAsync()
    {
        var text = await ConfirmationText.TextContentAsync() ?? string.Empty;
        var match = Regex.Match(text, @"Amount:\s*(\d+(?:\.\d{2})?)");
        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var total))
            return total;
        return 0;
    }

    public async Task<IHomePage> ReturnToHomeAsync()
    {
        await OkButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return new SiteA_HomePage(_page);
    }
}