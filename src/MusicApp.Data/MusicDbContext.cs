namespace MusicApp.Data;

// EF Core configuration and exposing DbSets for the application's data model.
public class MusicDbContext(DbContextOptions<MusicDbContext> options, ILogger<MusicDbContext> logger)
    : IdentityDbContext<ApplicationUser>(options)
{
  /// <summary>Represents the Songs table.</summary>
  public DbSet<SongEntity> Songs { get; set; }

  /// <summary>Configures entity mappings and constraints for the data model.</summary>
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    logger.LogInformation("Configuring entity model for {Entity}", nameof(SongEntity));

    modelBuilder.Entity<SongEntity>(entity =>
    {
      entity.ToTable("Songs");
      entity.HasKey(e => e.Id);

      // NEWSEQUENTIALID reduces index fragmentation vs random GUIDs
      entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

      entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

      entity.Property(e => e.Artist)
                .IsRequired()
                .HasMaxLength(100);

      entity.Property(e => e.AlbumName)
                .HasMaxLength(100);
    });

    logger.LogInformation("Entity model configuration complete");
  }
}
