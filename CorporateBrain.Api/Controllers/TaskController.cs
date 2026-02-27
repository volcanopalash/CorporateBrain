using CorporateBrain.Application.Common.Interfaces;
using CorporateBrain.Domain.Entities;
using CorporateBrain.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CorporateBrain.Api.Controllers
{
    // The DTO so scalar knows what JSON to expect
    public record CreateTaskDto(string Title, string Description, DateTime DueDate, bool IsCompleted, string AssignedTo);
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiChatServices _aiServices;

        // Injecting both the DB and the Brain
        public TaskController(ApplicationDbContext context, IAiChatServices aiServices)
        {
            _context = context;
            _aiServices = aiServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto request)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                // PostgreSQL strictly requires UTC dates!
                DueDate = request.DueDate.ToUniversalTime(),
                AssignedTo = request.AssignedTo,
                IsCompleted = false,
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // 2. The AI Magic: Translate the database object into a redable memory string!
            var aiMemory = $"Task ID {task.Id}: The task '{task.Title}' is assigned to {task.AssignedTo}. " +
                           $"The details are: {task.Description}. " +
                           $"It must be completed by {task.DueDate:yyyy-MM-dd}. Currently, it is not completed.";

            // 3. Inject it straight into the AI's Brain (Vector DB)
            await _aiServices.IngestDocumentAsync(aiMemory);

            return Ok(new
            {
                Message = "Task created successfully!",
                TaskId = task.Id,
                AiStatus = "CorporateBrain has fully memorized the task."
            });
        }
    }
}
