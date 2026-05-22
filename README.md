# Birthday Reminder API

Timezone aware Web API designed to manage and send automated birthday email reminders. Built with .NET.

---

## Key Features

* **Timezone Aware Scheduling:** Supports global timezones using NodaTime (e.g. `Asia/Jakarta`, `Australia/Sydney`, `UTC`) ensuring users receive notifications at **9:00 AM local time** regardless of their location.
* **Leap Year Handling:** Built in domain extension logic to shift February 29th birthdays to February 28th on non leap years.
* **Background Worker:** A background service (`ReminderWorker`) that polls the database and dispatches notifications at the appropriate time with failure retry logic.
* **OpenAPI Documentation:** Interactive Swagger UI generated natively and hosted directly on startup.

---

## Tech Stack

* **Language/Framework:** C# / .NET 10 Web API
* **Database:** PostgreSQL via Entity Framework Core & Npgsql
* **Date & Time:** NodaTime
* **API Documentation:** Microsoft OpenAPI & Swagger UI
* **Testing:** xUnit & Moq
* **Deployment/Environment:** Docker Compose (PostgreSQL)

---

## Project Structure

```
├── API/                    # API Controllers and Endpoint Routing
├── Application/            # Core business logic interfaces, services, and DTOs
│   ├── Services/           # TimeZoneService, UserService
│   └── DTOs/               # CreateUserRequest, UpdateUserRequest
├── Domain/                 # Domain Entities (User, Reminder), Exceptions, and Extensions
│   ├── Entities/           # Core Domain Models
│   └── Extensions/         # DateOnlyExtensions (Leap year adjustment)
├── Infrastructure/         # DB Context, Repositories, and External Service Integrations
├── Workers/                # Background Hosted Services (ReminderWorker)
└── BirthdayReminder.Tests/ # Unit test suites (Application, Infrastructure, Workers)
```

---

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

### Step 1: Start the Database
Start the PostgreSQL container using Docker Compose:
```bash
docker compose up -d
```

### Step 2: Apply Migrations
Update the database schema with Entity Framework Core:
```bash
dotnet ef database update
```

### Step 3: Run the Application
Run the Web API:
```bash
dotnet run
```
Once the application starts, it will be listening at **`http://localhost:5271`**.

---

## API Documentation & Testing

### Interactive Swagger UI
Visiting the base URL will automatically redirect to the Swagger documentation:
**[http://localhost:5271/](http://localhost:5271/)**

### Running Unit Tests
Execute the tests for services, strategies, and background workers:
```bash
dotnet test
```
