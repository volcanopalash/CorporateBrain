using CorporateBrain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Security.Claims;

namespace CorporateBrain.Infrastructure.AI.Plugins;

public class TaskPlugin
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TaskPlugin(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    // A helper method to quickly grab the current user's ID
    private string GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
    }

    // This tag exposes the method to the AI!
    [KernelFunction("mark_task_completed")]
    [Description("Marks a specific task as completed in the database using its title.")]
    public async Task<string> MarkTaskCompleteAsync([Description("The title or part of the title of the task to mark as complete")] string taskTitle)
    {

        var currentUserId = GetCurrentUserId(); // Who is asking?

        // Find the task in the database by title (or part of it)
        //var task = _context.Tasks
        //    .FirstOrDefault(t => t.Title.ToLower().Contains(taskTitle.ToLower()) && !t.IsCompleted);

        var task = _context.Tasks.FirstOrDefault(t => 
            t.OwnerId == currentUserId && // The Multi-Tenant Lock!
            t.Title.ToLower().Contains(taskTitle.ToLower()) &&
            !t.IsCompleted);

        if(task == null)
        {
            return $"Could not find an incomplete task matching '{taskTitle}' that belongs to you.";
        }

        // 2. Actually modify the database!
        task.IsCompleted = true;
        await _context.SaveChangesAsync();

        return $"Success! I have marked the task '{task.Title}' as completed in the system.";
    }

    [KernelFunction("get_total_task_count")]
    [Description("Gets the total number of tasks currently assigned or existing in the database system.")]
    public async Task<string> GetTotalTaskCountAsync()
    {
        // Security PATCH: Only count tasks owned by this specific user! This way, the AI can't peek at how many tasks other users have in the system. It can only report on the count of the user's own tasks, which is still useful for productivity insights without compromising privacy.
        var currentUserId = GetCurrentUserId();

        var count = await _context.Tasks.CountAsync(t => t.OwnerId == currentUserId);

        return $"You currently have {count} taks assigned to you account.";
    }
}
