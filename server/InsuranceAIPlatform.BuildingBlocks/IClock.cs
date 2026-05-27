namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Abstraction over the system clock. Enables deterministic testing.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>
/// Production implementation backed by <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
