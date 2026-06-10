using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;

namespace TaskManager.Core.Services;

public class TaskValidator
{
    public void Validate(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Title)) throw new ArgumentException("Title cannot be empty.");
        if (task.Title.Length > 200) throw new ArgumentException("Title length cannot exceed 200 characters.");
        if (task is DeadlineTask dt && dt.DueDate <= DateTime.UtcNow)
            throw new ArgumentException("DueDate must be in the future for a DeadlineTask.");
    }
}

public class TaskService
{
    private readonly ITaskRepository _repository;
    private readonly TaskValidator _validator;
    private readonly IReadOnlyDictionary<string, ITaskNotifier> _notifiers;

    public TaskService(ITaskRepository repository, TaskValidator validator, IReadOnlyDictionary<string, ITaskNotifier> notifiers)
    {
        // DIP: Protejăm serviciul cerând explicit interfețele în constructor
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _notifiers = notifiers ?? throw new ArgumentNullException(nameof(notifiers));
    }

    public void AddTask(TaskItem task)
    {
        _validator.Validate(task);
        _repository.Add(task);
    }

    public void CompleteTask(int id)
    {
        var task = _repository.GetById(id) ?? throw new KeyNotFoundException("Task not found.");
        task.Complete();
        _repository.Update(task);
        if (_notifiers.TryGetValue(task.NotificationType, out var notifier)) notifier.Notify(task);
    }
}

// NOU: ReportService (Cerinta 1 - ISP)
public class ReportService
{
    private readonly ITaskReader _reader;

    public ReportService(ITaskReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public string GenerateSummary()
    {
        var tasks = _reader.GetAll();
        int total = tasks.Count();
        int done = tasks.Count(t => t.Status == "Done");
        return $"Total tasks: {total}, Completed: {done}";
    }
}