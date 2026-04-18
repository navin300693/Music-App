namespace MusicApp.Services.Factories;

/// <summary>
/// PATTERN: Strategy Pattern (with DI-based resolution (Factory Pattern))
/// </summary>
/// <remarks>
/// HOW IT FITS:
/// 1. Behavioral (Strategy):
/// Selects an appropriate IPlaybackPolicy implementation at runtime
/// based on the provided PlanType.
///
/// 2. Resolution Mechanism: (Factory Pattern)`
/// Uses .NET Keyed Services to resolve the correct implementation.
/// The DI container acts as the factory; this class is a selector.
///
/// 3. Responsibility:
/// Encapsulates the logic for mapping PlanType to a concrete strategy,
/// keeping consumers decoupled from implementation details and supporting
/// Open/Closed Principle.
/// </remarks>
public class PolicyResolver(IServiceProvider serviceProvider) : IPolicyResolver
{
    public IPlaybackPolicy GetPolicy(PlanType plan)
    {
        // This is the "Keyed" lookup logic
        return serviceProvider.GetRequiredKeyedService<IPlaybackPolicy>(plan);
    }
}