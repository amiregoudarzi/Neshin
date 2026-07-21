# Neshin architecture rules

## Direction of dependencies

```text
Api -> Application <- Infrastructure
          |
          v
        Domain
```

- Domain contains business behavior and has no dependency on EF Core, ASP.NET Core, or infrastructure.
- Application coordinates use cases through Commands and Queries and owns abstraction contracts.
- Infrastructure implements persistence and external integration contracts.
- API is the composition root and exposes versioned REST endpoints through Minimal API.

## CQRS and persistence

- Commands mutate aggregates through write repositories and commit through `IUnitOfWork`.
- Queries return projections through read repositories. Domain aggregates are not returned by query endpoints.
- `NeshinWriteDbContext` uses normal tracking and owns migrations.
- `NeshinReadDbContext` defaults to `NoTracking`.
- Both contexts initially use the same PostgreSQL database. This is logical CQRS, not premature physical replication.
- A consistency-sensitive read must use a write repository. Examples include the final price/menu check before
  payment, duplicate-payment checks, invoice declaration, and any read-your-own-write workflow.
- Repositories are aggregate/use-case specific. Do not introduce a generic repository that hides EF Core query
  capabilities or business intent.

## Open/Closed and integrations

External concerns are application interfaces with replaceable implementations. OTP, token generation, payment
gateways, SMS delivery, and later POS integration must be added behind focused contracts. The future custom token
project implements `ITokenService`; Domain and Application must not reference that project.

Use design patterns only when they remove an observed source of change:

- Strategy for multiple payment gateways or discovery policies.
- Factory when aggregate creation requires coordinated validation.
- Adapter for SMS, payment, or POS providers.
- Outbox only when reliable external event delivery becomes necessary.

## Module ownership

- Clients: clients, branches, branch settings, and staff access.
- Catalog: independent branch menus, categories, items, prices, availability, featured/new markers.
- Discovery: location-based branch matching and ambiguity handling.
- Ordering: carts/orders, immutable item snapshots, payments, and status transitions.
- Identity: phone-number users, OTP verification, and authentication contracts.

The initial deployment is a modular monolith. Module boundaries should be represented in namespaces, database
schemas, contracts, and use cases before considering separate services.
