
using CrunchyrollRatingMapper.DTOs;
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

    private async Task<string> GetToken()
    {
        if (_token is not null &&
            _token.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(60))
        {
            return _token.AccessToken;
        }

        await _tokenLock.WaitAsync();
        try
        {
            if (_token is not null && _token.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(60))
            {
                return _token.AccessToken;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = HttpMethod.Post;
            httpRequestMessage.RequestUri = new Uri("https://www.crunchyroll.com/auth/v1/token");
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("cr_web:"));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            Dictionary<string, string> content = new Dictionary<string, string>();
            content.Add("grant_type", "client_id");
            httpRequestMessage.Content = new FormUrlEncodedContent(content);
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            var body = await httpResponseMessage.Content.ReadAsStringAsync();
            httpResponseMessage.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            var accessToken = root.GetProperty("access_token").GetString()
                    ?? throw new InvalidOperationException(
                        "Access token was missing.");

            var expiresIn = root.GetProperty("expires_in").GetInt32();

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

    public async Task<string> GetBrowsePage(int start)
    {
        string token = await GetToken();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://www.crunchyroll.com/content/v2/discover/browse" +
            $"?n=50&sort_by=alphabetical&ratings=true&locale=en-US&start={start}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return body;
    }
}
