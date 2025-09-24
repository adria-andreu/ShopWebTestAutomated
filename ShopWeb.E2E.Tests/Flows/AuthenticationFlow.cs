using Microsoft.Playwright;
using ShopWeb.E2E.Tests.Pages;
using ShopWeb.E2E.Tests.Pages.SiteA;
using ShopWeb.E2E.Tests.Config;

namespace ShopWeb.E2E.Tests.Flows;

public class AuthenticationFlow
{
    private readonly IPage _page;
    private readonly TestSettings _settings;

    public AuthenticationFlow(IPage page)
    {
        _page = page;
        _settings = ConfigurationManager.TestSettings;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var homePage = GetHomePage();
            await homePage.NavigateAsync();
            
            if (!await homePage.IsLoadedAsync())
                throw new Exception("Home page failed to load");

            var loginPage = await homePage.GoToLoginAsync();
            
            if (!await loginPage.IsLoadedAsync())
                throw new Exception("Login page failed to load");

            await loginPage.LoginAsync(username, password);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SignUpAsync(string username, string password)
    {
        try
        {
            var homePage = GetHomePage();
            await homePage.NavigateAsync();
            
            if (!await homePage.IsLoadedAsync())
                throw new Exception("Home page failed to load");

            var loginPage = await homePage.GoToLoginAsync();
            
            if (!await loginPage.IsLoadedAsync())
                throw new Exception("Login page failed to load");

            var signUpPage = await loginPage.GoToSignUpAsync();
            
            if (!await signUpPage.IsLoadedAsync())
                throw new Exception("Sign up page failed to load");

            await signUpPage.SignUpAsync(username, password);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryLoginWithInvalidCredentialsAsync(string username, string password)
    {
        try
        {
            var homePage = GetHomePage();
            await homePage.NavigateAsync();
            
            var loginPage = await homePage.GoToLoginAsync();
            
            if (!await loginPage.IsLoadedAsync())
                return false;

            // This should fail for invalid credentials
            await loginPage.LoginAsync(username, password);
            
            return false; // If we reach here, login unexpectedly succeeded
        }
        catch
        {
            return true; // Expected failure for invalid credentials
        }
    }

    private IHomePage GetHomePage()
    {
        return new SiteA_HomePage(_page);
    }

    /// <summary>
    /// T-078: Logout functionality for IT08 policy-compliant tests
    /// </summary>
    public async Task<bool> LogoutAsync()
    {
        try
        {
            // Navigate to a page that has logout functionality
            // For SauceDemo, we can use the inventory page menu
            await _page.ClickAsync("#react-burger-menu-btn");
            await _page.ClickAsync("#logout_sidebar_link");

            // Verify we're back at login page
            await _page.WaitForSelectorAsync("#user-name", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// T-078: Verify authentication state persistence for IT08 policy-compliant tests
    /// </summary>
    public async Task<bool> VerifyAuthenticationStateAsync()
    {
        try
        {
            // Navigate to a protected page and verify we're still authenticated
            await _page.GotoAsync($"{_settings.BaseUrl}/inventory.html");

            // If we can see the inventory page elements, we're authenticated
            await _page.WaitForSelectorAsync(".inventory_list", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task CleanupAsync()
    {
        // Authentication flow doesn't typically need cleanup
        // but this method is here for consistency with other flows
        await Task.CompletedTask;
    }
}