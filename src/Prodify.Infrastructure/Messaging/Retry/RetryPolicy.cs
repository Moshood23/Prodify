namespace Prodify.Infrastructure.Messaging.Retry;

public static class RetryPolicy
{
    public static async Task ExecuteAsync(
        Func<Task> action,
        int maxAttempts = 3,
        TimeSpan? initialDelay = null,
        CancellationToken cancellationToken = default)
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                await action();
                return;
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
    }

    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        int maxAttempts = 3,
        TimeSpan? initialDelay = null,
        CancellationToken cancellationToken = default)
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                return await action();
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
    }
}