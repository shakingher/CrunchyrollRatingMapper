namespace CrunchyrollRatingMapper.DTOs;
public class CrunchyrollToken
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
