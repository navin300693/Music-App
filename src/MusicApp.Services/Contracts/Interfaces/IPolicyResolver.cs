namespace MusicApp.Services.Contracts.Interfaces;

public interface IPolicyResolver
{
    IPlaybackPolicy GetPolicy(PlanType plan);
}