namespace MusicApp.Repositories.Implementations;

public class SongRepository(MusicDbContext context, ILogger<SongRepository> logger) : ISongRepository
{
    /// <summary>Returns all songs as a non-tracked list.</summary>
    public async Task<IEnumerable<SongEntity>> ListAllAsync()
    {
        logger.LogInformation("Querying all songs from database");
        var songs = await context.Songs.AsNoTracking().ToListAsync();
        logger.LogInformation("Retrieved {Count} songs from database", songs.Count);
        return songs;
    }

    /// <summary>Returns a single song by ID, or null if not found.</summary>
    public async Task<SongEntity?> GetByIdAsync(Guid id)
    {
        logger.LogInformation("Querying song with ID {SongId}", id);
        var song = await context.Songs.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

        if (song == null)
            logger.LogWarning("Song with ID {SongId} not found in database", id);

        return song;
    }

    /// <summary>Persists a new song entity and saves changes.</summary>
    public async Task AddAsync(SongEntity entity)
    {
        logger.LogInformation("Inserting song with ID {SongId}", entity.Id);
        await context.Songs.AddAsync(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Song with ID {SongId} saved to database", entity.Id);
    }

    /// <summary>Updates an existing song entity and saves changes.</summary>
    public async Task UpdateAsync(SongEntity entity)
    {
        logger.LogInformation("Updating song with ID {SongId}", entity.Id);
        context.Songs.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Song with ID {SongId} updated in database", entity.Id);
    }

    /// <summary>Deletes a song by ID if it exists, then saves changes.</summary>
    public async Task DeleteAsync(Guid id)
    {
        logger.LogInformation("Deleting song with ID {SongId}", id);
        var song = await GetByIdAsync(id);

        if (song == null)
        {
            logger.LogWarning("Delete skipped — song with ID {SongId} not found", id);
            return;
        }

        context.Songs.Remove(song);
        await context.SaveChangesAsync();
        logger.LogInformation("Song with ID {SongId} deleted from database", id);
    }

    /// <summary>Persists multiple song entities in a single SaveChanges call.</summary>
    public async Task AddRangeAsync(IEnumerable<SongEntity> entities)
    {
        var list = entities.ToList();
        logger.LogInformation("Bulk inserting {Count} songs", list.Count);
        await context.Songs.AddRangeAsync(list);
        await context.SaveChangesAsync();
        logger.LogInformation("Bulk insert of {Count} songs completed", list.Count);
    }
}
