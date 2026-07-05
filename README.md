# Authentication System

A simple authentication API built with ASP.NET Core using Clean Architecture. The project demonstrates user authentication with JWT access tokens and refresh tokens, along with ASP.NET Core Identity and Entity Framework Core.

## Features

* User registration
* User login
* JWT authentication
* Refresh token support
* Refresh token rotation
* Logout
* Protected endpoints
* Global exception handling
* Request validation with FluentValidation

## Technologies

* ASP.NET Core Web API
* C#
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* JWT Authentication
* FluentValidation
* Swagger

## Project Structure

```text
Auth.API
Auth.Application
Auth.Domain
Auth.Infrastructure
```

The solution follows a simple Clean Architecture approach:

* **Auth.API** – Controllers, middleware, validation, and application startup.
* **Auth.Application** – DTOs, interfaces, and application contracts.
* **Auth.Domain** – Domain entities.
* **Auth.Infrastructure** – Database, Identity, and service implementations.

## Getting Started

1. Clone the repository.
2. Update the connection string in `appsettings.json`.
3. delete existing migrations
4. Apply the database migrations:

```bash
dotnet ef database update
```

4. Run the application:

```bash
dotnet run
```

5. Open Swagger or use Postman to test the endpoints.

## API Endpoints

| Method | Endpoint                  | Description                                       |
| ------ | ------------------------- | ------------------------------------------------- |
| POST   | `/api/Auth/register`      | Register a new user                               |
| POST   | `/api/Auth/login`         | Login and receive JWT + refresh token             |
| POST   | `/api/Auth/refresh-token` | Generate a new access token using a refresh token |
| POST   | `/api/Auth/logout`        | Invalidate the current refresh token              |
| GET    | `/api/Auth/protected`     | Example protected endpoint                        |

## Notes

Passwords are managed using ASP.NET Core Identity, and access tokens are generated as JWTs. Refresh tokens are stored in the database and rotated after each successful refresh request.

This project was built as a learning project to practice backend development with ASP.NET Core and modern authentication techniques.
