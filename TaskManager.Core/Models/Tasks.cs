namespace TaskManager.Core.Models;

public abstract class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; protected set; } = "Todo";
    public int Priority { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void Complete()
    {
        if (Status == "Done") throw new InvalidOperationException("Task is already completed.");
        CompleteCore();
        if (Status != "Done") throw new InvalidOperationException("Post-condition failed: Status must be Done.");
    }
    protected abstract void CompleteCore();
}

public class StandardTask : TaskItem
{
    public StandardTask() { TaskType = "Standard"; }
    protected override void CompleteCore() { Status = "Done"; }
}

public class DeadlineTask : TaskItem
{
    public DateTime DueDate { get; set; }
    public DeadlineTask() { TaskType = "Deadline"; }
    protected override void CompleteCore() { Status = "Done"; }
}

public class RecurringTask : TaskItem
{
    public int RecurrenceInterval { get; set; }
    public DateTime? DueDate { get; set; }
    public RecurringTask() { TaskType = "Recurring"; }
    protected override void CompleteCore()
    {
        Status = "Done";
        if (DueDate.HasValue) DueDate = DueDate.Value.AddDays(RecurrenceInterval);
    }
}