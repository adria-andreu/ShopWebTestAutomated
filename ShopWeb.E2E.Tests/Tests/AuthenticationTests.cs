using NUnit.Framework;
using ShopWeb.E2E.Tests.Flows;
using ShopWeb.E2E.Tests.Utilities;

namespace ShopWeb.E2E.Tests.Tests;

/// <summary>
/// Policy-compliant authentication tests (IT08)
/// 🔹 Iteración 8 – Creación de nuevos tests alineados con guía de UI
/// Replaces non-conforming AuthenticationTests suite eliminated in IT07
///
/// Follows E2E_Policy.md requirements:
/// - Uses Verify.* instead of Assert.*
/// - Deterministic test data via DataFactory
/// - Focused on E2E flows, not Unit/Integration logic
/// - Proper categorization and cleanup
/// - T-077: Crear nueva suite AuthenticationTests_PolicyCompliant
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AuthenticationTests : BaseTest
{
    private AuthenticationFlow? _authFlow;

    [SetUp]
    public void SetUpAuthFlow()
    {
        _authFlow = new AuthenticationFlow(Page);
    }

    [TearDown]
    public async Task CleanupAuthFlow()
    {
        if (_authFlow != null)
        {
            await _authFlow.CleanupAsync();
        }
    }

    /// <summary>
    /// IT08-T001: Verify successful login with valid credentials
    /// Category: Smoke - essential for PR validation
    /// Data: Deterministic via DataFactory
    /// Scope: E2E cross-component authentication flow
    /// </summary>
    [Test]
    [Category("Smoke")]
    [Description("Verify that a valid user can successfully log in and access the dashboard")]
    public async Task Login_WhenValidCredentials_ShouldSucceedAndRedirectToDashboard()
    {
        // Arrange
        var validUser = DataFactory.CreateValidLoginUser();

        // Act
        var loginResult = await _authFlow!.LoginAsync(validUser.Username, validUser.Password);

        // Confirm
        Verify.True(loginResult, "Login should succeed with valid credentials");
    }

    /// <summary>
    /// IT08-T002: Verify login failure with invalid credentials
    /// Category: Smoke - essential negative path validation
    /// Data: Deterministic via DataFactory
    /// Scope: E2E authentication flow validation
    /// </summary>
    [Test]
    [Category("Smoke")]
    [Description("Verify that login fails gracefully with invalid credentials")]
    public async Task Login_WhenInvalidCredentials_ShouldFailGracefully()
    {
        // Arrange
        var invalidUser = DataFactory.CreateInvalidUser();

        // Act
        var loginResult = await _authFlow!.TryLoginWithInvalidCredentialsAsync(invalidUser.Username, invalidUser.Password);

        // Confirm
        Verify.True(loginResult, "Login should fail appropriately with invalid credentials");
    }

    /// <summary>
    /// IT08-T003: Verify login behavior with empty credentials
    /// Category: CriticalPath - boundary condition testing
    /// Data: Deterministic via DataFactory
    /// Scope: E2E form validation and error handling
    /// </summary>
    [Test]
    [Category("CriticalPath")]
    [Description("Verify that login handles empty credentials appropriately")]
    public async Task Login_WhenEmptyCredentials_ShouldHandleGracefully()
    {
        // Arrange
        var emptyCredentials = DataFactory.CreateEmptyCredentials();

        // Act
        var loginResult = await _authFlow!.TryLoginWithInvalidCredentialsAsync(emptyCredentials.Username, emptyCredentials.Password);

        // Confirm
        Verify.True(loginResult, "Login should handle empty credentials gracefully");
    }

    /// <summary>
    /// IT08-T004: Verify logout functionality
    /// Category: CriticalPath - session management validation
    /// Data: Deterministic via DataFactory
    /// Scope: E2E session lifecycle management
    /// </summary>
    [Test]
    [Category("CriticalPath")]
    [Description("Verify that user can successfully log out after being logged in")]
    public async Task Logout_WhenUserIsLoggedIn_ShouldSucceedAndRedirectToLogin()
    {
        // Arrange
        var validUser = DataFactory.CreateValidLoginUser();
        await _authFlow!.LoginAsync(validUser.Username, validUser.Password);

        // Act
        var logoutResult = await _authFlow.LogoutAsync();

        // Confirm
        Verify.True(logoutResult, "Logout should succeed and redirect to login page");
    }

    /// <summary>
    /// IT08-T005: Verify authentication state persistence
    /// Category: Regression - session stability validation
    /// Data: Deterministic via DataFactory
    /// Scope: E2E session persistence across page navigation
    /// </summary>
    [Test]
    [Category("Regression")]
    [Description("Verify that authentication state persists across page navigation")]
    public async Task Authentication_WhenNavigatingBetweenPages_ShouldMaintainLoginState()
    {
        // Arrange
        var validUser = DataFactory.CreateValidLoginUser();
        await _authFlow!.LoginAsync(validUser.Username, validUser.Password);

        // Act
        var stateValid = await _authFlow.VerifyAuthenticationStateAsync();

        // Confirm
        Verify.True(stateValid, "Authentication state should persist across navigation");
    }

    /// <summary>
    /// IT08-T006: Verify locked out user handling
    /// Category: Regression - special user scenarios
    /// Data: Deterministic via DataFactory
    /// Scope: E2E error handling for locked accounts
    /// </summary>
    [Test]
    [Category("Regression")]
    [Description("Verify that locked out user receives appropriate error message")]
    public async Task Login_WhenLockedOutUser_ShouldShowLockoutMessage()
    {
        // Arrange
        var lockedUser = DataFactory.CreateLockedOutUser();

        // Act
        var loginResult = await _authFlow!.TryLoginWithInvalidCredentialsAsync(lockedUser.Username, lockedUser.Password);

        // Confirm
        Verify.True(loginResult, "Locked out user should receive appropriate error handling");
    }
}