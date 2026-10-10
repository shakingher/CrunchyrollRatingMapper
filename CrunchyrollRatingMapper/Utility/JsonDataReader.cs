using CrunchyrollRatingMapper.DTOs;
using System.Text.Json;

namespace CrunchyrollRatingMapper.Utility;

public class JsonDataReader
{
    public static AnimeCompleteData ParseAnime(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var result = new AnimeCompleteData();

        if (root.TryGetProperty("total", out var totalElement) &&
            totalElement.ValueKind == JsonValueKind.Number)
        {
            result.TotalAnimes = totalElement.GetInt32();
        }

        foreach (var item in root.GetProperty("data").EnumerateArray())
        {
            if (!item.TryGetProperty("series_metadata", out var metadata) ||
                !item.TryGetProperty("rating", out var rating))
            {
                continue;
            }

            var genres = metadata.TryGetProperty("tenant_categories", out var categories) &&
                         categories.ValueKind == JsonValueKind.Array
                ? categories.EnumerateArray()
                    .Where(g => g.ValueKind == JsonValueKind.String)
                    .Select(g => g.GetString())
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                : Enumerable.Empty<string?>();

            double? averageRating = null;

            if (rating.TryGetProperty("average", out var averageElement) &&
                averageElement.ValueKind == JsonValueKind.String &&
                double.TryParse(
                    averageElement.GetString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double average))
            {
                averageRating = average;
            }

            int? totalRatings = null;

            if (rating.TryGetProperty("total", out var ratingTotal) &&
                ratingTotal.ValueKind == JsonValueKind.Number &&
                ratingTotal.TryGetInt32(out int count))
            {
                totalRatings = count;
            }

            result.animes.Add(new AnimeInfo
            {
                Id = item.TryGetProperty("id", out var id)
                    ? id.GetString() ?? ""
                    : "",
                Title = item.TryGetProperty("title", out var title)
                    ? title.GetString() ?? ""
                    : "",
                Availability = metadata.TryGetProperty(
                    "availability_status", out var availability)
                        ? availability.GetString() ?? ""
                        : "",
                Genres = string.Join(" ", genres),
                AverageRating = averageRating,
                TotalRatings = totalRatings
            });
        }

        return result;
    }

    public static (int LastVisiblePage, List<MalAnimeInfo> Animes)
    ParseMalAnime(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        int lastVisiblePage = 1;

        if (root.TryGetProperty("pagination", out var pagination) &&
            pagination.TryGetProperty("last_visible_page", out var lastPage) &&
            lastPage.TryGetInt32(out int page))
        {
            lastVisiblePage = page;
        }

        var animes = new List<MalAnimeInfo>();

        if (!root.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array)
        {
            return (lastVisiblePage, animes);
        }

        foreach (var item in data.EnumerateArray())
        {
            string? titleEnglish = GetOptionalString(item, "title_english");
            double? score = null;

            if (item.TryGetProperty("score", out var scoreElement) &&
                scoreElement.ValueKind == JsonValueKind.Number &&
                scoreElement.TryGetDouble(out double parsedScore))
            {
                score = parsedScore;
            }

            string genres = "";

            if (item.TryGetProperty("genres", out var genreArray) &&
                genreArray.ValueKind == JsonValueKind.Array)
            {
                genres = string.Join(
                    " ",
                    genreArray.EnumerateArray()
                        .Where(g => g.TryGetProperty("name", out _))
                        .Select(g => g.GetProperty("name").GetString())
                        .Where(name => !string.IsNullOrWhiteSpace(name)));
            }

            animes.Add(new MalAnimeInfo
            {
                LastVisiblePage = lastVisiblePage,
                MalId = item.GetProperty("mal_id").ToString(),
                Title = GetOptionalString(item, "title") ?? "",
                TitleEnglish = titleEnglish,
                Score = score,
                Genres = genres
            });
        }

        return (lastVisiblePage, animes);
    }

    private static string? GetOptionalString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }
}

