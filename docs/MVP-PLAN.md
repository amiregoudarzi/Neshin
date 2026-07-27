# MVP plan

## Confirmed product decisions

- Customers order only while physically at the venue; discovery uses device location.
- A branch must explicitly activate `AcceptsAppOrders` in its web application.
- Online payment happens before the restaurant accepts the order.
- Discovery, browsing, venue visits, ordering, and order tracking work through an anonymous customer session.
- Phone number is optional contact data. OTP is required only when a customer later claims an account or verifies ownership of a phone number.
- Every branch has an independent menu.
- Pay-at-venue/POS is an optional branch setting and can be enabled or disabled by the restaurant.

## Main customer flow

1. Create or restore an anonymous customer session; no phone or OTP is required.
2. The PWA asks the browser for location permission and submits the coordinates to discovery.
3. Resolve zero, one, or multiple nearby branches. The API returns all candidates to support co-located venues.
4. Load the active branch's public profile, published menus/items, and events.
5. Optionally record a privacy-preserving venue visit without storing exact coordinates.
6. Create an idempotent order containing menu item ids and quantities only.
7. Revalidate branch state, menu publication, item availability, names, and prices on the write database.
8. Submit pay-at-venue orders to the venue queue. Online orders remain `AwaitingPayment` until a verified payment callback exists.
9. Let authorized branch staff accept, reject, prepare, mark ready, and complete the order.
10. Ask for a name/phone only when useful, and expose the phone to the branch only with explicit call consent.

## Delivery slices

1. Anonymous experience: secure customer session, discovery, storefront, visits, and order tracking.
2. Catalog/content: branch menu items, public profile, events, and owner administration UI.
3. Ordering: server-priced item snapshots, idempotency, optimistic concurrency, and venue workflow.
4. Branch operations: branch-scoped access, order queue, active visits, and CRM archive/restore.
5. Optional identity: request/verify OTP, claim a guest profile, and merge customer history.
6. Payments: gateway strategy, idempotent callback, verification, rejection/refund policy, and reconciliation.
7. Pilot hardening: staff roles, audit trail, observability, backup, and integration tests.

## Deferred from MVP

QR discovery, delivery, reservations, loyalty, wallet, complex discounts, inventory/accounting, microservices,
message broker, event sourcing, and direct external POS integration.
