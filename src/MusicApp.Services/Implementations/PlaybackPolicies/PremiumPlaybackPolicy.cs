namespace MusicApp.Services.Implementations.PlaybackPolicies;

// [Single Responsibility Principle - SRP]: Defines playback rules exclusively for Premium-tier users.
// [Open/Closed Principle - OCP]: New plan types get a new policy class — this one never changes.
// [Liskov Substitution Principle - LSP]: Can replace any IPlaybackPolicy without breaking callers.
public class PremiumPlaybackPolicy : IPlaybackPolicy
{
    /// <summary>Premium users have unlimited skips.</summary>
    public bool CanSkip() => true;

    /// <summary>Premium users can download up to 100 tracks per day for offline playback.</summary>
    public int MaxDailyDownloads() => 100;

    /// <summary>High-quality 320kbps streaming for premium-tier users.</summary>
    public string AudioQuality => "320kbps";
}
