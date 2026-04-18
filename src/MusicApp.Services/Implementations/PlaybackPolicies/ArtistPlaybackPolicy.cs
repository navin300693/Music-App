namespace MusicApp.Services.Implementations.PlaybackPolicies;

public class ArtistPlaybackPolicy : IPlaybackPolicy
{
    /// <summary>Artists have unlimited skips.</summary>
    public bool CanSkip() => true;

    /// <summary>Artists get a high offline download allowance for monitoring their own content.</summary>
    public int MaxDailyDownloads() => 500;

    /// <summary>Lossless master quality for accurate audio monitoring.</summary>
    public string AudioQuality => "Lossless";
}
