using CorporateBrain.Domain.Entities;

namespace CorporateBrain.Application.Common.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id);
    Task<List<Document>> GetAllAsync();
    Task AddAsync(Document document);
    Task UpdateAsync(Document document); // For later
}
