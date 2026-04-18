namespace MusicApp.Services.Factories;

/// <summary>
/// PATTERN: Simple Factory (Static Implementation)
/// </summary>
/// <remarks>
/// HOW IT FITS:
/// 1. Creational: Centralizes the instantiation logic for Song-related objects.
/// 
/// 2. Pure Transformation: Designed for stateless mapping between DTOs and Entities.
///    Since it is static, it offers high performance but does not support DI-based 
///    dependency injection.
public static class SongFactory
{
    public static SongEntity CreateEntity(CreateSongRequest request, TimeSpan duration)
    {
        return new SongEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Artist = request.Artist,
            AlbumName = request.Album,
            Duration = duration,
            ReleasedDate = request.ReleasedDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static SongResponse CreateResponse(SongEntity s)
    {
        return new SongResponse(
            s.Id,
            s.Title,
            s.Artist,
            s.AlbumName ?? "N/A",
            s.Duration,
            s.ReleasedDate
        );
    }
}