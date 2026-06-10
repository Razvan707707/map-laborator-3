using Microsoft.Data.Sqlite;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;

namespace TaskManager.Data;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();
    private int _nextId = 1;

    public IEnumerable<TaskItem> GetAll() => _tasks.ToList();
    public TaskItem? GetById(int id) => _tasks.FirstOrDefault(t => t.Id == id);
    public void Add(TaskItem task) { task.Id = _nextId++; _tasks.Add(task); }
    public void Update(TaskItem task) { var index = _tasks.FindIndex(t => t.Id == task.Id); if (index != -1) _tasks[index] = task; }
    public void Delete(int id) => _tasks.RemoveAll(t => t.Id == id);
}

public class SqliteTaskRepository : ITaskRepository
{
    private readonly string _connectionString;

    public SqliteTaskRepository(string connectionString)
    {
        _connectionString = connectionString;
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT NOT NULL, Description TEXT, Status TEXT NOT NULL,
                Priority INTEGER NOT NULL, TaskType TEXT NOT NULL, NotificationType TEXT NOT NULL,
                DueDate TEXT, TaskRecurrenceInterval INTEGER, CreatedAt TEXT NOT NULL
            )";
        cmd.ExecuteNonQuery();
    }

    public void Add(TaskItem task)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Tasks (Title, Description, Status, Priority, TaskType, NotificationType, DueDate, TaskRecurrenceInterval, CreatedAt) 
                            VALUES ($title, $desc, $status, $prio, $type, $notif, $due, $interval, $created)";
        cmd.Parameters.AddWithValue("$title", task.Title);
        cmd.Parameters.AddWithValue("$desc", task.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$status", task.Status);
        cmd.Parameters.AddWithValue("$prio", task.Priority);
        cmd.Parameters.AddWithValue("$type", task.TaskType);
        cmd.Parameters.AddWithValue("$notif", task.NotificationType);
        cmd.Parameters.AddWithValue("$created", task.CreatedAt.ToString("O"));
        string? due = task is DeadlineTask dt ? dt.DueDate.ToString("O") : (task is RecurringTask rt && rt.DueDate.HasValue ? rt.DueDate.Value.ToString("O") : null);
        cmd.Parameters.AddWithValue("$due", due ?? (object)DBNull.Value);
        int? interval = task is RecurringTask r ? r.RecurrenceInterval : null;
        cmd.Parameters.AddWithValue("$interval", interval ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public TaskItem? GetById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Tasks WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read()) return MapReaderToTask(reader);
        return null;
    }

    public IEnumerable<TaskItem> GetAll()
    {
        var tasks = new List<TaskItem>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Tasks";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) tasks.Add(MapReaderToTask(reader));
        return tasks;
    }

    public void Update(TaskItem task)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Tasks SET Status = $status, DueDate = $due WHERE Id = $id";
        cmd.Parameters.AddWithValue("$status", task.Status);
        cmd.Parameters.AddWithValue("$id", task.Id);
        string? due = task is RecurringTask rt && rt.DueDate.HasValue ? rt.DueDate.Value.ToString("O") : null;
        cmd.Parameters.AddWithValue("$due", due ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Tasks WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private TaskItem MapReaderToTask(SqliteDataReader reader)
    {
        var type = reader.GetString(reader.GetOrdinal("TaskType"));
        TaskItem task = type switch
        {
            "Deadline" => new DeadlineTask { DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? DateTime.MinValue : DateTime.Parse(reader.GetString(reader.GetOrdinal("DueDate"))) },
            "Recurring" => new RecurringTask { RecurrenceInterval = reader.IsDBNull(reader.GetOrdinal("TaskRecurrenceInterval")) ? 0 : reader.GetInt32(reader.GetOrdinal("TaskRecurrenceInterval")), DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("DueDate"))) },
            _ => new StandardTask()
        };
        task.Id = reader.GetInt32(reader.GetOrdinal("Id"));
        task.Title = reader.GetString(reader.GetOrdinal("Title"));
        task.Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"));
        task.Priority = reader.GetInt32(reader.GetOrdinal("Priority"));
        task.NotificationType = reader.GetString(reader.GetOrdinal("NotificationType"));
        task.CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")));
        typeof(TaskItem).GetProperty("Status")!.SetValue(task, reader.GetString(reader.GetOrdinal("Status")));
        return task;
    }
}