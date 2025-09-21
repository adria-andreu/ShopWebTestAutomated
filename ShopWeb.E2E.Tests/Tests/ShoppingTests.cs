using NUnit.Framework;
// Allure integration disabled to unblock IT03 - complex context management issues in CI/CD
// using Allure.NUnit;
// using Allure.NUnit.Attributes;
using ShopWeb.E2E.Tests.Flows;
using ShopWeb.E2E.Tests.Pages;

namespace ShopWeb.E2E.Tests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
// [AllureNUnit] // Disabled to unblock IT03 - complex context management issues in CI/CD
// [AllureSuite("Shopping Tests")]
public class ShoppingTests : BaseTest
{
    private ShoppingFlow? _shoppingFlow;

    [SetUp]
    public void SetUpShoppingFlow()
    {
        _shoppingFlow = new ShoppingFlow(Page);
    }

    [TearDown]
    public async Task CleanupShoppingFlow()
    {
        if (_shoppingFlow != null)
        {
            await _shoppingFlow.CleanupAsync();
        }
    }

   
}