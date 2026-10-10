using CrunchyrollRatingMapper.Helpers;

namespace CrunchyrollRatingMapper.Services;


public class TenraiService
{
    private readonly HttpClient _httpClient = new();

    public async Task<string> GetMALAnimes(
        int page,
        CancellationToken cancellationToken = default)
    {
        string url =
            "https://api.tenrai.org/v1/anime" +
            $"?page={page}&order_by=title&sort=asc&type=tv";

        using var response = await HttpRetry.SendAsync(
            _httpClient,
            () => new HttpRequestMessage(HttpMethod.Get, url),
            $"Tenrai anime list (page={page})",
            cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> GetMALAnimesByTitle(
        string title,
        CancellationToken cancellationToken = default)
    {
        string encodedTitle = Uri.EscapeDataString(title);

        string url =
            "https://api.tenrai.org/v1/anime" +
            $"?page=1&order_by=title&sort=asc&q={encodedTitle}";

        using var response = await HttpRetry.SendAsync(
            _httpClient,
            () => new HttpRequestMessage(HttpMethod.Get, url),
            $"Tenrai title search ({title})",
            cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
