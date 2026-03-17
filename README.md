# Fantasy League Manager: Docker & Cloud Integration

## Project Overview
This repository contains the final project for **IT435 (Current Topics in Software Development)** at Centralia College. It serves as an evolutionary step for the **Fantasy League Manager** initially developed in **IT410 (Advanced Data Access Techniques)**. 

The application has been transformed from a local Windows-based console app into a **fully containerized microservice** that connects to a live **Cloud SQL Server**.

---

## Key Features
* **Containerized Architecture**: The entire .NET 9 application is encapsulated within a Docker image for 100% portability.
* **Cloud Data Integration**: Replaced local database seeding with a live migration of 12 teams and 120 players to a cloud-hosted SmarterASP.net SQL Server.
* **Hybrid Data Access (DAL)**: 
    * **EF Core**: Used for complex relational mapping, eager loading (`.Include`), and CRUD operations.
    * **ADO.NET**: Utilized for high-performance raw SQL queries and parameterized position searches.
* **Audit Logging**: A robust `RosterLogs` system tracks player movements, trades, and history with full navigation property support in EF Core.

---

## Technical Stack
* **Language**: C# / .NET 9
* **ORM**: Entity Framework Core 9
* **Database**: Microsoft SQL Server (Cloud-hosted via SmarterASP.net)
* **Virtualization**: Docker & Docker Compose

---

## How to Run
This project is designed to be highly portable. You do not need SQL Server installed locally to run it.

### Prerequisites
* **Docker Desktop** installed and running.
* **Internet Connection** (to reach the Cloud SQL instance).

### Execution
1.  **Clone the Repository**:
    ```bash
    git clone [your-repository-link]
    cd FantasyLeagueManager
    ```
2.  **Build and Run**:
    ```powershell
    docker compose build --no-cache
    docker compose run --rm app
    ```

---

## Architecture & Design Patterns
* **Repository Pattern**: Abstracted data access via `IDataAccess<T>` to allow for swappable providers (ADO.NET vs. EF Core).
* **Dependency Injection**: Managed through standard .NET patterns to decouple the UI from the database logic.
* **Infrastructure as Code**: Using `docker-compose.yml` to define the environment and manage connection strings via environment variables.

---

## Future Enhancements
* **Extended Auditing**: Implement `OldTeamId` tracking in `RosterLogs` for a more detailed "Before/After" audit trail.
* **Web Integration**: Transition from a Console UI to a containerized ASP.NET Core Web API.
* **Testing**: Implement Unit Testing for the Repository layer using an In-Memory database.

---

**Developer**: Lee Houk  
**Course**: IT435 - Software Engineering
