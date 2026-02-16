using CorporateBrain.Application.Common.Interfaces;
using CorporateBrain.Domain.Entities;
using CorporateBrain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace CorporateBrain.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id)
    {
        return await _context.Documents.FindAsync(id);
    }

    public async Task<List<Document>> GetAllAsync()
    {
        return await _context.Documents.ToListAsync();
    }

    public async Task AddAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
    }

    public async Task UpdateAsync(Document document)
    {
        _context.Documents.Update(document);
        await Task.CompletedTask; // No actual async work here, but we want to keep the signature consistent
    }
}
