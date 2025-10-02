# Repository Guidelines - UBNT SecPilot (.NET 8 + Orleans)

## Project Structure & Module Organization
Configuration defaults are defined in `configs/settings/base.yaml`, with optional environment-specific overrides (set `UBNT_ENV`) and finally environment variables in `.env`. Project dependencies are managed in `.csproj` files and global packages. The presentation layer consists of `UbntSecPilot.WebApi/` (ASP.NET Core API) and `UbntSecPilot.BlazorUI/` (Blazor Server UI). Core business rules reside in `UbntSecPilot.Domain/` (models, repositories) and must remain framework-agnostic. `UbntSecPilot.Application/` orchestrates services and agents. Concrete integrations (Kafka/Redpanda, MongoDB, HTTP clients, metrics) are in `UbntSecPilot.Infrastructure/`. Tests are in `**/Tests/` directories and additional documentation in `docs/`. Preserve these boundaries when evolving the system.

## Architecture Overview
- **Runtime**: .NET 8
- **Distributed Actors**: Microsoft Orleans (grains) in `UbntSecPilot.Agents.Orleans/`
- **API Layer**: ASP.NET Core in `UbntSecPilot.WebApi/`
- **UI Layer**: Blazor Server in `UbntSecPilot.BlazorUI/`
- **Domain Layer**: Business models and contracts in `UbntSecPilot.Domain/`
- **Application Layer**: Use cases and orchestration in `UbntSecPilot.Application/`
## Build, Test, and Development Commands
Ensure .NET 8 SDK is installed before development: verify with `dotnet --version`. Restore dependencies with `./build.sh restore` or `dotnet restore`. Build the application with `./build.sh build Debug` for development or `./build.sh build Release` for production. With the infrastructure stack running (`docker compose up redpanda mongo`), start the API with `./build.sh debug api` or the Blazor UI with `./build.sh debug blazor`. Run both together with `./build.sh debug full`. Execute tests with `./build.sh test` or target specific projects with `dotnet test UbntSecPilot.Application.Tests/UbntSecPilot.Application.Tests.csproj`. Format code with `dotnet format` and validate with `./build.sh build`. The default bootstrap servers are `localhost:9092`, MongoDB URI includes `localhost:27017`, the intelligence engine defaults to `spark` (with optional fallback to heuristics), and log level can be adjusted via `Logging:LogLevel` in `appsettings.Development.json`.

## Coding Style & Naming Conventions
Follow C# coding standards with PascalCase for public methods and properties, camelCase for private fields, and descriptive naming. Models in `UbntSecPilot.Domain/` should use record types or classes with clear intent through XML documentation. Service classes in `UbntSecPilot.Application/` remain thin and delegate side effects to infrastructure adapters. Orleans grains in `UbntSecPilot.Agents.Orleans/` inherit from `Grain` base class; name new grains `<Capability>Grain` and register them in the silo configuration. Use dependency injection throughout and maintain immutability where possible.

## Testing Guidelines
Add unit tests that mirror the target layer (`UbntSecPilot.Application.Tests/`, `UbntSecPilot.Orleans.Tests/`). Use xUnit or NUnit with Moq for mocking dependencies instead of relying on real services. Validate grain behavior with Orleans TestHost, include regression tests when evolving pipelines, and cover metrics/decisions produced. Integration tests should use TestContainers for Kafka/MongoDB/Spark/Prometheus simulation.

## Commit & Pull Request Guidelines
Author imperative, concise commit subjects ("Add threat enrichment agent grain"), with optional bodies explaining context or follow-ups. Group infrastructure, domain, and agent changes logically so diffs remain reviewable. Pull requests should describe intent, list manual or automated test output (`dotnet test`, API endpoint testing), and include screenshots if the Blazor UI changes. Link relevant issues and call out configuration migrations or new environment variables.

## Security & Configuration Tips
Keep API keys, connection strings, and sensitive configuration out of version control. Extend `appsettings.json` when introducing new configuration sections and document agent adjustments in `docs/`. Use mocked adapters for demos and anonymize logs from Kafka/Redpanda and MongoDB before sharing them. Use ASP.NET Core Data Protection for sensitive data and follow OWASP guidelines for API security.
