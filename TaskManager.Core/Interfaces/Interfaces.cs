using TaskManager.Core.Models;

namespace TaskManager.Core.Interfaces;

public interface ITaskRepository
{
    IEnumerable<TaskItem> GetAll();
    TaskItem? GetById(int id);
    void Add(TaskItem task);
    void Update(TaskItem task);
    void Delete(int id);
}

public interface ITaskNotifier
{
    void Notify(TaskItem task);
}