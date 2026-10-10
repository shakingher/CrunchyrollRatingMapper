
using CrunchyrollRatingMapper.DTOs;
using CrunchyrollRatingMapper.Helpers;
using CurlImpersonate.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CrunchyrollRatingMapper.Services;

public class CrunchyrollService
{
    private readonly HttpClient _httpClient;
    private CrunchyrollToken? _token;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public CrunchyrollService()
    {
        _httpClient = new HttpClient(new CurlHandler());
    }

    private async Task<string> GetToken(
        CancellationToken cancellationToken = default)
    {
        if (_token is not null &&
            _token.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(60))
        {
            return _token.AccessToken;
        }

        await _tokenLock.WaitAsync(cancellationToken);

        try
        {
            if (_token is not null &&
                _token.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(60))
            {
                return _token.AccessToken;
            }

            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes("cr_web:"));

            using var response = await HttpRetry.SendAsync(
                _httpClient,
                () =>
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        "https://www.crunchyroll.com/auth/v1/token");

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Basic", credentials);

                    request.Content = new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            ["grant_type"] = "client_id"
                        });

                    return request;
                },
                "Crunchyroll token request",
                cancellationToken);

            string body = await response.Content.ReadAsStringAsync(
                cancellationToken);

            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;

            string accessToken = root.GetProperty("access_token")
                .GetString()
                ?? throw new InvalidOperationException(
                    "Crunchyroll access token was missing.");

            int expiresIn = root.GetProperty("expires_in").GetInt32();

            _token = new CrunchyrollToken
            {
                AccessToken = accessToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn)
            };

            return _token.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public async Task<string> GetBrowsePage(
        int start,
        CancellationToken cancellationToken = default)
    {
        string token = await GetToken(cancellationToken);

        using var response = await HttpRetry.SendAsync(
            _httpClient,
            () =>
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://www.crunchyroll.com/content/v2/discover/browse" +
                    $"?n=50&sort_by=alphabetical&ratings=true&locale=en-US&start={start}");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                return request;
            },
            $"Crunchyroll browse (start={start})",
            cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
