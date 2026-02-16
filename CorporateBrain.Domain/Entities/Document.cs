using CorporateBrain.Domain.Common;

namespace CorporateBrain.Domain.Entities;

public sealed class Document : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; } = DocumentStatus.Uploaded;

    // Constructor helps enforce required data
    public Document(string title, string fileName)
    {
        Title = title;
        FileName = fileName;
    }

    // Methods to change state (Bussiness Logic)
    public void UpdateContent(string extractedText)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
            throw new ArgumentException("Extracted text cannot be empty.", nameof(extractedText));
        Content = extractedText;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        Status = DocumentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsReady()
    {
        Status = DocumentStatus.Ready;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = DocumentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
