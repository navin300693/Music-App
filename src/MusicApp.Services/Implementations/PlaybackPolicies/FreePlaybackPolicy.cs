namespace MusicApp.Services.Implementations.PlaybackPolicies;

public class FreePlaybackPolicy : IPlaybackPolicy
{
    /// <summary>Free users cannot skip tracks.</summary>
    public bool CanSkip() => false;

    /// <summary>Offline downloads are disabled on the free tier.</summary>
    public int MaxDailyDownloads() => 0;

    /// <summary>Standard 128kbps quality for free-tier streaming.</summary>
    public string AudioQuality => "128kbps";
}
