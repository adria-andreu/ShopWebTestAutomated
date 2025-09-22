using ShopWeb.E2E.Tests.Exceptions;

namespace ShopWeb.E2E.Tests.Utilities;

public class RetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        bool useExponentialBackoff = true,
        Type[]? retryableExceptions = null)
    {
        var delay = baseDelay ?? TimeSpan.FromSeconds(1);
        var exceptions = new List<Exception>();
        retryableExceptions ??= new[] { typeof(Exception) };

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (ShouldRetry(ex, retryableExceptions) && attempt < maxRetries)
            {
                exceptions.Add(ex);
                
                if (useExponentialBackoff)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt)));
                }
                else
                {
                    await Task.Delay(delay);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                throw new ActionRetryException(operationName, attempt, maxRetries, ex);
            }
        }

        throw new ActionRetryException(operationName, maxRetries, maxRetries);
    }

    public static async Task ExecuteAsync(
        Func<Task> operation,
        string operationName,
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        bool useExponentialBackoff = true,
        Type[]? retryableExceptions = null)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, operationName, maxRetries, baseDelay, useExponentialBackoff, retryableExceptions);
    }

    private static bool ShouldRetry(Exception exception, Type[] retryableExceptions)
    {
        return retryableExceptions.Any(type => type.IsAssignableFrom(exception.GetType()));
    }
}

public static class RetryPolicyExtensions
{
    public static async Task<T> WithRetryAsync<T>(
        this Task<T> task,
        string operationName,
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        bool useExponentialBackoff = true,
        Type[]? retryableExceptions = null)
    {
        return await RetryPolicy.ExecuteAsync(() => task, operationName, maxRetries, baseDelay, useExponentialBackoff, retryableExceptions);
    }

    public static async Task WithRetryAsync(
        this Task task,
        string operationName,
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        bool useExponentialBackoff = true,
        Type[]? retryableExceptions = null)
    {
        await RetryPolicy.ExecuteAsync(() => task, operationName, maxRetries, baseDelay, useExponentialBackoff, retryableExceptions);
    }
}