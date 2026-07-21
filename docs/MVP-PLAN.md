# MVP plan

## Confirmed product decisions

- Customers order only while physically at the venue; discovery uses device location.
- A branch must explicitly activate `AcceptsAppOrders` in its web application.
- Online payment happens before the restaurant accepts the order.
- Customers are identified by Iranian mobile number and OTP is mandatory.
- Every branch has an independent menu.
- Pay-at-venue/POS is an optional branch setting and can be enabled or disabled by the restaurant.

## Main customer flow

1. Verify phone number with OTP.
2. Submit location and discover the branch.
3. Confirm that the branch is active and currently accepts app orders.
4. Load its published menu.
5. Create an order and snapshot selected item names and prices.
6. Revalidate availability and prices with the write side.
7. Select online payment, or POS only when enabled for the branch.
8. For online payment, verify the gateway callback before moving to `Paid`.
9. Let restaurant staff accept and process the paid order.

## Delivery slices

1. Identity: request/verify OTP, user persistence, custom token implementation.
2. Clients: client/branch onboarding and ordering/payment switches.
3. Catalog: branch menu administration and customer menu query.
4. Discovery: nearest active branch with an explicit confidence/radius policy.
5. Ordering: draft, item snapshot, consistency validation, submit, and status workflow.
6. Payments: gateway strategy, idempotent callback, verification, and reconciliation.
7. Pilot hardening: authorization, audit trail, rate limiting, observability, backup, and integration tests.

## Deferred from MVP

QR discovery, delivery, reservations, loyalty, wallet, complex discounts, inventory/accounting, microservices,
message broker, event sourcing, and direct external POS integration.
