namespace MusicApp.Repositories.Contracts.Interfaces;

/// <summary>
/// [Interface Segregation]: This interface is specific to Song data operations.
/// [Dependency Inversion]: Services will depend on this interface, not the SQL implementation.
/// </summary>
public interface ISongRepository
{
    Task<IEnumerable<SongEntity>> ListAllAsync();
    Task<SongEntity?> GetByIdAsync(Guid id);
    Task AddAsync(SongEntity entity);
    Task UpdateAsync(SongEntity entity);
    Task DeleteAsync(Guid id);
    Task AddRangeAsync(IEnumerable<SongEntity> entities);
}