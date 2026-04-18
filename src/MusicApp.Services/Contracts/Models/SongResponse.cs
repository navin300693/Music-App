namespace MusicApp.Services.Contracts.Models;

/// <summary>
/// [Data Transfer Object Pattern]: This represents the "Public Face" of a song.
/// We use a record here because the data is read-only once it leaves the service.
/// </summary>
public record SongResponse(
    Guid Id,
    string Title,
    string Artist,
    string Album,
    TimeSpan Duration,
    DateTime ReleasedDate
);