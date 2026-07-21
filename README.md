# Neshin

Neshin is a location-based, in-venue ordering platform for cafes and restaurants.

## Architecture

- .NET 10 Minimal REST API
- Clean Architecture: Domain, Application, Infrastructure, API
- CQRS with explicit command/query contracts and handlers
- PostgreSQL through EF Core
- Separate read and write DbContexts and repositories
- Unit of Work on the write side

Both DbContexts currently use the same PostgreSQL database. The read context is optimized for
no-tracking queries. Consistency-critical reads (for example, immediately before payment or invoice
creation) must use a write repository and the write context.

## Business rules captured in the model

- A branch must explicitly enable app ordering before an order can be placed.
- Each branch owns an independent menu.
- Customers are identified by a verified phone number; OTP delivery is represented by an application
  boundary and will be implemented separately.
- Online payment is the default. A branch may enable or disable pay-at-venue/POS independently.
- In-venue discovery is location-only for the MVP.

## Run locally

1. Start PostgreSQL with `docker compose up -d`.
2. Restore packages with `dotnet restore`.
3. Apply migrations after the first migration is created.
4. Run `dotnet run --project src/Neshin.Api`.

The API exposes `/health` and OpenAPI in the Development environment.
