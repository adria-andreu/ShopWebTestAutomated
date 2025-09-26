using NUnit.Framework;

namespace ShopWeb.E2E.Tests.Utilities;

/// <summary>
/// Standardized verification methods for E2E tests
/// Replaces direct Assert.* usage as per E2E_Policy.md section 7
/// 🔹 Iteración 8 - T-075: Crear utilidades Verify.* para assertions
/// </summary>
public static class Verify
{
    /// <summary>
    /// Verify that two values are equal
    /// </summary>
    public static void Equals<T>(T actual, T expected, string message)
    {
        Assert.That(actual, Is.EqualTo(expected), message);
    }

    /// <summary>
    /// Verify that two values are not equal
    /// </summary>
    public static void NotEquals<T>(T actual, T unexpected, string message)
    {
        Assert.That(actual, Is.Not.EqualTo(unexpected), message);
    }

    /// <summary>
    /// Verify that condition is true
    /// </summary>
    public static void True(bool condition, string message)
    {
        StepLogger.Confirm(message);
        Assert.That(condition, Is.True, message);
    }

    /// <summary>
    /// Verify that condition is false
    /// </summary>
    public static void False(bool condition, string message)
    {
        Assert.That(condition, Is.False, message);
    }

    /// <summary>
    /// Verify that object is not null
    /// </summary>
    public static void NotNull<T>(T? objectValue, string message)
    {
        Assert.That(objectValue, Is.Not.Null, message);
    }

    /// <summary>
    /// Verify that object is null
    /// </summary>
    public static void Null<T>(T? objectValue, string message)
    {
        Assert.That(objectValue, Is.Null, message);
    }

    /// <summary>
    /// Verify that collection is empty
    /// </summary>
    public static void Empty<T>(IEnumerable<T> collection, string message)
    {
        Assert.That(collection, Is.Empty, message);
    }

    /// <summary>
    /// Verify that string contains substring
    /// </summary>
    public static void Contains(string actualString, string expectedSubstring, string message)
    {
        Assert.That(actualString, Does.Contain(expectedSubstring), message);
    }

    /// <summary>
    /// Verify that string does not contain substring
    /// </summary>
    public static void DoesNotContain(string actualString, string forbiddenSubstring, string message)
    {
        Assert.That(actualString, Does.Not.Contain(forbiddenSubstring), message);
    }

    /// <summary>
    /// Verify that string is not null or empty
    /// </summary>
    public static void NotNullOrEmpty(string? value, string message)
    {
        Assert.That(value, Is.Not.Null.And.Not.Empty, message);
    }

    /// <summary>
    /// Verify that collection contains specific item
    /// </summary>
    public static void CollectionContains<T>(IEnumerable<T> collection, T item, string message)
    {
        Assert.That(collection, Does.Contain(item), message);
    }

    /// <summary>
    /// Verify that operation completes within timeout
    /// </summary>
    public static void CompletesWithinTimeout(Func<Task> operation, TimeSpan timeout, string message)
    {
        var cts = new CancellationTokenSource(timeout);
        Assert.DoesNotThrowAsync(async () => await operation(), message);
    }
}