using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Core.Notifications;
using TaskManager.Core.Services;
using TaskManager.Data;

var builder = WebApplication.CreateBuilder(args);

// CERINȚA 3: Înregistrarea dependențelor în containerul IoC (Fără "new" manual)
builder.Services.AddSingleton<ITaskRepository>(sp => new SqliteTaskRepository("Data Source=tasks.db"));
builder.Services.AddSingleton<ITaskReader>(sp => sp.GetRequiredService<ITaskRepository>());

builder.Services.AddTransient<TaskValidator>();
builder.Services.AddTransient<TaskService>();
builder.Services.AddTransient<ReportService>();

// Factory pentru Dicționarul de notificatori
builder.Services.AddSingleton<IReadOnlyDictionary<string, ITaskNotifier>>(sp =>
    new Dictionary<string, ITaskNotifier>
    {
        { "Email", new EmailNotifier() },
        { "Console", new ConsoleNotifier() },
        { "FileLog", new FileLogNotifier() }
    });

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

// Endpoint nou ca să putem testa ReportService direct din browser
app.MapGet("/tasks/report", (ReportService reportService) => {
    return Results.Ok(reportService.GenerateSummary());
});

app.Run();