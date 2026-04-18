namespace MusicApp.Services.Contracts.Interfaces;

public interface IPlaybackPolicy
{
    bool CanSkip();
    int MaxDailyDownloads();
    string AudioQuality { get; }
}