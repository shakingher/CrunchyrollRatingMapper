using System.Net;

namespace CrunchyrollRatingMapper.Helpers;
public static class HttpRetry
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(1);

    public static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        Func<HttpRequestMessage> createRequest,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        int attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                using var request = createRequest();
                var response = await client.SendAsync(
                    request,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                    return response;

                if (!IsTransientStatusCode(response.StatusCode))
                {
                    var status = response.StatusCode;
                    var reason = response.ReasonPhrase;
                    response.Dispose();

                    throw new HttpRequestException(
                        $"{serviceName} returned HTTP {(int)status} ({reason}).",
                        null,
                        status);
                }

                var delay = GetRetryDelay(response);
                var error = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase})";
                response.Dispose();

                Console.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                    $"{serviceName}: attempt {attempt} failed ({error}). " +
                    $"Retrying in {delay.TotalSeconds:0} seconds.");

                await Task.Delay(delay, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode.HasValue &&
                    !IsTransientStatusCode(ex.StatusCode.Value))
                {
                    throw;
                }

                Console.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                    $"{serviceName}: network error on attempt {attempt}: " +
                    $"{ex.Message}. Retrying in 60 seconds.");

                await Task.Delay(RetryDelay, cancellationToken);
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                    $"{serviceName}: timeout on attempt {attempt}: " +
                    $"{ex.Message}. Retrying in 60 seconds.");

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        int code = (int)statusCode;

        return statusCode == HttpStatusCode.RequestTimeout ||
               code == 429 ||
               code >= 500;
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is TimeSpan delta && delta > TimeSpan.Zero)
            return delta;

        if (retryAfter?.Date is DateTimeOffset date)
        {
            var wait = date - DateTimeOffset.UtcNow;

            if (wait > TimeSpan.Zero)
                return wait;
        }

        return RetryDelay;
    }
}