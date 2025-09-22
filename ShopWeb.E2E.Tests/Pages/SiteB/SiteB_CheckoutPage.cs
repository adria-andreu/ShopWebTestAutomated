using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteB;

public class SiteB_CheckoutPage : ICheckoutPage
{
    private readonly IPage _page;

    public SiteB_CheckoutPage(IPage page)
    {
        _page = page;
    }

    private ILocator FirstNameField => _page.Locator("[data-test='firstName']");
    private ILocator LastNameField => _page.Locator("[data-test='lastName']");
    private ILocator PostalCodeField => _page.Locator("[data-test='postalCode']");
    private ILocator ContinueButton => _page.Locator("[data-test='continue']");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await FirstNameField.WaitForAsync(new LocatorWaitForOptions
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
        await FirstNameField.FillAsync(shippingInfo.Name.Split(' ').FirstOrDefault() ?? "");
        await LastNameField.FillAsync(string.Join(" ", shippingInfo.Name.Split(' ').Skip(1)));
        await PostalCodeField.FillAsync("12345"); // SauceDemo only needs postal code
        await ContinueButton.ClickAsync();
        await _page.WaitForURLAsync("**/checkout-step-two.html");
    }

    public async Task<IOrderConfirmationPage> PlaceOrderAsync()
    {
        var finishButton = _page.Locator("[data-test='finish']");
        await finishButton.ClickAsync();
        await _page.WaitForURLAsync("**/checkout-complete.html");
        return new SiteB_OrderConfirmationPage(_page);
    }

    public async Task<decimal> GetOrderTotalAsync()
    {
        var totalElement = _page.Locator(".summary_total_label");
        var totalText = await totalElement.TextContentAsync() ?? "0";
        var cleanTotal = totalText.Replace("Total: $", "").Trim();
        return decimal.TryParse(cleanTotal, out var total) ? total : 0m;
    }
}