using CrunchyrollRatingMapper.DTOs;
using System.Text.Json;

namespace CrunchyrollRatingMapper.Utility;

public class JsonDataReader
{
    public static AnimeCompleteData ParseAnime(string json)
    {
        using var document = JsonDocument.Parse(json);
        var completeAnimeData = new AnimeCompleteData();
        completeAnimeData.TotalAnimes = document.RootElement.GetProperty("total").GetInt32();

        var animeList = completeAnimeData.animes;

        foreach (var item in document.RootElement
                                     .GetProperty("data")
                                     .EnumerateArray())
        {
            var metadata = item.GetProperty("series_metadata");
            var rating = item.GetProperty("rating");

            var genres = metadata
                .GetProperty("tenant_categories")
                .EnumerateArray()
                .Select(g => g.GetString())
                .Where(g => !string.IsNullOrWhiteSpace(g));

            animeList.Add(new AnimeInfo
            {
                Id = item.GetProperty("id").GetString() ?? "",
                Title = item.GetProperty("title").GetString() ?? "",
                Availability = metadata
                    .GetProperty("availability_status")
                    .GetString() ?? "",
                Genres = string.Join(" ", genres),
                AverageRating = double.TryParse(
                    rating.GetProperty("average").GetString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var average)
                        ? average
                        : null,
                TotalRatings = rating.GetProperty("total").GetInt32()
            });
        }

        return completeAnimeData;
    }

    public static (int LastVisiblePage, List<MalAnimeInfo> Animes) ParseMalAnime(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        int lastVisiblePage = root
            .GetProperty("pagination")
            .GetProperty("last_visible_page")
            .GetInt32();

        var animes = new List<MalAnimeInfo>();

        foreach (var item in root.GetProperty("data").EnumerateArray())
        {
            var genres = item.GetProperty("genres")
                .EnumerateArray()
                .Select(genre => genre.GetProperty("name").GetString() ?? "")
                .ToList();
            string genresString = string.Empty;
            if (genres != null && genres.Any())
            {
                foreach (var genre in genres) 
                {
                    genresString += genre + " ";
                }
            }

            animes.Add(new MalAnimeInfo
            {
                LastVisiblePage = lastVisiblePage,
                MalId = item.GetProperty("mal_id").ToString(),
                Title = item.GetProperty("title").GetString() ?? "",
                TitleEnglish = item.GetProperty("title_english").ValueKind
                    == JsonValueKind.Null
                        ? null
                        : item.GetProperty("title_english").GetString(),
                Score = item.GetProperty("score").ValueKind
                    == JsonValueKind.Null
                        ? null
                        : item.GetProperty("score").GetDouble(),
                Genres = genresString
            });
        }

        return (lastVisiblePage, animes);
    }
}

