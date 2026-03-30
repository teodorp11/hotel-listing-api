# Hotel Listing API

A robust, production-ready RESTful Web API built with **.NET 10** for managing hotels, countries, and bookings. This API follows modern architecture practices and comes pre-configured with industry-standard patterns for security, documentation, logging, and monitoring.

## Features

- **Entity Framework Core**: Code-first database management using SQL Server.
- **Identity & Authentication**: Complete user management with ASP.NET Core Identity. Supports multiple authentication schemes:
  -  JWT (JSON Web Token)
  -  API Key Authentication
  -  Basic Authentication
- **API Versioning**: Full support for API versioning (v1, v2) via URL segments.
- **Robust Documentation**: Integrated Swagger UI with OpenAPI standards, XML comments, and example filters.
- **Advanced Logging**: Structured logging implemented via **Serilog** (Console and File sinks).
- **Health Checks**: Built-in health monitoring endpoints (`/healthz`, `/healthz/live`, `/healthz/ready`) for application and database status.
- **Rate Limiting**: Configured fixed-window rate limiting to protect endpoints from abuse.
- **Data Mapping**: Object-object mapping utilizing **AutoMapper**.
- **Global Error Handling**: Centralized exception handling to prevent sensitive data leakage.

## Project Structure

The solution follows a clean, modular architecture separated into distinct projects:

- `HotelListing.API` - The presentation layer (Controllers, Middleware, Configuration).
- `HotelListing.API.Application` - Application logic, DTOs, and interface definitions.
- `HotelListing.API.Domain` - Core domain entities (Models).
- `HotelListing.API.Common` - Shared constants, configuration models, and utilities.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB or full instance)
- A Code Editor like Visual Studio 2022, VS Code, or JetBrains Rider

## Configuration & Setup

### 1. Clone the repository

```bash
git clone https://github.com/teodorp11/hotel-listing-api.git
cd HotelListing.API
```

### 2. Configure AppSettings
Update your [appsettings.json] and [appsettings.Development.json]

```json
"ConnectionStrings": {
  "HotelListingDbConnectionString": "Server=(localdb)\\mssqllocaldb;Database=HotelListingDb;Trusted_Connection=True;MultipleActiveResultSets=true"
},
"JwtSettings": {
  "Issuer": "your-issuer",
  "Audience": "your-audience",
  "Key": "your-super-secret-key-that-is-long-enough"
}
```

### 3. Apply Database Migrations
Ensure your database is set up and up to date.

```bash
dotnet ef database update --project HotelListing.API
```

### 4. Run the Application

```bash
dotnet run --project HotelListing.API
```

## API Documentation
Once the application is running in a development environment, navigate to the Swagger UI to explore and test the available endpoints:

- **Swagger UI** - `https://localhost:<port>/swagger`
- **Health Check Status** - `https://localhost:<port>/healthz`

*Note: Replace `<port>` with the HTTPS port defined in `Properties/launchSettings.json` (under `profiles` -> `https` -> `applicationUrl`).*

### Authentication in Swagger
To test secure endpoints using Swagger:

1. Obtain a token by calling the appropriate login/authentication endpoint.
2. Click the **Authorize** lock button in the top right.
3. Enter `Bearer <your-token-here>` into the JWT scheme input, or use your API key in the `X-Api-Key` scheme.

## Security & Authentication Schemes
- **JWT Bearer**: Standard authorization header using the Bearer schema.
- **X-Api-Key**: Header-based API key authentication.
- **Basic**: Standard basic authentication protocol.
*(Note: Route definitions dictate which scheme is required)*

## Logging
Logs are written to the console natively and stored in rolling files via **Serilog**. Check the `/Logs` folder in the API project root for daily rotating log output (`log-YYYYMMDD.txt`).

## License
This project is licensed under the [MIT License](LICENSE.txt).