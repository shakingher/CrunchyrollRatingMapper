using System;
using System.Collections.Generic;
using System.Text;

namespace CrunchyrollRatingMapper.Services;

public class TenraiService
{
    private readonly HttpClient _httpClient;
    public TenraiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GetMALAnimes(int page)
    {
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.Method = HttpMethod.Get;
        httpRequestMessage.RequestUri = new Uri($"https://api.tenrai.org/v1/anime?page={page}&order_by=title&sort=asc&type=tv");
        HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);
        string json = await response.Content.ReadAsStringAsync();
        return json;
    }

    public async Task<string> GetMALAnimesByTitle(string q)
    {
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.Method = HttpMethod.Get;
        httpRequestMessage.RequestUri = new Uri($"https://api.tenrai.org/v1/anime?page=1&order_by=title&sort=asc&q={q}");
        HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);
        string json = await response.Content.ReadAsStringAsync();
        return json;
    }
}
