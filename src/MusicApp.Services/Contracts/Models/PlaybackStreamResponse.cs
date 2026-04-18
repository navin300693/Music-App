namespace MusicApp.Services.Contracts.Models;

/// <summary>
/// A Data Transfer Object (DTO) to return structured data to the Controller.
/// </summary>
public record PlaybackStreamResponse(
    Guid SongId,
    string StreamUrl,
    string AudioQuality,
    bool SupportsOffline);