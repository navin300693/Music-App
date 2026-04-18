namespace MusicApp.Services.Contracts.Interfaces;

/// <summary>
/// Orchestrates playback logic by coordinating between Users, 
/// Policy Resolvers, and Music Data.
/// </summary>
public interface IPlaybackService
{
    /// <summary>
    /// Validates user plan and returns streaming metadata for a specific song.
    /// </summary>
    Task<PlaybackStreamResponse?> GetStreamAsync(Guid songId);

    /// <summary>
    /// Checks if the current user's policy allows them to skip the current track.
    /// </summary>
    Task<bool> AttemptSkipAsync();
}