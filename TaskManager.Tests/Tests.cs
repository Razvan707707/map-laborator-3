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
    public void CompleteTask_CallsInjectedNotifier()
    {
        var task = new StandardTask { Title = "Test", NotificationType = "Mock" };
        _repo.Add(task);
        _service.CompleteTask(task.Id);
        var mock = (MockNotifier)_notifiers["Mock"];
        Assert.That(mock.WasCalled, Is.True);
    }

    [Test]
    public void Validator_EmptyTitle_ThrowsException()
    {
        var task = new StandardTask { Title = "" };
        Assert.Throws<ArgumentException>(() => _validator.Validate(task));
    }

    [Test]
    public void Validator_LongTitle_ThrowsException()
    {
        var task = new StandardTask { Title = new string('A', 201) };
        Assert.Throws<ArgumentException>(() => _validator.Validate(task));
    }

    [Test]
    public void Validator_PastDeadline_ThrowsException()
    {
        var task = new DeadlineTask { Title = "Test", DueDate = DateTime.UtcNow.AddDays(-1) };
        Assert.Throws<ArgumentException>(() => _validator.Validate(task));
    }

    [Test]
    public void Repository_AddAndGet_WorksCorrectly()
    {
        var task = new StandardTask { Title = "Test" };
        _repo.Add(task);
        var fetched = _repo.GetById(task.Id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Title, Is.EqualTo("Test"));
    }
}