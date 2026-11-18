# RaceDay Part 2 — Sporting Event Management REST API

## 1. Project overview

RaceDay is a C# ASP.NET Core RESTful Web API for managing sporting events and their participants. The application supports event management, participant registration, profile management, event categories, enrolments and results.

The API uses Microsoft SQL Server for data storage and Entity Framework Core for database access. Swagger UI provides interactive API documentation, while automated tests help verify selected application rules.

The application was developed as part of the RaceDay Part 2 software development assignment.

## 2. Project objectives

The main objectives are to:

- Develop a RESTful API using C# and ASP.NET Core.
- Connect the application to a Microsoft SQL Server database.
- Represent the database entities using Entity Framework Core models.
- Implement participant registration and login.
- Store passwords as hashes rather than plain text.
- Apply session-based authentication and role-based authorization.
- Support sporting event and event category management.
- Allow participants to enrol in eligible event categories.
- Support results management.
- Provide interactive API documentation.
- Include automated unit tests.
- Configure GitHub Actions to build and test the solution.
- Document installation, configuration and usage.

## 3. Technologies and tools

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| ASP.NET Core Web API | HTTP endpoints and application logic |
| .NET 10 | Application framework |
| Microsoft SQL Server | Relational database |
| Entity Framework Core | Object-relational mapping and database queries |
| ASP.NET Core Identity password hashing | Password hashing and verification |
| HTTP sessions | Session-based authentication state |
| Swagger UI / Swashbuckle | Interactive API documentation |
| xUnit | Unit testing |
| Git and GitHub | Source control and repository hosting |
| GitHub Actions | Automated build and test workflow |
| Visual Studio 2022 | Development environment |
| SQL Server Management Studio | Database administration |

## 4. System architecture

The application uses a controller-based Web API architecture.

A client sends an HTTP request to an API endpoint. The relevant controller validates the request, applies business rules and uses the Entity Framework Core database context to query or update SQL Server. The controller then returns an appropriate HTTP response.

The main components are:

- **Controllers:** Handle HTTP requests and responses.
- **DTOs:** Define the data accepted by selected endpoints.
- **Models:** Represent users, profiles, events, routes, categories, enrolments, results and weather snapshots.
- **Data:** Contains the Entity Framework Core database context.
- **Authorization:** Contains the custom session-based authorization attribute.
- **Tests:** Contains automated tests for selected validation, password hashing and authorization behaviour.

## 5. Database design

The application uses the `RaceDayDB` SQL Server database. The Entity Framework Core context is `RaceDayDbContext`.

The model classes include:

| Entity | Purpose |
|---|---|
| `User` | Stores account details, password hashes, roles and account status |
| `UserProfile` | Stores participant profile information |
| `Event` | Stores sporting event details |
| `Route` | Stores route distance and optional coordinate information |
| `EventCategory` | Stores event categories, distances, fees, age limits and capacity |
| `Enrolment` | Connects participants to event categories |
| `Result` | Stores finish times, positions and result status |
| `WeatherSnapshot` | Stores weather observations associated with events |

The relationships between these entities allow the application to associate users with profiles, organisers with events, events with categories, categories with enrolments, and enrolments with results.

The database schema and constraints should remain consistent with the approved Part 1 database design.

## 6. User roles and access control

The application uses two principal roles: Participant and Organiser.

### Participant

Participants can:

- Register an account and log in.
- View sporting events.
- Create and update their own profile.
- View their own enrolments.
- Enrol in eligible event categories.
- View results available to their account, subject to endpoint authorization.

### Organiser

Organisers can:

- Create, update and delete events they are authorized to manage.
- Manage event categories.
- View enrolments associated with their own events.
- Create, update and delete results associated with their own events.

Access is controlled through the custom `SessionAuthorizeAttribute`, which checks the session for the user's ID and role. Unauthenticated requests should receive HTTP 401, while requests from authenticated users without the required role should receive HTTP 403.

Public registration assigns the Participant role. Organiser accounts must be provisioned through an authorized setup or administrative process.

**Security note:** Endpoint-level ownership and role checks must be verified before claiming that all access-control rules are fully enforced.

## 7. Authentication and password security

Registration is provided through:

`POST /api/Auth/register`

Login is provided through:

`POST /api/Auth/login`

During registration, the application normalizes the email address, checks for an existing account, hashes the password and saves the new user.

During login, the application looks up the account and verifies the supplied password against the stored password hash. A successful login stores the user ID and role in the session.

The application uses a session timeout of 30 minutes.

For session-protected endpoints, clients must retain and send the session cookie returned after login. In the current configuration, the session cookie is marked HttpOnly and Secure.

For production deployment, HTTPS, session-cookie configuration, account protection, session lifecycle management and authorization rules should be reviewed carefully.

