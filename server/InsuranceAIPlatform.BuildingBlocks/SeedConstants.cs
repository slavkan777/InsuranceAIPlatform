namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Shared deterministic seed constants. Used by all service seeders for consistent synthetic data.
/// No domain entities here — primitives only.
/// </summary>
public static class SeedConstants
{
    public const int SyntheticUserCount = 200;
    public const string GoldenClaimId = "CLM-1006";
    public const string SyntheticEmailDomain = "example.invalid";
    public const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=InsuranceAIPlatform;" +
        "Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
    public const string ConnectionStringName = "InsuranceAIPlatform";
    public const string ConnectionStringEnvVar = "INSURANCEAI_CONNECTION_STRING";
    public const string ConnectionStringConfigKey = "ConnectionStrings__InsuranceAIPlatform";
}
