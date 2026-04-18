namespace MusicApp.Services.Implementations;

public class SongService(ISongRepository repository, IBlobService blobService, ILogger<SongService> logger) : ISongService
{
    /// <summary>Returns all songs mapped to response DTOs.</summary>
    public async Task<IEnumerable<SongResponse>> GetAllSongsAsync()
    {
        logger.LogInformation("Retrieving all songs from repository");
        var songs = await repository.ListAllAsync();
        var results = songs.Select(SongFactory.CreateResponse).ToList();
        logger.LogInformation("Retrieved {Count} songs", results.Count);
        return results;
    }

    /// <summary>Extracts duration from the audio file, uploads to blob storage, and persists the entity.</summary>
    public async Task<Guid> CreateSongAsync(CreateSongRequest request)
    {
        logger.LogInformation("Creating song: {Title}", request.Title);

        using var stream = request.SongFile.OpenReadStream();

        var duration = request.Duration ?? AudioFileHelper.ExtractDuration(request.SongFile.FileName, stream);
        logger.LogInformation("Resolved duration {Duration} for file {FileName}", duration, request.SongFile.FileName);

        var entity = SongFactory.CreateEntity(request, duration);
        var extension = Path.GetExtension(request.SongFile.FileName);

        // Define the blob path as {id}{extension} (e.g. guid.mp3)
        var blobName = $"{entity.Id}{extension}";

        // We store the blob name, not the full URL, to keep the DB portable
        string storedPath = await blobService.UploadFileAsync("music-content", blobName, stream);
        logger.LogInformation("Uploaded blob {BlobName} to path {StoredPath}", blobName, storedPath);

        await repository.AddAsync(entity);
        logger.LogInformation("Song created with ID {SongId}", entity.Id);
        return entity.Id;
    }

    /// <summary>Returns a single song by ID, or null if not found.</summary>
    public async Task<SongResponse?> GetSongByIdAsync(Guid id)
    {
        logger.LogInformation("Fetching song with ID {SongId}", id);
        var entity = await repository.GetByIdAsync(id);

        if (entity == null)
        {
            logger.LogWarning("Song with ID {SongId} not found", id);
            return null;
        }

        return SongFactory.CreateResponse(entity);
    }

    /// <summary>Extracts duration for each file, then persists all entities in a single repository call.</summary>
    public async Task<IEnumerable<Guid>> CreateMultipleSongsAsync(IEnumerable<CreateSongRequest> requests)
    {
        var requestList = requests.ToList();
        logger.LogInformation("Bulk creating {Count} songs", requestList.Count);

        var entities = requestList.Select(r =>
        {
            using var stream = r.SongFile.OpenReadStream();
            var duration = r.Duration ?? AudioFileHelper.ExtractDuration(r.SongFile.FileName, stream);
            return SongFactory.CreateEntity(r, duration);
        }).ToList();

        await repository.AddRangeAsync(entities);

        var ids = entities.Select(e => e.Id).ToList();
        logger.LogInformation("Bulk create succeeded; created {Count} songs", ids.Count);
        return ids;
    }
}