## 8. API endpoint overview

The following table summarises the controller routes implemented in the project. Available operations and access requirements should be confirmed against the current source code and Swagger UI.

| Controller | Endpoint | Purpose |
|---|---|---|
| Auth | `POST /api/Auth/register` | Register a participant |
| Auth | `POST /api/Auth/login` | Authenticate a user |
| Profiles | `GET /api/Profiles/me` | Retrieve the current user's profile |
| Profiles | `POST /api/Profiles` | Create the current user's profile |
| Profiles | `PUT /api/Profiles/me` | Update the current user's profile |
| Events | `GET /api/Events` | List events |
| Events | `GET /api/Events/{id}` | Retrieve an event |
| Events | `POST /api/Events` | Create an event |
| Events | `PUT /api/Events/{id}` | Update an event |
| Events | `DELETE /api/Events/{id}` | Delete an event |
| Event categories | `GET /api/EventCategories` | List event categories |
| Event categories | `GET /api/EventCategories/{id}` | Retrieve a category |
| Event categories | `POST /api/EventCategories` | Create a category |
| Event categories | `PUT /api/EventCategories/{id}` | Update a category |
| Event categories | `DELETE /api/EventCategories/{id}` | Delete a category |
| Routes | `GET /api/Routes` | List routes |
| Routes | `GET /api/Routes/{id}` | Retrieve a route |
| Enrolments | `GET /api/Enrolments` | Retrieve the current participant's enrolments |
| Enrolments | `GET /api/Enrolments/{id}` | Retrieve an individual enrolment |
| Enrolments | `GET /api/Enrolments/my` | Retrieve the current participant's enrolments |
| Enrolments | `POST /api/Enrolments` | Enrol in an event category |
| Enrolments | `GET /api/Enrolments/organiser` | Retrieve enrolments for the organiser's events |
| Results | `GET /api/Results` | Retrieve results |
| Results | `GET /api/Results/{id}` | Retrieve an individual result |
| Results | `POST /api/Results` | Create a result |
| Results | `PUT /api/Results/{id}` | Update a result |
| Results | `DELETE /api/Results/{id}` | Delete a result |

HTTP status codes communicate the outcome of API operations. These include 200 for successful retrieval, 201 for successful creation, 400 for invalid requests, 401 for unauthenticated requests, 403 for forbidden requests, 404 for resources not found, and 409 for conflicts.

## 9. Event enrolment rules

The enrolment functionality includes checks intended to ensure that:

- The requested event category exists.
- The event is published and still open for enrolment.
- The event has not passed its registration closing date.
- The category has available capacity.
- The participant has supplied a date of birth in their profile.
- The participant meets the category's minimum age.
- The participant has not already enrolled in the same category.

The enrolment request also records emergency contact details.

These rules should be tested with valid and invalid requests to verify the expected responses.

## 10. Swagger API documentation

Swagger UI provides an interactive interface for reviewing API endpoints, request parameters, schemas and responses.

To open Swagger UI:

1. Open the solution in Visual Studio.
2. Set `RaceDay.Api` as the startup project.
3. Confirm that the SQL Server connection string is correct.
4. Build and run the API.
5. Open the HTTPS URL displayed by Visual Studio and append `/swagger`.

Example:

`https://localhost:7029/swagger`

The local port may differ. Swagger UI is configured for the Development environment in the current application configuration.

## 11. Prerequisites

Install the following software before running the project:

- Visual Studio 2022 with the required development workload and .NET 10 SDK support.
- .NET 10 SDK.
- Microsoft SQL Server Express or another compatible SQL Server instance.
- SQL Server Management Studio (recommended).
- GitHub Desktop or Git command-line tools.

## 12. Installation and setup

### Step 1: Obtain the repository

Clone the RaceDay Part 2 repository from GitHub, or open the existing local repository.

### Step 2: Open the solution

Open the solution file in Visual Studio. Set `RaceDay.Api` as the startup project.

### Step 3: Configure the database

Ensure SQL Server is running and the `RaceDayDB` database has been created using the approved database schema and seed data.

Update the `RaceDayConnection` value in `appsettings.json` to match your SQL Server instance.

Example connection string:

```json
{
  "ConnectionStrings": {
    "RaceDayConnection": "Server=localhost\\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

The example assumes a local SQL Server Express instance named `SQLEXPRESS. Your server name may differ.

**Important:** Do not execute scripts that drop the database unless you intentionally want to delete and recreate it.

### Step 4: Restore dependencies

From the directory containing the solution file, run:

```bash
dotnet restore
```

### Step 5: Build the solution

Run:

```bash
dotnet build
```

Alternatively, use **Build → Build Solution** in Visual Studio.

### Step 6: Run the API

