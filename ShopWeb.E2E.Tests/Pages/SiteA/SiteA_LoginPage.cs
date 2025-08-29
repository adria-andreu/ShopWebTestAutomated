using Microsoft.Playwright;

namespace ShopWeb.E2E.Tests.Pages.SiteA;

public class SiteA_LoginPage : ILoginPage
{
    private readonly IPage _page;

    public SiteA_LoginPage(IPage page)
    {
        _page = page;
    }

    private ILocator UsernameField => _page.Locator("#loginusername");
    private ILocator PasswordField => _page.Locator("#loginpassword");
    private ILocator LoginButton => _page.Locator("button:has-text('Log in')");
    private ILocator CloseButton => _page.Locator("#logInModal .close");
    private ILocator SignUpLink => _page.Locator("#signin2");

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

        // Wait for modal to close or error to appear
        await Task.Delay(2000);
        
        // Check if login was successful by looking for the modal to disappear
        var isModalVisible = await _page.Locator("#logInModal").IsVisibleAsync();
        
        if (!isModalVisible)
        {
            // Login successful, modal closed
            return new SiteA_HomePage(_page);
        }
        else
        {
            // Login failed, modal still visible
            throw new Exception("Login failed - invalid credentials");
        }
    }

    public async Task<string> GetErrorMessageAsync()
    {
        // In demoblaze, errors are shown as alerts, not as text elements
        // This would require setting up dialog handling in the calling test
        return "Check browser alerts for error messages";
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

    public async Task<string> GetErrorMessageAsync()
    {
        // In demoblaze, errors are shown as alerts
        return "Check browser alerts for error messages";
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