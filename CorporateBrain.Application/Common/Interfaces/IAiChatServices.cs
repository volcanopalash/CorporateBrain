namespace CorporateBrain.Application.Common.Interfaces;

public interface IAiChatServices
{
    // Simple chat: User sends text, AI returns text
    Task<string> ChatAsync(string userMessage);
    Task IngestDocumentAsync(string text);
}
