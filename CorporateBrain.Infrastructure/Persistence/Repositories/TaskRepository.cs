using CorporateBrain.Application;
using CorporateBrain.Domain.Entities;
using CorporateBrain.Infrastructure.Persistence;

using System.Runtime.InteropServices;

namespace CorporateBrain.Infrastructure;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem> CreateTask(TaskItem request)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            // PostgreSQL strictly requires UTC dates!
            DueDate = request.DueDate.ToUniversalTime(),
            AssignedTo = request.AssignedTo,
            IsCompleted = false,
            OwnerId = request.OwnerId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }
}
