using NUnit.Framework;

namespace ShopWeb.E2E.Tests.Utilities;

public static class Verify
{
    public static void VerifyEquals<T>(T expected, T actual, string message)
    {
        Assert.That(actual, Is.EqualTo(expected), message);
    }

    public static void VerifyNotEquals<T>(T expected, T actual, string message)
    {
        Assert.That(actual, Is.Not.EqualTo(expected), message);
    }

    public static void VerifyTrue(bool condition, string message)
    {
        Assert.That(condition, Is.True, message);
    }

    public static void VerifyFalse(bool condition, string message)
    {
        Assert.That(condition, Is.False, message);
    }

    public static void VerifyNotNull(object? value, string message)
    {
        Assert.That(value, Is.Not.Null, message);
    }

    public static void VerifyNull(object? value, string message)
    {
        Assert.That(value, Is.Null, message);
    }

    public static void VerifyEmpty<T>(IEnumerable<T> collection, string message)
    {
        Assert.That(collection, Is.Empty, message);
    }

    public static void VerifyContains<T>(IEnumerable<T> collection, T item, string message)
    {
        Assert.That(collection, Does.Contain(item), message);
    }

    public static void VerifyDoesNotContain<T>(IEnumerable<T> collection, T item, string message)
    {
        Assert.That(collection, Does.Not.Contain(item), message);
    }

    public static void VerifyContains(string actualString, string expectedSubstring, string message)
    {
        Assert.That(actualString, Does.Contain(expectedSubstring), message);
    }

    public static void VerifyDoesNotContain(string actualString, string forbiddenSubstring, string message)
    {
        Assert.That(actualString, Does.Not.Contain(forbiddenSubstring), message);
    }

    public static void VerifyCount<T>(IEnumerable<T> collection, int expectedCount, string message)
    {
        Assert.That(collection.ToList(), Has.Count.EqualTo(expectedCount), message);
    }

    public static void VerifyGreaterThan<T>(T value, T threshold, string message) where T : IComparable<T>
    {
        Assert.That(value, Is.GreaterThan(threshold), message);
    }

    public static void VerifyLessThan<T>(T value, T threshold, string message) where T : IComparable<T>
    {
        Assert.That(value, Is.LessThan(threshold), message);
    }

    public static void VerifyGreaterOrEqual<T>(T value, T threshold, string message) where T : IComparable<T>
    {
        Assert.That(value, Is.GreaterThanOrEqualTo(threshold), message);
    }

    public static void VerifyLessOrEqual<T>(T value, T threshold, string message) where T : IComparable<T>
    {
        Assert.That(value, Is.LessThanOrEqualTo(threshold), message);
    }

    public static void VerifyFail(string message)
    {
        Assert.Fail(message);
    }

    public static void VerifyPass(string message)
    {
        Assert.Pass(message);
    }
}