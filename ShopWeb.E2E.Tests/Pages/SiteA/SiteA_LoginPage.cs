using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteA;

public class SiteA_LoginPage : ILoginPage
{
    private readonly IPage _page;

    public SiteA_LoginPage(IPage page)
    {
        _page = page;
    }

    private ILocator UsernameField => _page.Locator("#user-name");
    private ILocator PasswordField => _page.Locator("#password");
    private ILocator LoginButton => _page.Locator("#login-button");
    private ILocator CloseButton => _page.Locator(""); // Not needed for SauceDemo
    private ILocator SignUpLink => _page.Locator(""); // SauceDemo doesn't have signup

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

        // Wait for navigation after login
        await Task.Delay(2000);

        // Check if login was successful by looking for the inventory page
        try
        {
            await _page.WaitForSelectorAsync(".inventory_list", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
            // Login successful, we're on the inventory page
            return new SiteA_HomePage(_page);
        }
        catch
        {
            // Check if there's an error message on the login page
            var errorElement = _page.Locator("[data-test='error']");
            if (await errorElement.IsVisibleAsync())
            {
                var errorMessage = await errorElement.TextContentAsync();
                throw new Exception($"Login failed: {errorMessage}");
            }

            throw new Exception("Login failed - unknown error");
        }
    }

    public Task<string> GetErrorMessageAsync()
    {
        // In demoblaze, errors are shown as alerts, not as text elements
        // This would require setting up dialog handling in the calling test
        return Task.FromResult("Check browser alerts for error messages");
    }

    public async Task<ISignUpPage> GoToSignUpAsync()
    {
        // Close login modal first
        await CloseButton.ClickAsync();
        
        // Click sign up
        await SignUpLink.ClickAsync();
        
        // Wait for sign up modal
        await _page.WaitForSelectorAsync("#signInModal", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible
        });
        
        return new SiteA_SignUpPage(_page);
    }
}

public class SiteA_SignUpPage : ISignUpPage
{
    private readonly IPage _page;

    public SiteA_SignUpPage(IPage page)
    {
        _page = page;
    }

    private ILocator UsernameField => _page.Locator("#sign-username");
    private ILocator PasswordField => _page.Locator("#sign-password");
    private ILocator SignUpButton => _page.Locator("button:has-text('Sign up')");
    private ILocator CloseButton => _page.Locator("#signInModal .close");
    private ILocator LoginLink => _page.Locator("#login2");

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

    public async Task<IHomePage> SignUpAsync(string username, string password)
    {
        await UsernameField.FillAsync(username);
        await PasswordField.FillAsync(password);
        await SignUpButton.ClickAsync();

        // Handle the alert that appears after signup
        _page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        // Wait for dialog to be handled and modal to close
        await Task.Delay(2000);
        
        return new SiteA_HomePage(_page);
    }

    public Task<string> GetErrorMessageAsync()
    {
        // In demoblaze, errors are shown as alerts
        return Task.FromResult("Check browser alerts for error messages");
    }

    public async Task<ILoginPage> GoToLoginAsync()
    {
        // Close signup modal first
        await CloseButton.ClickAsync();
        
        // Click login
        await LoginLink.ClickAsync();
        
        // Wait for login modal
        await _page.WaitForSelectorAsync("#logInModal", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible
        });
        
        return new SiteA_LoginPage(_page);
    }
}