namespace MusicApp.Services.Contracts.Models;

/// <summary>
/// [Data Transfer Object Pattern]: This is the input model for creating a song.
/// We use a record here for immutability and easy validation.
/// </summary>
public record CreateSongRequest(
    [Required][StringLength(200)] string Title,
    [Required][StringLength(100)] string Artist,
    [StringLength(100)] string? Album,
    DateTime ReleasedDate,
    [Required][AllowedExtensions([".mp3", ".wav", ".flac"])] IFormFile SongFile,
    TimeSpan? Duration = null
);