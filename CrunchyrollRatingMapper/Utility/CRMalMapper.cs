using CrunchyrollRatingMapper.DTOs;
using CrunchyrollRatingMapper.Services;

namespace CrunchyrollRatingMapper.Utility;
public class CRMalMapper
{
    public static async Task<List<AnimeInfo>> MapResponses(List<AnimeInfo> animeInfos, List<MalAnimeInfo> malAnimeInfos)
    {
        foreach (var animeInfo in animeInfos)
        {
            var mai = malAnimeInfos.FirstOrDefault(m => (m.TitleEnglish == animeInfo.Title || m.Title == animeInfo.Title));
            if (mai == null)
                continue;
            animeInfo.MalId = mai.MalId;
            animeInfo.MalGenres = mai.Genres;
            animeInfo.MalRating = mai.Score;
            animeInfo.IsMalPopulated = true;
        }
        TenraiService tenraiService = new TenraiService();
        var unpopulatedAnimes = animeInfos.Where(ai => !ai.IsMalPopulated);
        foreach (var anime in unpopulatedAnimes) 
        { 
            string json = await tenraiService.GetMALAnimesByTitle(anime.Title);
            var malAnimeInfo = JsonDataReader.ParseMalAnime(json);
            if (malAnimeInfo.Animes != null && malAnimeInfo.Animes.Any())
            {
                var firstMalAnime = malAnimeInfo.Animes.First();
                anime.MalId = firstMalAnime.MalId;
                anime.MalGenres = firstMalAnime.Genres;
                anime.MalRating = firstMalAnime.Score;
                anime.IsMalPopulated = true;
            }
        }
        return animeInfos;
    }
}
