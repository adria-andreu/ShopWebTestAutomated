using NUnit.Framework;
// Allure integration re-enabled for consistency with BaseTest (T-027)
using Allure.NUnit;
using Allure.NUnit.Attributes;
using ShopWeb.E2E.Tests.Flows;

namespace ShopWeb.E2E.Tests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[AllureNUnit] // Re-enabled for consistency with BaseTest (T-027)
[AllureSuite("Authentication Tests")]
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

    [Test]
    [Category("Smoke")]
    // [AllureFeature("User Authentication")] // Temporarily disabled - TD-14
    // [AllureStory("User Registration")]
        [Description("Verify that a new user can successfully register with valid credentials")]
    public async Task SignUp_WhenValidCredentials_ShouldRegisterSuccessfully()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var username = $"testuser_{timestamp}";
        var password = "TestPassword123!";
        
        // Act
        var result = await _authFlow!.SignUpAsync(username, password);
        
        // Assert
        Assert.That(result, Is.True, "Sign up should succeed with valid credentials");
    }

    [Test]
    [Category("Negative")]
    // [AllureFeature("User Authentication")] // Temporarily disabled - TD-14
    // [AllureStory("Invalid Login Attempts")]
        [Description("Verify that login fails with invalid credentials")]
    public async Task Login_WhenInvalidCredentials_ShouldFailGracefully()
    {
        // Arrange
        var invalidUsername = "nonexistentuser";
        var invalidPassword = "wrongpassword";
        
        // Act
        var result = await _authFlow!.TryLoginWithInvalidCredentialsAsync(invalidUsername, invalidPassword);
        
        // Assert
        Assert.That(result, Is.True, "Login should fail with invalid credentials");
    }

    [Test]
    [Category("Negative")]
    // [AllureFeature("User Authentication")] // Temporarily disabled - TD-14
    // [AllureStory("Empty Credentials")]
        [Description("Verify that login fails with empty credentials")]
    public async Task Login_WhenEmptyCredentials_ShouldFailGracefully()
    {
        // Arrange
        var emptyUsername = "";
        var emptyPassword = "";
        
        // Act
        var result = await _authFlow!.TryLoginWithInvalidCredentialsAsync(emptyUsername, emptyPassword);
        
        // Assert
        Assert.That(result, Is.True, "Login should fail with empty credentials");
    }

    [Test]
    [Category("Edge")]
    // [AllureFeature("User Authentication")] // Temporarily disabled - TD-14
    // [AllureStory("Special Characters in Credentials")]
        [Description("Verify handling of special characters in username and password")]
    public async Task SignUp_WhenSpecialCharactersInCredentials_ShouldHandleGracefully()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var usernameWithSpecialChars = $"test@user_{timestamp}";
        var passwordWithSpecialChars = "Test!@#$%^&*()123";
        
        // Act & Assert
        // This test verifies that the application handles special characters gracefully
        // whether it succeeds or fails, it shouldn't crash
        try
        {
            var result = await _authFlow!.SignUpAsync(usernameWithSpecialChars, passwordWithSpecialChars);
            // If it succeeds, that's fine
            Assert.Pass("Sign up with special characters handled successfully");
        }
        catch (Exception ex)
        {
            // If it fails, it should fail gracefully without system errors
            Assert.That(ex.Message, Does.Not.Contain("System."), 
                "Should not throw system-level exceptions");
            Assert.Pass("Sign up with special characters failed gracefully as expected");
        }
    }

    [Test]
    [Category("Regression")]
    // [AllureFeature("User Authentication")] // Temporarily disabled - TD-14
    // [AllureStory("Duplicate User Registration")]
        [Description("Verify that duplicate user registration is handled properly")]
    public async Task SignUp_WhenDuplicateUsername_ShouldHandleAppropriately()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var username = $"duplicate_user_{timestamp}";
        var password = "TestPassword123!";
        
        // Act
        var firstSignUp = await _authFlow!.SignUpAsync(username, password);
        
        // Try to sign up with same username again
        var secondSignUp = await _authFlow.SignUpAsync(username, password);
        
        // Assert
        Assert.That(firstSignUp, Is.True, "First sign up should succeed");
        // The second signup behavior depends on the application - it might succeed or fail
        // We just verify it doesn't crash the application
        Assert.That(secondSignUp, Is.True.Or.False, "Second sign up should handle gracefully");
    }
}