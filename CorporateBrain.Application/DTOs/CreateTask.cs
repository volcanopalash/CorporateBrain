namespace CorporateBrain.Application;

public record CreateTaskDto(string Title, string Description, DateTime DueDate, string AssignedTo);