namespace MusicApp.Services.Contracts.Interfaces;

// [Interface Segregation Principle - ISP]: We keep this interface focused 
// specifically on Song management.
// [Dependency Inversion Principle - DIP]: This interface acts as the "bridge" 
// that allows the Controller to depend on an abstraction rather than a specific class.
public interface ISongService
{
    Task<IEnumerable<SongResponse>> GetAllSongsAsync();
    Task<SongResponse?> GetSongByIdAsync(Guid id);
    Task<Guid> CreateSongAsync(CreateSongRequest request);
    Task<IEnumerable<Guid>> CreateMultipleSongsAsync(IEnumerable<CreateSongRequest> requests);
}