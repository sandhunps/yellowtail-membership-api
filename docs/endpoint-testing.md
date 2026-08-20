# Endpoint Test Payloads

Base URL (local dev, `http` profile): `http://localhost:5191`
Base URL (local dev, `https` profile): `https://localhost:7035`

All error responses are RFC 7807 `ProblemDetails`:
- `404 Not Found` — member/resource doesn't exist
- `400 Bad Request` — validation failure (FluentValidation or service-level, e.g. unknown `SportId`)

Seeded sport IDs (from `YellowtailDbContext`, always present):
| Sport | Id |
|---|---|
| Tennis | `00000000-0000-0000-0000-000000000001` |
| Football | `00000000-0000-0000-0000-000000000002` |
| Swimming | `00000000-0000-0000-0000-000000000003` |
| Basketball | `00000000-0000-0000-0000-000000000004` |
| Padel | `00000000-0000-0000-0000-000000000005` |

---

## Members — `api/Members`

### 1. GET `/api/Members` — list members

Query params (all optional): `SportId` (guid), `IsActive` (bool), `Name` (string, matches first or last name), `Page` (int, default 1), `PageSize` (int, default 20, max 100).

```
GET /api/Members
GET /api/Members?Page=1&PageSize=10
GET /api/Members?Name=john
GET /api/Members?SportId=00000000-0000-0000-0000-000000000001
GET /api/Members?IsActive=false
GET /api/Members?SportId=00000000-0000-0000-0000-000000000001&IsActive=true&Name=smith&Page=1&PageSize=5
```

Expected: `200 OK` with
```json
{
  "items": [ /* MemberResponse[] */ ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

Edge cases to try:
- `PageSize=1000` → should clamp to 100 (`MaxPageSize`)
- `Page=0` or negative → should clamp to 1
- `SportId=<random-guid-not-in-db>` → `200 OK`, empty `items` (not an error — listing doesn't validate the sport exists)

---

### 2. GET `/api/Members/{id}` — get one member

```
GET /api/Members/{id}
```

Expected: `200 OK` with a `MemberResponse`, or `404 Not Found` if `id` doesn't exist (works even for soft-deleted/inactive members — `GetById` ignores the active filter).

Try:
```
GET /api/Members/00000000-0000-0000-0000-000000000000
```
→ `404 Not Found`

---

### 3. POST `/api/Members` — create a member

**Valid payload (minimal — only required fields):**
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com"
}
```

**Valid payload (full):**
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "phone": "+1-555-0100",
  "dateOfBirth": "1995-06-15",
  "photoUrl": "https://cdn.example.com/photos/jane.jpg",
  "role": "Coach",
  "sportIds": [
    "00000000-0000-0000-0000-000000000001",
    "00000000-0000-0000-0000-000000000003"
  ]
}
```

Expected: `201 Created`, `Location` header pointing to `GET /api/Members/{newId}`, body is the created `MemberResponse`.

**Validation failure cases (expect `400 Bad Request`):**

Missing required field:
```json
{
  "firstName": "",
  "lastName": "Doe",
  "email": "jane.doe@example.com"
}
```

Invalid email:
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "not-an-email"
}
```

`FirstName`/`LastName` over 100 chars, or `Phone` over 30 chars.

Non-HTTPS photo URL (expect `400`):
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "photoUrl": "http://cdn.example.com/photos/jane.jpg"
}
```

Unknown sport id (expect `400` — service validates sport existence):
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "sportIds": ["11111111-1111-1111-1111-111111111111"]
}
```

---

### 4. PUT `/api/Members/{id}` — update a member

All fields required except `phone`, `dateOfBirth`, `photoUrl`, `sportIds` (unlike create, `role` and `isActive` are non-nullable here).

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "phone": "+1-555-0100",
  "dateOfBirth": "1995-06-15",
  "photoUrl": "https://cdn.example.com/photos/jane.jpg",
  "role": "Member",
  "isActive": true,
  "sportIds": ["00000000-0000-0000-0000-000000000002"]
}
```

Expected: `204 No Content`.

Edge cases:
- `PUT /api/Members/{random-guid}` with the payload above → `404 Not Found`
- `"isActive": false` → soft-deactivates; the member then disappears from default `GET /api/Members` list and needs `IsActive=false` to be seen, but `GET /api/Members/{id}` still finds it
- Same validation rules as create apply (empty names, bad email, non-HTTPS photo URL, unknown `sportIds` → `400`)

---

### 5. DELETE `/api/Members/{id}` — soft delete

```
DELETE /api/Members/{id}
```

Expected: `204 No Content`. Sets `IsActive = false` (does not remove the row).

Try:
```
DELETE /api/Members/00000000-0000-0000-0000-000000000000
```
→ `404 Not Found`

Verify: after delete, `GET /api/Members/{id}` still returns `200` (ignores filter), but `GET /api/Members` (default) no longer includes it, while `GET /api/Members?IsActive=false` does.

---

## Sports — `api/Sports`

### 6. GET `/api/Sports` — list sports catalog

```
GET /api/Sports
```

Expected: `200 OK` with the 5 seeded sports (no auth, no params, read-only).

---

## Photos — `api/Photos`

### 7. GET `/api/Photos/upload-signature` — get pre-signed upload URL

```
GET /api/Photos/upload-signature
GET /api/Photos/upload-signature?extension=jpg
GET /api/Photos/upload-signature?extension=jpeg
GET /api/Photos/upload-signature?extension=png
GET /api/Photos/upload-signature?extension=webp
```

Expected: `200 OK` with:
```json
{
  "uploadUrl": "https://...",
  "publicUrl": "https://..."
}
```

Invalid extension (expect `400 Bad Request`):
```
GET /api/Photos/upload-signature?extension=gif
GET /api/Photos/upload-signature?extension=exe
```

Flow: `PUT` the file bytes to `uploadUrl`, then use `publicUrl` as `photoUrl` in a subsequent Create/Update member request.

---

## Suggested test order

1. `GET /api/Sports` — confirm seed data present
2. `POST /api/Members` — create a member (minimal, then full payload)
3. `GET /api/Members/{id}` — confirm it was created correctly
4. `GET /api/Members` — confirm it shows up in the list, test filters/paging
5. `PUT /api/Members/{id}` — update it, including `isActive: false`
6. `GET /api/Members` vs `GET /api/Members?IsActive=false` — confirm soft-delete filtering behaves
7. `DELETE /api/Members/{id}` — soft delete, re-verify filtering
8. `GET /api/Photos/upload-signature` — sanity check independent of member data
9. Re-run steps 2–5 with each invalid/validation-failure payload above, confirming `400`/`404` and the `ProblemDetails` body shape
