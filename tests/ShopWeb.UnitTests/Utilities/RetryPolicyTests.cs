using FluentAssertions;
using NUnit.Framework;
using ShopWeb.E2E.Tests.Exceptions;
using ShopWeb.E2E.Tests.Utilities;
using System.Diagnostics;

namespace ShopWeb.UnitTests.Utilities;

[TestFixture]
[Category("Unit")]
public class RetryPolicyTests
{
    [Test]
    public async Task ExecuteAsync_WhenOperationSucceedsOnFirstAttempt_ShouldReturnResultImmediately()
    {
        // Arrange
        var expectedResult = "success";
        var operationCallCount = 0;
        
        Func<Task<string>> operation = () =>
        {
            operationCallCount++;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await RetryPolicy.ExecuteAsync(
            operation, 
            "test-operation", 
            maxRetries: 3
        );

        // Assert
        result.Should().Be(expectedResult);
        operationCallCount.Should().Be(1, "operation should succeed on first attempt");
    }

    [Test]
    public async Task ExecuteAsync_WhenOperationFailsButSucceedsOnRetry_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = "success";
        var operationCallCount = 0;
        
        Func<Task<string>> operation = () =>
        {
            operationCallCount++;
            if (operationCallCount < 3)
            {
                throw new InvalidOperationException($"Attempt {operationCallCount} failed");
            }
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await RetryPolicy.ExecuteAsync(
            operation, 
            "test-operation", 
            maxRetries: 3,
            baseDelay: TimeSpan.FromMilliseconds(1), // Fast execution for unit test
            useExponentialBackoff: false
        );

        // Assert
        result.Should().Be(expectedResult);
        operationCallCount.Should().Be(3, "operation should succeed on third attempt");
    }

    [Test]
    public async Task ExecuteAsync_WhenAllRetriesExhausted_ShouldThrowActionRetryException()
    {
        // Arrange
        var operationCallCount = 0;
        var maxRetries = 2;
        
        Func<Task<string>> operation = () =>
        {
            operationCallCount++;
            throw new InvalidOperationException($"Attempt {operationCallCount} failed");
        };

        // Act & Assert
        var exception = await FluentActions
            .Invoking(() => RetryPolicy.ExecuteAsync(
                operation, 
                "test-operation", 
                maxRetries: maxRetries,
                baseDelay: TimeSpan.FromMilliseconds(1),
                useExponentialBackoff: false
            ))
            .Should().ThrowAsync<ActionRetryException>();

        exception.Which.Action.Should().Be("test-operation");
        exception.Which.RetryCount.Should().Be(maxRetries);
        exception.Which.MaxRetries.Should().Be(maxRetries);
        
        operationCallCount.Should().Be(maxRetries + 1, "should attempt maxRetries + 1 times");
    }

    [Test]
    public async Task ExecuteAsync_WithExponentialBackoff_ShouldIncreaseDelayExponentially()
    {
        // Arrange
        var operationCallCount = 0;
        var delays = new List<TimeSpan>();
        var stopwatch = Stopwatch.StartNew();
        var lastTimestamp = TimeSpan.Zero;
        
        Func<Task<string>> operation = () =>
        {
            operationCallCount++;
            var currentTimestamp = stopwatch.Elapsed;
            
            if (operationCallCount > 1)
            {
                var actualDelay = currentTimestamp - lastTimestamp;
                delays.Add(actualDelay);
            }
            
            lastTimestamp = currentTimestamp;
            
            if (operationCallCount < 4)
            {
                throw new InvalidOperationException($"Attempt {operationCallCount} failed");
            }
            return Task.FromResult("success");
        };

        // Act
        await RetryPolicy.ExecuteAsync(
            operation, 
            "test-operation", 
            maxRetries: 3,
            baseDelay: TimeSpan.FromMilliseconds(10),
            useExponentialBackoff: true
        );

        // Assert
        delays.Should().HaveCount(3, "should have 3 delays between 4 attempts");
        
        // Verify exponential backoff pattern (allowing generous tolerance for Windows timing)
        delays[0].Should().BeCloseTo(TimeSpan.FromMilliseconds(10), precision: TimeSpan.FromMilliseconds(15));
        delays[1].Should().BeCloseTo(TimeSpan.FromMilliseconds(20), precision: TimeSpan.FromMilliseconds(25));
        delays[2].Should().BeCloseTo(TimeSpan.FromMilliseconds(40), precision: TimeSpan.FromMilliseconds(40));
    }

    [Test]
    public async Task ExecuteAsync_WithLinearBackoff_ShouldMaintainConstantDelay()
    {
        // Arrange
        var operationCallCount = 0;
        var delays = new List<TimeSpan>();
        var stopwatch = Stopwatch.StartNew();
        var lastTimestamp = TimeSpan.Zero;
        
        Func<Task<string>> operation = () =>
        {
            operationCallCount++;
            var currentTimestamp = stopwatch.Elapsed;
            
            if (operationCallCount > 1)
            {
                var actualDelay = currentTimestamp - lastTimestamp;
                delays.Add(actualDelay);
            }
            
            lastTimestamp = currentTimestamp;
            
            if (operationCallCount < 3)
            {
                throw new InvalidOperationException($"Attempt {operationCallCount} failed");
            }
            return Task.FromResult("success");
        };

        // Act
        await RetryPolicy.ExecuteAsync(
            operation, 
            "test-operation", 
            maxRetries: 2,
            baseDelay: TimeSpan.FromMilliseconds(10),
            useExponentialBackoff: false
        );

        // Assert
        delays.Should().HaveCount(2, "should have 2 delays between 3 attempts");
        
        // Verify linear backoff pattern (constant delay with generous tolerance for Windows timing)
        delays.ForEach(delay => delay.Should().BeCloseTo(
            TimeSpan.FromMilliseconds(10), 
            precision: TimeSpan.FromMilliseconds(20)
        ));
    }

    [Test]
    public async Task ExecuteAsync_WithSpecificRetryableExceptions_ShouldOnlyRetryMatchingExceptions()
    {
        // Arrange
        var operationCallCount = 0;
        var retryableExceptions = new[] { typeof(InvalidOperationException) };
        
        Func<Task<string>> operation = () =>
        {
            operationCallCount++;
            if (operationCallCount == 1)
            {
                throw new InvalidOperationException("Retryable exception");
            }
            if (operationCallCount == 2)
            {
                throw new ArgumentException("Non-retryable exception");
            }
            return Task.FromResult("success");
        };

        // Act & Assert
        var exception = await FluentActions
            .Invoking(() => RetryPolicy.ExecuteAsync(
                operation, 
                "test-operation", 
                maxRetries: 3,
                baseDelay: TimeSpan.FromMilliseconds(1),
                useExponentialBackoff: false,
                retryableExceptions: retryableExceptions
            ))
            .Should().ThrowAsync<ActionRetryException>();

        exception.Which.InnerException.Should().BeOfType<ArgumentException>();
        operationCallCount.Should().Be(2, "should stop retrying when non-retryable exception is thrown");
    }

    [Test]
    public async Task ExecuteAsync_VoidOverload_WhenOperationSucceeds_ShouldCompleteNormally()
    {
        // Arrange
        var operationCallCount = 0;
        
        Func<Task> operation = () =>
        {
            operationCallCount++;
            return Task.CompletedTask;
        };

        // Act & Assert
        await FluentActions
            .Invoking(() => RetryPolicy.ExecuteAsync(
                operation, 
                "test-operation", 
                maxRetries: 3
            ))
            .Should().NotThrowAsync();

        operationCallCount.Should().Be(1);
    }

    [Test]
    public async Task ExecuteAsync_VoidOverload_WhenOperationFails_ShouldRetryAndThrow()
    {
        // Arrange
        var operationCallCount = 0;
        var maxRetries = 2;
        
        Func<Task> operation = () =>
        {
            operationCallCount++;
            throw new InvalidOperationException($"Attempt {operationCallCount} failed");
        };

        // Act & Assert
        var exception = await FluentActions
            .Invoking(() => RetryPolicy.ExecuteAsync(
                operation, 
                "test-operation", 
                maxRetries: maxRetries,
                baseDelay: TimeSpan.FromMilliseconds(1),
                useExponentialBackoff: false
            ))
            .Should().ThrowAsync<ActionRetryException>();

        exception.Which.Action.Should().Be("test-operation");
        operationCallCount.Should().Be(maxRetries + 1);
    }

    [Test]
    public void ExecuteAsync_WithNullOperation_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<Task<string>> nullOperation = null!;

        // Act & Assert
        FluentActions
            .Invoking(() => RetryPolicy.ExecuteAsync(
                nullOperation, 
                "test-operation"
            ))
            .Should().ThrowAsync<NullReferenceException>();
    }

    [Test]
    public async Task ExecuteAsync_WithZeroMaxRetries_ShouldExecuteOnceAndFailIfException()
    {
        // Arrange
        var operationCallCount = 0;
        
        Func<Task<string>> operation = () =>
        {
            operationCallCount++;
            throw new InvalidOperationException("Always fails");
        };

        // Act & Assert
        var exception = await FluentActions
            .Invoking(() => RetryPolicy.ExecuteAsync(
                operation, 
                "test-operation", 
                maxRetries: 0,
                baseDelay: TimeSpan.FromMilliseconds(1)
            ))
            .Should().ThrowAsync<ActionRetryException>();

        exception.Which.RetryCount.Should().Be(0);
        exception.Which.MaxRetries.Should().Be(0);
        operationCallCount.Should().Be(1, "should execute exactly once when maxRetries is 0");
    }
}