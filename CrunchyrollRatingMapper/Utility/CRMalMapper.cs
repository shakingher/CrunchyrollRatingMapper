using CrunchyrollRatingMapper.DTOs;
using CrunchyrollRatingMapper.Services;
using System.Text.RegularExpressions;

namespace CrunchyrollRatingMapper.Utility;
public class CRMalMapper
{
    public static async Task<List<AnimeInfo>> MapResponses(
    List<AnimeInfo> animeInfos,
    List<MalAnimeInfo> malAnimeInfos)
    {
        // Index MAL entries by normalized title for faster lookups.
        var malByTitle = malAnimeInfos
            .SelectMany(m => new[]
            {
            new { Key = NormalizeTitle(m.Title), Anime = m },
            new { Key = NormalizeTitle(m.TitleEnglish), Anime = m }
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First().Anime);

        foreach (var anime in animeInfos)
        {
            string key = NormalizeTitle(anime.Title);

            if (malByTitle.TryGetValue(key, out var match))
            {
                ApplyMalData(anime, match);
            }
        }

        var tenraiService = new TenraiService();

        foreach (var anime in animeInfos.Where(a => !a.IsMalPopulated))
        {
            try
            {
                string json = await tenraiService.GetMALAnimesByTitle(anime.Title);
                var result = JsonDataReader.ParseMalAnime(json);

                var match = result.Animes.FirstOrDefault(m =>
                    NormalizeTitle(m.Title) == NormalizeTitle(anime.Title) ||
                    NormalizeTitle(m.TitleEnglish) == NormalizeTitle(anime.Title));

                if (match is not null)
                {
                    ApplyMalData(anime, match);
                }
                else
                {
                    Console.WriteLine(
                        $"No confident MAL title match found for: {anime.Title}");
                }
            }
            catch (Exception ex)
            {
                // A non-retryable HTTP response or invalid JSON can still fail.
                Console.WriteLine(
                    $"Could not map '{anime.Title}' to MAL: {ex.Message}");
            }
        }

        return animeInfos;
    }

    private static void ApplyMalData(AnimeInfo anime, MalAnimeInfo mal)
    {
        anime.MalId = mal.MalId;
        anime.MalGenres = mal.Genres;
        anime.MalRating = mal.Score;
        anime.IsMalPopulated = true;
    }

    private static string NormalizeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "";

        return Regex.Replace(title.Trim().ToLowerInvariant(), @"[^a-z0-9]", "");
    }
}
