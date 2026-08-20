# Dummy Member Payloads (with real R2 photo URLs)

10 `POST /api/Members` payloads, each pointing at a real image uploaded to the
`yellowtail-photos` R2 bucket (solid-color 200x200 PNG placeholders, one per member).

Endpoint: `POST http://localhost:5191/api/Members`
Header: `Content-Type: application/json`

Sport IDs used (seeded):
| Sport | Id |
|---|---|
| Tennis | `00000000-0000-0000-0000-000000000001` |
| Football | `00000000-0000-0000-0000-000000000002` |
| Swimming | `00000000-0000-0000-0000-000000000003` |
| Basketball | `00000000-0000-0000-0000-000000000004` |
| Padel | `00000000-0000-0000-0000-000000000005` |

`role` is a numeric enum (no `JsonStringEnumConverter` configured): `0` = Member, `1` = Coach, `2` = Admin.

> These 10 members have already been created against local dev (`http://localhost:5191`) using these exact payloads. Re-posting them as-is will fail with `400` (duplicate email isn't blocked by the API, but you'll just get 10 more rows) — treat these as a record of what was created, or tweak the emails to create more.

| # | Name | Member Id |
|---|---|---|
| 1 | Alice Carter | `96f35e3d-6f9a-451d-92bf-d016ff91ef52` |
| 2 | Ben Nguyen | `2c955f01-e7fc-42b0-8355-dff8c3bd19c0` |
| 3 | Chloe Martinez | `e67e4464-effc-4217-a04b-9dae164b7ba1` |
| 4 | David O'Brien | `29a33f11-381a-46b0-b4d4-57ff707b07f3` |
| 5 | Emma Schmidt | `dccb752a-1679-4bd7-aa54-8a1dc81aa768` |
| 6 | Felix Andersson | `bcf5ff4c-d1bd-4cc6-a1f4-ca4e77479c72` |
| 7 | Grace Kim | `d24613f4-2a7b-4698-a714-ba4ad0a9c024` |
| 8 | Hassan Ali | `67dd85bc-ae25-4041-ab3f-86aa993dd46a` |
| 9 | Isla Fraser | `32907525-7dca-4607-9f0d-b4a20b686846` |
| 10 | Jamal Robinson | `b75372e7-4f60-468c-a54d-49744e4c3dc6` |

---

### 1
```json
{
  "firstName": "Alice",
  "lastName": "Carter",
  "email": "alice.carter@example.com",
  "phone": "+1-555-0101",
  "dateOfBirth": "1992-03-14",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/b8a9c4bf-13b6-442c-8cd2-478c6a39cfa5.png",
  "role": 0,
  "sportIds": ["00000000-0000-0000-0000-000000000001"]
}
```

### 2
```json
{
  "firstName": "Ben",
  "lastName": "Nguyen",
  "email": "ben.nguyen@example.com",
  "phone": "+1-555-0102",
  "dateOfBirth": "1988-07-22",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/67058b06-0a51-4491-b44e-194beeb68236.png",
  "role": 1,
  "sportIds": ["00000000-0000-0000-0000-000000000002"]
}
```

### 3
```json
{
  "firstName": "Chloe",
  "lastName": "Martinez",
  "email": "chloe.martinez@example.com",
  "phone": "+1-555-0103",
  "dateOfBirth": "2000-11-02",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/cd678da9-0ba8-4502-8d25-fbc5efdd3e5b.png",
  "role": 0,
  "sportIds": ["00000000-0000-0000-0000-000000000003", "00000000-0000-0000-0000-000000000005"]
}
```

### 4
```json
{
  "firstName": "David",
  "lastName": "O'Brien",
  "email": "david.obrien@example.com",
  "phone": "+1-555-0104",
  "dateOfBirth": "1995-01-30",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/9943c7be-519a-49e2-80a0-00e314c2f618.png",
  "role": 0,
  "sportIds": ["00000000-0000-0000-0000-000000000004"]
}
```

### 5
```json
{
  "firstName": "Emma",
  "lastName": "Schmidt",
  "email": "emma.schmidt@example.com",
  "phone": "+1-555-0105",
  "dateOfBirth": "1990-09-18",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/84e4b324-0ab5-44a0-9f28-4e6e572cb45b.png",
  "role": 2,
  "sportIds": ["00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002"]
}
```

### 6
```json
{
  "firstName": "Felix",
  "lastName": "Andersson",
  "email": "felix.andersson@example.com",
  "phone": "+1-555-0106",
  "dateOfBirth": "1998-05-09",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/4dc011b4-2ea4-4255-bc5a-6a4eb483b3fd.png",
  "role": 0,
  "sportIds": ["00000000-0000-0000-0000-000000000005"]
}
```

### 7
```json
{
  "firstName": "Grace",
  "lastName": "Kim",
  "email": "grace.kim@example.com",
  "phone": "+1-555-0107",
  "dateOfBirth": "1985-12-25",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/19dde21f-2a16-4663-b67e-0f59fc749298.png",
  "role": 1,
  "sportIds": ["00000000-0000-0000-0000-000000000003"]
}
```

### 8
```json
{
  "firstName": "Hassan",
  "lastName": "Ali",
  "email": "hassan.ali@example.com",
  "phone": "+1-555-0108",
  "dateOfBirth": "1993-04-11",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/41ac67a7-38f5-40c9-af16-03e2da33db7b.png",
  "role": 0,
  "sportIds": ["00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000004"]
}
```

### 9
```json
{
  "firstName": "Isla",
  "lastName": "Fraser",
  "email": "isla.fraser@example.com",
  "phone": "+1-555-0109",
  "dateOfBirth": "2001-08-07",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/09b483de-2508-4209-a1f6-0e317328a1c4.png",
  "role": 0,
  "sportIds": []
}
```

### 10
```json
{
  "firstName": "Jamal",
  "lastName": "Robinson",
  "email": "jamal.robinson@example.com",
  "phone": "+1-555-0110",
  "dateOfBirth": "1987-02-19",
  "photoUrl": "https://pub-d4c292cca02248e6b605da9a8b864b1e.r2.dev/members/cb28b81f-a8d6-41e2-bd40-3648342c5425.png",
  "role": 0,
  "sportIds": ["00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000003", "00000000-0000-0000-0000-000000000004"]
}
```

---

## Bulk-create with curl

```bash
for f in payload1.json payload2.json ...; do
  curl -sS -X POST http://localhost:5191/api/Members \
    -H "Content-Type: application/json" \
    -d @"$f"
done
```

Or save this file's blocks as `member1.json`..`member10.json` and loop over them.
