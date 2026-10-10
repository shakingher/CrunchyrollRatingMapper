using CrunchyrollRatingMapper.DTOs;
using CrunchyrollRatingMapper.Services;
using CrunchyrollRatingMapper.Utility;

var crunchyrollService = new CrunchyrollService();
var animeInfos = new List<AnimeInfo>();

const int pageSize = 50;
int finalStart = 1100; // Fallback if the API doesn't provide a total.
bool totalDiscovered = false;

for (int start = 0; start <= finalStart; start += pageSize)
{
    Console.WriteLine($"Fetching Crunchyroll page starting at {start}...");

    string json = await crunchyrollService.GetBrowsePage(start);
    AnimeCompleteData page = JsonDataReader.ParseAnime(json);

    if (!totalDiscovered && page.TotalAnimes > 0)
    {
        finalStart = ((page.TotalAnimes - 1) / pageSize) * pageSize;
        totalDiscovered = true;

        Console.WriteLine(
            $"Crunchyroll reports {page.TotalAnimes} anime. " +
            $"Last page offset: {finalStart}.");
    }

    animeInfos.AddRange(page.animes);

    Console.WriteLine(
        $"Collected {animeInfos.Count} Crunchyroll anime so far.");
}

var tenraiService = new TenraiService();
var malAnimeInfos = new List<MalAnimeInfo>();

int lastPage = 1;

for (int page = 1; page <= lastPage; page++)
{
    Console.WriteLine($"Fetching MAL page {page}...");

    string json = await tenraiService.GetMALAnimes(page);
    var result = JsonDataReader.ParseMalAnime(json);

    // The first response tells us how many pages are available.
    if (page == 1)
    {
        lastPage = Math.Max(1, result.LastVisiblePage);
        Console.WriteLine($"MAL reports {lastPage} pages.");
    }

    malAnimeInfos.AddRange(result.Animes);

    Console.WriteLine(
        $"Collected {malAnimeInfos.Count} MAL anime so far.");
}

Console.WriteLine("Matching Crunchyroll anime to MAL...");

List<AnimeInfo> finalList =
    await CRMalMapper.MapResponses(animeInfos, malAnimeInfos);

int matched = finalList.Count(a => a.IsMalPopulated);

Console.WriteLine(
    $"Matching complete: {matched}/{finalList.Count} anime matched.");

ExcelBuilder.BuildExcel(finalList);

Console.WriteLine("All tasks completed.");
