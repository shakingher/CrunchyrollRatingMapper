using CrunchyrollRatingMapper.DTOs;
using CrunchyrollRatingMapper.Services;
using CrunchyrollRatingMapper.Utility;

CrunchyrollService crunchyrollService = new CrunchyrollService();
int finalNum = 1100;
bool isFinalNumSet = false;
List<AnimeInfo> animeInfos = new List<AnimeInfo>();
for (int start = 0; start <= finalNum; start += 50)
{
    string json = await crunchyrollService.GetBrowsePage(start);
    AnimeCompleteData animeCompleteData = JsonDataReader.ParseAnime(json);
    if (animeCompleteData != null && animeCompleteData.TotalAnimes > 0 && !isFinalNumSet)
    {
        int totalAnimes = animeCompleteData.TotalAnimes;
        int rem = totalAnimes % 50;
        finalNum = rem == 0 ? totalAnimes - 50 : totalAnimes - rem;
        isFinalNumSet = true;
    }
    if (animeCompleteData != null && animeCompleteData.animes.Any())
    {
        animeInfos.AddRange(animeCompleteData.animes);
    }
}

TenraiService tenraiService = new TenraiService();
List<MalAnimeInfo> malAnimeInfos = new List<MalAnimeInfo>();
int lastPage = 10;
bool isLastPageSet = false;
for (int page = 1; page <= lastPage; page++)
{
    string malResponseString = await tenraiService.GetMALAnimes(page);
    var malResponse = JsonDataReader.ParseMalAnime(malResponseString);
    if (malResponse.LastVisiblePage > 0 && !isLastPageSet)
    {
        lastPage = malResponse.LastVisiblePage;
        isLastPageSet = true;
    }
    if (malResponse.Animes != null && malResponse.Animes.Any())
    {
        malAnimeInfos.AddRange(malResponse.Animes);
    }
}
List<AnimeInfo> finalList = await CRMalMapper.MapResponses(animeInfos, malAnimeInfos);

ExcelBuilder.BuildExcel(finalList);
