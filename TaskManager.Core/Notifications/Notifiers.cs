using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;

namespace TaskManager.Core.Notifications;

public class EmailNotifier : ITaskNotifier
{
    public void Notify(TaskItem task) => Console.WriteLine($"Email sent to admin for task: {task.Title}");
}

public class ConsoleNotifier : ITaskNotifier
{
    public void Notify(TaskItem task) => Console.WriteLine($"CONSOLE ALERT: Task {task.Id} completed!");
}

public class FileLogNotifier : ITaskNotifier
{
    public void Notify(TaskItem task) => File.AppendAllText("tasks.log", $"[{DateTime.UtcNow}] Task {task.Id} completed.\n");
}
