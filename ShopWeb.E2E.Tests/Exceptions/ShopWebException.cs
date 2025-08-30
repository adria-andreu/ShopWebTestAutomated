namespace ShopWeb.E2E.Tests.Exceptions;

public class ShopWebException : Exception
{
    public string? SiteId { get; }
    public string? TestName { get; }
    public string? PageUrl { get; }

    public ShopWebException(string message) : base(message)
    {
    }

    public ShopWebException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ShopWebException(string message, string siteId, string testName, string pageUrl) : base(message)
    {
        SiteId = siteId;
        TestName = testName;
        PageUrl = pageUrl;
    }

    public ShopWebException(string message, Exception innerException, string siteId, string testName, string pageUrl) 
        : base(message, innerException)
    {
        SiteId = siteId;
        TestName = testName;
        PageUrl = pageUrl;
    }
}

public class NavigationException : ShopWebException
{
    public string? TargetUrl { get; }
    public int AttemptNumber { get; }
    public int MaxAttempts { get; }

    public NavigationException(string message, string targetUrl, int attemptNumber, int maxAttempts) 
        : base($"Navigation failed to '{targetUrl}' (attempt {attemptNumber}/{maxAttempts}): {message}")
    {
        TargetUrl = targetUrl;
        AttemptNumber = attemptNumber;
        MaxAttempts = maxAttempts;
    }

    public NavigationException(string message, Exception innerException, string targetUrl, int attemptNumber, int maxAttempts) 
        : base($"Navigation failed to '{targetUrl}' (attempt {attemptNumber}/{maxAttempts}): {message}", innerException)
    {
        TargetUrl = targetUrl;
        AttemptNumber = attemptNumber;
        MaxAttempts = maxAttempts;
    }
}

public class ElementNotFoundException : ShopWebException
{
    public string? Selector { get; }
    public int TimeoutMs { get; }

    public ElementNotFoundException(string selector, int timeoutMs) 
        : base($"Element not found: '{selector}' after {timeoutMs}ms")
    {
        Selector = selector;
        TimeoutMs = timeoutMs;
    }

    public ElementNotFoundException(string message, string selector, int timeoutMs) 
        : base($"{message} - Element: '{selector}' after {timeoutMs}ms")
    {
        Selector = selector;
        TimeoutMs = timeoutMs;
    }
}

public class PageLoadException : ShopWebException
{
    public string? ExpectedElement { get; }
    public int TimeoutMs { get; }

    public PageLoadException(string pageName, string expectedElement, int timeoutMs) 
        : base($"Page '{pageName}' failed to load - Expected element '{expectedElement}' not found after {timeoutMs}ms")
    {
        ExpectedElement = expectedElement;
        TimeoutMs = timeoutMs;
    }
}

public class ActionRetryException : ShopWebException
{
    public string? Action { get; }
    public int RetryCount { get; }
    public int MaxRetries { get; }

    public ActionRetryException(string action, int retryCount, int maxRetries) 
        : base($"Action '{action}' failed after {retryCount} retries (max: {maxRetries})")
    {
        Action = action;
        RetryCount = retryCount;
        MaxRetries = maxRetries;
    }

    public ActionRetryException(string action, int retryCount, int maxRetries, Exception innerException) 
        : base($"Action '{action}' failed after {retryCount} retries (max: {maxRetries})", innerException)
    {
        Action = action;
        RetryCount = retryCount;
        MaxRetries = maxRetries;
    }
}