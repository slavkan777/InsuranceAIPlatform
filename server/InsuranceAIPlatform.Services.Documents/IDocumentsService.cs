using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Documents;

/// <summary>
/// Documents service boundary (skeleton). Future owner of document/photo metadata and
/// missing-evidence detection. No upload, no blob bytes, no OCR, no external calls in the skeleton.
/// </summary>
public interface IDocumentsService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.Documents"/>).</summary>
    string ServiceName { get; }
}
