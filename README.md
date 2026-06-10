# Task Manager - Refactorizare SOLID & IoC (Laboratorul 4)

Acest proiect demonstrează implementarea completă a celor 5 principii **SOLID** și utilizarea unui container **IoC (Inversion of Control)** pentru injectarea dependențelor într-o aplicație .NET 8 Web API.

## 🏛️ Arhitectura și Dependențele Proiectului

Proiectul este structurat astfel încât `TaskManager.Core` să fie complet izolat, respectând regula de aur: **Domeniul (Core) nu depinde de nimeni.**

```text
+-------------------+       +-------------------+
|                   |       |                   |
|  TaskManager.UI   | ----> | TaskManager.Core  | <---- ZERO dependențe externe
|  (IoC Container)  |       |                   |
|                   |       +-------------------+
+---------+---------+                 ^
          |                           |
          |                           |
          v                           |
+-------------------+                 |
|                   |                 |
| TaskManager.Data  | ----------------+
|    (SQLite)       |
|                   |
+-------------------+
🛠️ Decizii de design SOLID (Justificări)
S - Single Responsibility Principle (SRP)

Unde: TaskValidator, ReportService, clasele de Notifiers.

Problema rezolvată: Inițial, logica de validare și salvare se pot amesteca. Am izolat validarea datelor, salvarea în BD și generarea de rapoarte în clase diferite. Fiecare clasă are un singur motiv clar de a se schimba.

O - Open/Closed Principle (OCP)

Unde: Ierarhia de modele (TaskItem, DeadlineTask, RecurringTask) și sistemul de notificări.

Problema rezolvată: Pentru a adăuga un nou tip de task sau un nou mod de notificare (ex: SMS), adăugăm o clasă nouă care extinde interfețele existente. Nu modificăm codul de bază al serviciului principal.

L - Liskov Substitution Principle (LSP)

Unde: Metoda CompleteCore() din clasele derivate.

Problema rezolvată: Am creat post-condiții explicite (statusul trebuie să fie "Done"). Indiferent dacă folosim StandardTask sau RecurringTask, TaskService le poate completa pe toate la fel, fără să pice.

I - Interface Segregation Principle (ISP)

Unde: Extragerea ITaskReader și ITaskWriter din ITaskRepository.

Problema rezolvată: Clasa ReportService are nevoie doar să citească date pentru a genera un sumar. Prin crearea lui ITaskReader, am protejat baza de date (raportul nu va avea niciodată acces la metodele de .Add() sau .Delete()). ReportService primește prin constructor DOAR ce are nevoie.

D - Dependency Inversion Principle (DIP)

Unde: TaskService depinde de interfețe, nu de implementări. Program.cs se ocupă de asamblare (Composition Root).

Problema rezolvată: Modulele de nivel înalt (Core) nu depind de cele de nivel scăzut (Data / SQLite). Prin mutarea tuturor interfețelor în Core și injectarea cu containerul IoC din UI, decuplăm total codul. Astfel am putut scrie teste unitare folosind InMemoryTaskRepository în loc de o bază de date reală.
