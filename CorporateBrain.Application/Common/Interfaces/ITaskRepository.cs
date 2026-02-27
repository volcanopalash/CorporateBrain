using CorporateBrain.Domain.Entities;

namespace CorporateBrain.Application;

public interface ITaskRepository
{
    Task<TaskItem?> CreateTask(TaskItem task);
}
