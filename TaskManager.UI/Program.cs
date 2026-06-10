using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Core.Notifications;
using TaskManager.Core.Services;
using TaskManager.Data;

var builder = WebApplication.CreateBuilder(args);

var notifiers = new Dictionary<string, ITaskNotifier>
{
    { "Email", new EmailNotifier() },
    { "Console", new ConsoleNotifier() },
    { "FileLog", new FileLogNotifier() }
};

var repository = new SqliteTaskRepository("Data Source=tasks.db");
var validator = new TaskValidator();
var taskService = new TaskService(repository, validator, notifiers);

builder.Services.AddSingleton(taskService);

var app = builder.Build();

app.MapPost("/tasks/standard", (StandardTask task, TaskService service) => {
    service.AddTask(task);
    return Results.Ok();
});

app.MapPost("/tasks/{id}/complete", (int id, TaskService service) => {
    try
    {
        service.CompleteTask(id);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();AbandonedMutexException f;