# Customer and branch-owner API workflows

## Customer flow

1. `POST /api/v1/customer-sessions`
   - Returns `customerId`, a one-time `accessToken`, and expiry.
   - Also sets a `HttpOnly`, `SameSite=Strict` cookie for a same-origin PWA. Native/non-browser clients can send the
     returned token as `X-Customer-Session` on customer writes and private reads.
2. The PWA calls the browser Geolocation API after a user gesture. Geolocation requires HTTPS outside localhost.
3. `POST /api/v1/discovery/resolve`
   - Body: `latitude`, `longitude`, `accuracyMeters`, and optional `radiusMeters` (25-1000).
   - Returns `resolution: none|single|multiple`, an optional `suggestedBranchId`, and sorted `matches`.
   - Coordinates are intentionally sent in a body rather than a URL and are not persisted.
4. `GET /api/v1/public/branches/{branchId}`
   - Returns the active venue profile, published menus/available items, and upcoming published events.
5. `POST /api/v1/customer/visits`
   - Requires `X-Customer-Session`.
   - Sends coordinates and accuracy. The server recomputes distance, then records only branch, distance, and accuracy—not latitude/longitude.
6. `POST /api/v1/customer/orders`
   - Requires `X-Customer-Session` and a unique `Idempotency-Key` header.
   - Body contains `branchId`, `paymentMethod`, item ids/quantities, and optional contact details/consent.
   - The server loads names and prices from the write database; clients never submit prices.
7. `GET /api/v1/customer/orders/{orderId}`
   - Requires the same customer session and returns only an order owned by that customer.

An active visit from the last 15 minutes is required to order. Pay-at-venue orders enter `Submitted`. Online ordering
is rejected until a real provider verification/callback API is implemented.

## Branch-owner flow

Owner endpoints require `X-Branch-Management-Key`. Configure the SHA-256 hash of a different long random secret per
branch through configuration or environment variables; do not commit plaintext keys:

```text
OwnerAccess__BranchKeyHashes__<branch-guid>=<lowercase-sha256-hex>
```

- `GET /api/v1/owner/branches/{branchId}/orders?status=Submitted`
- `POST /api/v1/owner/branches/{branchId}/orders/{orderId}/actions`
  - Body: `action`, optional `reason`, and `expectedVersion`.
  - Actions: `accept`, `reject`, `start-preparing`, `ready`, `complete`.
- `GET /api/v1/owner/branches/{branchId}/visits`
  - Shows customers active during the last 15 minutes and whether they have an open order; no coordinates or phone.
- `GET /api/v1/owner/branches/{branchId}/customers`
- `PUT /api/v1/owner/branches/{branchId}/customers/{customerId}`
- `DELETE /api/v1/owner/branches/{branchId}/customers/{customerId}`
  - Delete archives the branch relationship and preserves the customer and order history.
- `PUT /api/v1/owner/branches/{branchId}/profile`
- `POST /api/v1/owner/branches/{branchId}/menus`
- `POST /api/v1/owner/branches/{branchId}/menus/{menuId}/items`
- `PUT /api/v1/owner/branches/{branchId}/menus/{menuId}/items/{itemId}`
- `POST /api/v1/owner/branches/{branchId}/events`
- `PUT /api/v1/owner/branches/{branchId}/events/{eventId}`

The branch management key is a deployable first authorization boundary. Replace it with authenticated staff
memberships, role permissions, and audit records before exposing a multi-tenant owner application broadly. Never
embed the branch key in the customer PWA/TWA bundle.

## PWA and TWA notes

- Prefer same-origin Next.js and API deployment. It avoids broad credentialed CORS and simplifies secure cookies later.
- Production hosting redirects to HTTPS and enables HSTS. Configure only trusted reverse proxies to forward the
  original scheme and client IP before relying on IP rate limits.
- Never service-worker-cache session, order, contact, owner, or discovery responses; these endpoints return
  `Cache-Control: no-store`. Storefront data is briefly cacheable.
- A Trusted Web Activity wraps the same HTTPS PWA and API contracts. It adds Android Digital Asset Links but does not
  require a different backend workflow.
- Location is a match signal, not cryptographic proof of presence. QR/BLE/Wi-Fi or a short-lived signed discovery proof
  can be layered on later if spoofing becomes material.
