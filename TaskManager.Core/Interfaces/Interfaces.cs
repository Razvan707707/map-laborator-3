using TaskManager.Core.Models;

namespace TaskManager.Core.Interfaces;

public interface ITaskReader
{
    IEnumerable<TaskItem> GetAll();
    TaskItem? GetById(int id);
}

public interface ITaskWriter
{
    void Add(TaskItem task);
    void Update(TaskItem task);
    void Delete(int id);
}

// Interfața veche acum doar le moștenește pe amândouă, 
// deci baza de date (Data) nu trebuie modificată absolut deloc!
public interface ITaskRepository : ITaskReader, ITaskWriter
{
}

public interface ITaskNotifier
{
    void Notify(TaskItem task);
}