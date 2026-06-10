using NUnit.Framework;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using TaskManager.Data;

namespace TaskManager.Tests;

public class MockNotifier : ITaskNotifier
{
    public bool WasCalled { get; private set; }
    public void Notify(TaskItem task) => WasCalled = true;
}

[TestFixture]
public class TaskServiceTests
{
    private InMemoryTaskRepository _repo;
    private TaskValidator _validator;
    private Dictionary<string, ITaskNotifier> _notifiers;
    private TaskService _service;

    [SetUp]
    public void Setup()
    {
        _repo = new InMemoryTaskRepository();
        _validator = new TaskValidator();
        _notifiers = new Dictionary<string, ITaskNotifier> { { "Mock", new MockNotifier() } };
        _service = new TaskService(_repo, _validator, _notifiers);
    }

    // --- TESTE LAB 4 NOI ---

    [Test]
    public void ReportService_WithInMemoryRepo_GeneratesCorrectSummary()
    {
        // Demonstrează ISP: ReportService acceptă InMemoryTaskRepository pe post de ITaskReader
        _repo.Add(new StandardTask { Title = "Task 1" });
        var task2 = new StandardTask { Title = "Task 2", NotificationType = "None" };
        _repo.Add(task2);

        _service.CompleteTask(task2.Id);

        var reportService = new ReportService(_repo);
        var summary = reportService.GenerateSummary();

        Assert.That(summary, Is.EqualTo("Total tasks: 2, Completed: 1"));
    }

    [Test]
    public void TaskService_NullRepository_ThrowsArgumentNullException()
    {
        // Demonstrează DIP: Dependențele sunt stricte și injectate
        Assert.Throws<ArgumentNullException>(() => new TaskService(null!, _validator, _notifiers));
    }

    [TestCase("Email")]
    [TestCase("Console")]
    [TestCase("FileLog")]
    public void CompleteTask_CallsAppropriateNotifier(string notifType)
    {
        // Test parametrizat pentru notificatori
        var mockNotifier = new MockNotifier();
        var testNotifiers = new Dictionary<string, ITaskNotifier> { { notifType, mockNotifier } };

        var task = new StandardTask { Title = "Test", NotificationType = notifType };
        _repo.Add(task);

        var testService = new TaskService(_repo, _validator, testNotifiers);
        testService.CompleteTask(task.Id);

        Assert.That(mockNotifier.WasCalled, Is.True);
    }

    // --- TESTE LAB 3 (Păstrate ca să rămână 100% funcțional) ---

    [TestCase(typeof(StandardTask))]
    [TestCase(typeof(DeadlineTask))]
    [TestCase(typeof(RecurringTask))]
    public void Complete_ProducesDoneStatus_ForAllTypes(Type taskType)
    {
        var task = (TaskItem)Activator.CreateInstance(taskType)!;
        task.Title = "Test";
        task.NotificationType = "None";
        _repo.Add(task);
        _service.CompleteTask(task.Id);
        Assert.That(_repo.GetById(task.Id)!.Status, Is.EqualTo("Done"));
    }

    [Test]
    public void Complete_AlreadyDone_ThrowsException()
    {
        var task = new StandardTask { Title = "Test", NotificationType = "None" };
        _repo.Add(task);
        _service.CompleteTask(task.Id);
        Assert.Throws<InvalidOperationException>(() => _service.CompleteTask(task.Id));
    }

    [Test]
    public void Validator_EmptyTitle_ThrowsException()
    {
        var task = new StandardTask { Title = "" };
        Assert.Throws<ArgumentException>(() => _validator.Validate(task));
    }
}