Start the `RaceDay.Api` project in Visual Studio. Open the Swagger URL shown by the application.

## 13. Example authentication requests

### Register a participant

Request:

```http
POST /api/Auth/register
Content-Type: application/json
```

Example body:

```json
{
  "email": "runner@example.com",
  "password": "ReplaceWithAStrongPassword123!"
}
```

The example credentials are illustrative only. Use a test account when demonstrating the application.

### Log in

Request:

```http
POST /api/Auth/login
Content-Type: application/json
```

Example body:

```json
{
  "email": "runner@example.com",
  "password": "ReplaceWithAStrongPassword123!"
}
```

A successful login establishes a session. Subsequent protected requests must include the session cookie. When using Swagger UI, confirm that the client retains the cookie; otherwise, use a client that supports cookie storage.

## 14. Unit testing

The solution contains a `RaceDay.Tests` project using xUnit.

The implemented tests include checks for:

- Registration request email validation.
- Password length validation.
- Valid registration request details.
- Password hashing and password verification.
- Unauthenticated access being rejected.
- Participants being denied access to Organiser-only endpoints.

Additional tests should cover event management, enrolment rules, result management and resource ownership to strengthen assignment coverage.

### Run tests in Visual Studio

1. Open **Test → Test Explorer**.
2. Select **Run All**.
3. Review the passed and failed test results.

### Run tests from the command line

From the directory containing the solution file, run:

```bash
dotnet test
```

The command builds and runs the tests, reporting any failures.

## 15. Continuous integration with GitHub Actions

The workflow file is located at:

`.github/workflows/dotnet.yml`

The workflow is configured to run on pushes, pull requests and manual workflow dispatches.

Its intended tasks are:

1. Check out the repository.
2. Install the .NET 10 SDK.
3. Restore project dependencies.
4. Build the solution in Release configuration.
5. Run the automated tests.

To view the workflow:

1. Open the repository on GitHub.
2. Select the **Actions** tab.
3. Open **RaceDay API CI**.
4. Select the latest workflow run.
5. Review the build and test results.

A green check indicates that the workflow completed successfully. A red cross indicates that one or more steps failed and need investigation.

The workflow must be successfully executed on GitHub before claiming that continuous integration has been verified.

## 16. Source control and version history

Git and GitHub are used to track project changes.

Meaningful commits should describe individual changes, such as:

- Adding database models.
- Configuring Entity Framework Core.
- Implementing registration and login.
- Adding session authorization.
- Implementing event management.
- Adding enrolment validation.
- Implementing result management.
- Adding unit tests.
- Configuring GitHub Actions.
- Documenting installation and usage.

The repository history should reflect the actual development process and meet the assignment's requirement for at least 20 meaningful commits.

## 17. Known limitations and further improvements

The following items should be reviewed or completed before final submission:

- Verify that all controller authorization attributes and resource ownership checks are correct.
- Confirm that participants cannot access other participants' private enrolment or profile information.
- Confirm that organisers can manage only their own events and related records.
- Expand automated tests to cover event management, role authorization and enrolment scenarios.
- Resolve any remaining build warnings, including database decimal precision warnings.
- Remove temporary diagnostic endpoints that are no longer required.
- Confirm that the GitHub Actions workflow passes on the remote repository.
- Ensure that database setup instructions match the final schema and connection configuration.
- Review session security and deployment configuration before any public deployment.


## 18. References

The following sources provide supporting technical documentation for the frameworks and tools used in the project. The reference format follows the IIE Harvard–Anglia approach. The access date should reflect the date on which each source was actually consulted.

GitHub, n.d. *GitHub Actions documentation*. Available at: https://docs.github.com/en/actions [Accessed: 19 October 2026].

Microsoft, n.d. *Create web APIs with ASP.NET Core*. Available at: https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed: 19 October 2026].

Microsoft, n.d. *Entity Framework Core*. Available at: https://learn.microsoft.com/en-us/ef/core/ [Accessed: 19 October 2026].

Microsoft, n.d. *dotnet test command*. Available at: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test [Accessed: 19 October 2026].

The Independent Institute of Education (The IIE), 2026. *Harvard–Anglia style reference guide: adapted for The IIE*. Available through the IIE Library referencing and plagiarism resources: https://iielibraryconnect.iie.ac.za/e-shelf/referencing-and-plagiarism [Accessed: 19 October 2026].

## 20. Project links

**GitHub repository:** [Insert repository URL]

**Swagger UI:** Available locally when the API is running.

**YouTube demonstration:** [Insert published video URL]

**GitHub Actions:** Open the repository's Actions tab to review automated build and test runs.

---

*This README documents the intended operation and current project structure. Verify the final implementation, test results, workflow status, repository URL and demonstration video before submission.*
