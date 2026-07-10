# Authentication System

A simple authentication API built with ASP.NET Core following a Clean Architecture approach. The project demonstrates secure user authentication using JWT access tokens, refresh tokens, ASP.NET Core Identity, email confirmation, and Entity Framework Core.

## Features

* User registration
* Email confirmation
* Resend confirmation email
* User login
* JWT authentication
* Refresh token support
* Refresh token rotation
* Secure logout
* Protected endpoints
* Global exception handling middleware
* Request validation with FluentValidation

## Technologies

* ASP.NET Core Web API
* C#
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* JWT Authentication
* MailKit & MimeKit
* FluentValidation
* Swagger

## Project Structure

```
Auth.API
Auth.Application
Auth.Domain
Auth.Infrastructure
```

The solution follows a simple Clean Architecture approach:

* **Auth.API** – Controllers, middleware, validation, and application startup.
* **Auth.Application** – DTOs, interfaces, and application contracts.
* **Auth.Domain** – Domain entities.
* **Auth.Infrastructure** – Database, Identity, email, and service implementations.

## Getting Started

1. Clone the repository.
2. Update the connection string in `appsettings.json`.
3. Configure the JWT settings.
4. Configure the Email settings (or use MailHog for local testing).
5. Apply the database migrations:

```bash
dotnet ef database update
```

6. Run the application:

```bash
dotnet run
```

7. Open Swagger or use Postman to test the API.

## API Endpoints

| Method | Endpoint                        | Description                                       |
| ------ | ------------------------------- | ------------------------------------------------- |
| POST   | `/api/Auth/register`            | Register a new user                               |
| GET    | `/api/auth/confirm-email`       | Confirm user email                                |
| POST   | `/api/auth/resend-confirmation` | Send a new confirmation email                     |
| POST   | `/api/Auth/login`               | Login and receive JWT and refresh token           |
| POST   | `/api/Auth/refresh-token`       | Generate a new access token using a refresh token |
| POST   | `/api/Auth/logout`              | Invalidate the current refresh token              |
| GET    | `/api/Auth/protected`           | Example protected endpoint                        |

## Notes

* Passwords are securely hashed using ASP.NET Core Identity.
* Access tokens are issued as JWTs.
* Refresh tokens are stored in the database and rotated after every successful refresh.
* Login requires a confirmed email address.
* MailHog can be used during development to test email confirmation without sending real emails.

## Purpose

This project was built as a learning project to practice backend development with ASP.NET Core, Clean Architecture, JWT authentication, refresh tokens, email confirmation, and modern authentication techniques.
