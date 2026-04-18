namespace MusicApp.Data.Entities;


/// <summary>Represents the available subscription tiers.</summary>
public enum PlanType { Free, Premium, Artist }

/// <summary>
/// Extends ASP.NET Core Identity's IdentityUser with a subscription plan,
/// adding a PlanType column to the AspNetUsers table.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>The user's current subscription tier. Defaults to Free.</summary>
    public PlanType PlanType { get; set; } = PlanType.Free;
}
