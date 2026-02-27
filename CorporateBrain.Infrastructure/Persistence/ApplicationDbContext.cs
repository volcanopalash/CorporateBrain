using System.Numerics;
using CorporateBrain.Application.Common.Interfaces;
using CorporateBrain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CorporateBrain.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();

    // 1. Add the new AI Memory Table
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    // Our new db for tasks table
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // 2. Turn on the PostgreSQL Vector Engine
        modelBuilder.HasPostgresExtension("vector");

        // 3. Define the column type for the arrays
        modelBuilder.Entity<DocumentChunk>()
            .Property(dc => dc.Embedding)
            .HasConversion(
                v => new Pgvector.Vector(v),
                v => new ReadOnlyMemory<float>(v.ToArray())
            )
            .HasColumnType("vector(3072)"); // Assuming 3072 dimensions for the embedding

        // This is where we configure database rules (Fluent API)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
