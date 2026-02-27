namespace CorporateBrain.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public string AssignedTo { get; set; } = string.Empty; // We can link this to your Users later

    public TaskItem(string TaskTitle, string description, string duedate, string assignedto)
    {
        Title = TaskTitle;
        Description = description;
        DueDate = System.DateTime.Now;
        AssignedTo = assignedto;
    }

    public TaskItem()
    {
    }
}
