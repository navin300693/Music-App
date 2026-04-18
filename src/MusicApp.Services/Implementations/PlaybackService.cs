namespace MusicApp.Services.Implementations;

public class PlaybackService(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IPolicyResolver policyResolver,
    ISongService songService,
    IBlobService blobService,
    ILogger<PlaybackService> logger) : IPlaybackService
{
    /// <summary>Resolves the user's playback policy and returns a secure stream URL for the song.</summary>
    public async Task<PlaybackStreamResponse?> GetStreamAsync(Guid songId)
    {
        logger.LogInformation("Stream requested for song ID {SongId}", songId);

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            logger.LogWarning("Stream denied — could not resolve current user for song ID {SongId}", songId);
            return null;
        }

        // [Strategy Pattern]: Resolve the correct policy based on the user's plan
        var policy = policyResolver.GetPolicy(user.PlanType);
        var quality = policy.AudioQuality;
        logger.LogInformation("Resolved policy for user {UserId}: quality={Quality}", user.Id, quality);

        var song = await songService.GetSongByIdAsync(songId);
        if (song == null)
        {
            logger.LogWarning("Stream denied — song ID {SongId} not found", songId);
            return null;
        }

        // Select the correct blob folder based on audio quality
        var url = quality.ToLower() switch
        {
            "lossless" => await blobService.GenerateSecureLinkAsync("music-content", "high", $"{song.Id}.mp3"),
            "320kbps" => await blobService.GenerateSecureLinkAsync("music-content", "standard", $"{song.Id}.mp3"),
            _ => await blobService.GenerateSecureLinkAsync("music-content", "low", $"{song.Id}.mp3")
        };

        if (url == null)
        {
            logger.LogWarning("Stream denied — blob URL could not be generated for song ID {SongId}", songId);
            return null;
        }

        logger.LogInformation("Stream URL generated for song ID {SongId}", songId);
        return new PlaybackStreamResponse(songId, url, quality, policy.MaxDailyDownloads() > 0);
    }

    /// <summary>Checks whether the current user's playback policy permits skipping.</summary>
    public async Task<bool> AttemptSkipAsync()
    {
        logger.LogInformation("Skip attempt initiated");

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            logger.LogWarning("Skip denied — could not resolve current user");
            return false;
        }

        // [Strategy Pattern]: Let the specific policy decide
        var policy = policyResolver.GetPolicy(user.PlanType);
        var canSkip = policy.CanSkip();

        if (canSkip)
            logger.LogInformation("Skip allowed for user {UserId} with plan {PlanType}", user.Id, user.PlanType);
        else
            logger.LogWarning("Skip denied for user {UserId} with plan {PlanType}", user.Id, user.PlanType);

        return canSkip;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal == null) return null;

        return await userManager.GetUserAsync(principal);
    }
}
