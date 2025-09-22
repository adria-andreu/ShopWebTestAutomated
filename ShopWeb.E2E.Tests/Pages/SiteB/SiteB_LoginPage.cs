using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteB;

public class SiteB_LoginPage : ILoginPage
{
    private readonly IPage _page;

    public SiteB_LoginPage(IPage page)
    {
        _page = page;
    }

    private ILocator UsernameField => _page.Locator("[data-test='username']");
    private ILocator PasswordField => _page.Locator("[data-test='password']");
    private ILocator LoginButton => _page.Locator("[data-test='login-button']");
    private ILocator ErrorMessage => _page.Locator("[data-test='error']");

    public async Task<bool> IsLoadedAsync()
    {
        try
        {
            await UsernameField.WaitForAsync(new LocatorWaitForOptions
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

    public async Task<IHomePage> LoginAsync(string username, string password)
    {
        await UsernameField.FillAsync(username);
        await PasswordField.FillAsync(password);
        await LoginButton.ClickAsync();

        // Wait for navigation or error
        try
        {
            await _page.WaitForURLAsync("**/inventory.html", new PageWaitForURLOptions
            {
                Timeout = 10000
            });
            return new SiteB_HomePage(_page);
        }
        catch
        {
            // Check if there's an error message
            var errorText = await GetErrorMessageAsync();
            throw new Exception($"Login failed: {errorText}");
        }
    }

    public async Task<string> GetErrorMessageAsync()
    {
        try
        {
            await ErrorMessage.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 3000
            });
            return await ErrorMessage.TextContentAsync() ?? "Unknown error";
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<ISignUpPage> GoToSignUpAsync()
    {
        // SauceDemo doesn't have a signup functionality
        // This is just to comply with the interface
        await Task.CompletedTask;
        throw new NotSupportedException("SauceDemo does not support user registration");
    }
}

public class SiteB_SignUpPage : ISignUpPage
{
    private readonly IPage _page;

    public SiteB_SignUpPage(IPage page)
    {
        _page = page;
    }

    public async Task<bool> IsLoadedAsync()
    {
        // Not applicable for SauceDemo
        await Task.CompletedTask;
        return false;
    }

    public async Task<IHomePage> SignUpAsync(string username, string password)
    {
        // Not applicable for SauceDemo
        await Task.CompletedTask;
        throw new NotSupportedException("SauceDemo does not support user registration");
    }

    public async Task<string> GetErrorMessageAsync()
    {
        await Task.CompletedTask;
        return "Sign up not available in SauceDemo";
    }

    public async Task<ILoginPage> GoToLoginAsync()
    {
        // Return to login page (which is the default page)
        await _page.GotoAsync("https://www.saucedemo.com/");
        return new SiteB_LoginPage(_page);
    }
}