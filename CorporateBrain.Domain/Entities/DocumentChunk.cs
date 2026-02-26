using Microsoft.Extensions.VectorData;

namespace CorporateBrain.Domain.Entities;

public class DocumentChunk
{
    // The new attributes do NOT have the word "Record" in them
    [VectorStoreKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [VectorStoreData]
    public string Text { get; set; } = string.Empty;

    [VectorStoreVector(3072)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}