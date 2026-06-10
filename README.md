# Task Manager - SOLID Principles (Laboratorul 3)

Acest proiect este o aplicație de tip Task Manager dezvoltată în C# (.NET 8), având ca scop principal aplicarea și demonstrarea celor 5 principii **SOLID** în arhitectura software. Proiectul folosește o bază de date SQLite și include teste unitare (NUnit) pentru validarea logicii.

## 🛠️ Cum au fost aplicate principiile SOLID în acest cod:

* **S - Single Responsibility Principle (SRP)**
  Clasele au o singură responsabilitate clară. De exemplu, `TaskValidator` se ocupă strict de validarea datelor, `SqliteTaskRepository` se ocupă doar de salvarea în baza de date, iar `EmailNotifier` doar de trimiterea notificărilor. Nu am amestecat validarea cu salvarea în aceeași clasă.

* **O - Open/Closed Principle (OCP)**
  Sistemul este deschis pentru extindere, dar închis pentru modificare. De exemplu, am creat o clasă de bază abstractă `TaskItem`. Pentru a adăuga tipuri noi de task-uri (`StandardTask`, `DeadlineTask`, `RecurringTask`), am creat clase noi care extind baza, fără să fiu nevoit să modific codul existent din `TaskItem`.

* **L - Liskov Substitution Principle (LSP)**
  Orice clasă derivată poate înlocui clasa de bază fără a strica funcționalitatea. Am implementat metoda `CompleteCore()` în clasele derivate, asigurându-mă că post-condițiile (ex: statusul trebuie să devină "Done") sunt respectate indiferent de tipul specific al task-ului.

* **I - Interface Segregation Principle (ISP)**
  Am folosit interfețe mici și specifice, care nu forțează clasele să implementeze metode de care nu au nevoie. De exemplu: `ITaskRepository` pentru manipularea datelor și `ITaskNotifier` exclusiv pentru sistemul de alerte.

* **D - Dependency Inversion Principle (DIP)**
  Clasa principală de logică (`TaskService`) nu depinde de implementări concrete (cum ar fi `SqliteTaskRepository`), ci depinde de abstracții (interfețele `ITaskRepository` și `ITaskNotifier`). Acest lucru a permis injectarea ușoară a unui `InMemoryTaskRepository` în timpul testelor (Dependency Injection).

## 🚀 Tehnologii folosite:
* C# / .NET 8 (Web API)
* SQLite (Microsoft.Data.Sqlite)
* NUnit & NUnit3TestAdapter pentru Unit Testing
