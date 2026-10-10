namespace CrunchyrollRatingMapper.DTOs;
public class AnimeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public double? AverageRating { get; set; }
    public int? TotalRatings { get; set; }
    public string MalId { get; set; } = string.Empty;
    public double? MalRating { get; set; }
    public string MalGenres { get; set; } = string.Empty;
    public bool IsMalPopulated { get; set; }
}

public class AnimeCompleteData
{
    public List<AnimeInfo> animes { get; set; } = new List<AnimeInfo>();
    public int TotalAnimes { get; set; }
}

public class MalAnimeInfo
{
    public int LastVisiblePage { get; set; }
    public string MalId { get; set; } = string.Empty;
    public string Title { get; set; } = "";
    public string? TitleEnglish { get; set; }
    public double? Score { get; set; }
    public string Genres { get; set; } = string.Empty;
}
