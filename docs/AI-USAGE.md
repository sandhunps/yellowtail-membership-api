# AI Usage in Yellowtail POC

How I used Claude (Claude Code, Sonnet 5) to build this backend: the actual prompts, why I
phrased them that way, what got kept vs. discarded, where I kept AI out of the loop, and where it
got things wrong. Organized in the order that best explains the process, not the order I was
originally asked for it in.

I own every line in this repository. Claude wrote most of the code; every architectural decision,
business rule, and external-system action (Cloudflare account, credentials, data cleanup) was
mine, reviewed before it landed.

---

## 1. Tools and models

| Tool | Used for |
|---|---|
| Claude Code (Sonnet 5) | Code generation, architecture analysis, tests, debugging — one continuous session, no sub-agents |
| `dotnet` CLI | Build, test, EF Core migrations |
| Docker Compose | Local Postgres for development |
| `git` | Status checks before any destructive operation |
| AWS SDK for .NET | Pre-signed URLs against Cloudflare R2's S3-compatible API |
| Serilog, FluentValidation, xUnit/Moq/coverlet | Logging, validation, testing |
| Azure Data Studio, Cloudflare dashboard, my own terminal | Driven by me directly — Claude has no access to these |

---

## 2. The process, phase by phase

### 2.1 Architecture and requirements — before any real code

I opened with an explicit constraint:

```
You are helping me design the backend architecture and implementation strategy BEFORE coding.
Provide analysis, not implementation. Ask clarifying questions when needed. Suggest alternatives
with trade-offs. Do not generate code until I ask.
```

This forced decisions into words I could push back on instead of hiding inside code. Claude
responded with four clarifying questions (tenant routing, sports catalog scope, project
structure, CQRS vs. plain services) rather than a design.

A structured requirements pass followed, deliberately over-specified with some answers
pre-filled — enough to save round-trips, but I left out things like exact field formats on
purpose, to see what Claude would think to ask. It caught one gap in my own answers unprompted:
whether "inactive" and "soft-deleted" were meant to be the same flag.

I asked for a neutral N-Tier vs. minimal-API vs. CQRS trade-off analysis:

```
For each option, provide:
1. Pros
2. Cons
3. Folder structure
4. What this signals to client
5. Extensibility story
Don't recommend yet. Just analyze the trade-offs.

we have decided to go with N-tier architecture and have implemented the code
```

— mainly to get the reasoning on record independent of any model preference, since (per the
second line, added once I circled back to this thread later) I'd already made and implemented
the decision by that point.

For multi-tenancy, Claude flagged that my own requirement recap seemed to reopen a scope I'd
already deferred, and asked before assuming I'd changed my mind. I confirmed the deferral; the
data-level-filtering design became a documented **Phase 2 target only** — nothing tenant-related
exists in the MVP schema. The schema-per-tenant alternative was rejected outright, on a concrete
failure mode Claude raised: Npgsql connection pooling can serve one tenant's schema on a
connection still carrying another tenant's `search_path`.

### 2.2 Building the real API

```
now can we start building as per our plan? where should we start?
```

— deliberately open-ended, to see whether Claude's proposed build order (schema → real DB →
services → API → smoke test → tests) matched sound judgment before I committed to it. It did.
Claude found no local Postgres
reachable and asked how I wanted to run one rather than guessing; I chose Docker Compose. It then
built the entities, `DbContext`, migration, repositories, services, and controllers, and actually
exercised every endpoint with `curl` against the real database before calling it done.

### 2.3 Iterating on the API surface

```
is this the best way to handle filters? what if in the future the amount of conditions
increases? Can I have a generic way to do it? In my company project I used query options as a
string which then resolved
```

— I cited my own prior pattern deliberately, to see if Claude would defer to it as precedent or
actually reason about the fit. It pushed back (loses compile-time type safety and
Swagger fidelity) and proposed `MemberListFilterRequest` instead, which is what got built.

Same pattern with validation: I pasted my own controller code with manually-injected
`IValidator<T>` and asked if it would clutter as more actions were added. Claude confirmed the
concern and proposed a global `ValidationFilter` instead.

### 2.4 Cleanup

```
Can we clean up the program.cs class and move service registry to an extension method, as well
call db context, and what others you suggest?
```

— left open on purpose; Claude noticed the middleware pipeline had the same problem as
`Program.cs` and split both, without me having to name it.

### 2.5 Testing

```
Now let's add unit tests. Try to achieve most code coverage
```

— Claude scoped this deliberately: full coverage on `Services` and `API`, but flagged the `Data`
layer (EF Core repositories, Postgres
specifics) as needing Testcontainers-based integration tests instead of mocks, and left that as
an explicit gap rather than writing hollow tests against it. I agreed. Ended at 66 tests, 98.5%
line / 100% branch coverage on the in-scope layers.

### 2.6 Image storage: Cloudinary, then a pivot to Cloudflare R2

```
Okay now let's add image storage endpoints using Cloudinary. What all things we should do,
let's first ideate
```

— asked before any code. I wanted the design space (signed-URL vs. backend-proxy) laid out
before picking, not the simpler option chosen for me silently. Cloudinary was fully built (signed
uploads, User Secrets, test script), then dropped entirely for a business reason (switching
providers), not a technical one. I anchored the R2 request to the already-agreed architecture:

```
okay for object storage we are planning to use Cloudflare R2. The flow is the same as earlier.
Client will call our endpoint to get the pre-signed url, which is then used by the client to
store the image and get the url. This is my account, tell me what to do
```

— rather than restarting the design. This time I walked through real Cloudflare account setup
myself and we verified a real end-to-end
upload — image uploaded through the real presigned URL, confirmed public, used as a real member's
`photoUrl`. Cloudinary never got that same live verification, since I pivoted before providing
real credentials for it.

### 2.7 Later feature and structure changes

Grouping API contracts by controller — asked as a genuine question first:

```
Is it a nice touch to group contracts in API to different folders based on controller
```

Claude flagged that `SportResponse` is shared across two controllers and shouldn't move; I
confirmed with:

```
Yes, keep shared untouched, group the rest
```

Removing an unused filter pair:

```
Remove joined from and joined to filters
```

— a clean five-layer removal with the affected tests updated in the same pass.

Duplicate email/phone validation, alongside a question on whether to adopt a custom
base-exception class from my own production code:

```
I would love to create a validation for creating a new member for email and phone number, also
what is your suggestion on creating a custom exception class [pasted R2QException example]
```

Claude recommended against the custom exception class — the existing `ValidationFailedException`
already covered the need — the same pattern as §2.3: shown a real precedent from my own
experience, it reasoned about fit rather than deferring to it. For the validation rule itself, I
picked duplicate-rejection via multiple-choice; Claude caught on its own that checking only on
create (not update) would leave a loophole, and fixed both. Before generating the migration it
checked live dev data, found existing duplicates — some mine, not just its own test data — and
asked rather than silently resolving them. I chose to clean it up myself.

`#region LLD` comments on every service method, in an exact format I specified:

```
can you add lld for all service methods??
```

clarified as:

```
a region just before writing any code in a method
#Region LLD
// step 1:
// step 2:
#EndRegion
```

— notably cutting against Claude's own default of minimal comments, followed exactly as given
anyway.

---

## 3. Where I did not use AI, and why

- **Every architectural and business decision** — multi-tenancy deferral, sports catalog scope,
  the `IsActive` flag design, the Cloudinary → R2 pivot, the duplicate email/phone rule, rejecting
  both the envelope-response and custom-exception patterns from my own production code. Claude
  proposed options; I picked.
- **All external account setup** — Cloudflare bucket, public URL, API token, all created in my
  own browser session.
- **Handling real credentials** — every `dotnet user-secrets set` with a real value was run by me,
  in my own terminal, on purpose.
- **Resolving duplicate test data** before the unique-index migration, and **rotating the leaked
  API token** — both mine, since both touched judgment calls or credentials that weren't Claude's
  to make.

## 4. Where Claude got it wrong

The clearest concrete mistake: when I asked for logging, Claude added `LogWarning` calls right
before several `throw new NotFoundException(...)` / `ValidationFailedException(...)` statements in
`MemberService`. I asked directly:

```
do I need both?
```

— the honest answer was no. ASP.NET Core's
`ExceptionHandlerMiddleware` already logs every caught exception at `Error` level unconditionally,
before the custom `GlobalExceptionHandler` even runs, so those code paths were being logged twice,
with near-identical text and the wrong relative severity (a 404 isn't really an `Error`). I asked
to strip the redundant lines back out rather than suppress the framework's own logging. The root
cause was a real gap in that turn's reasoning — not checking what was already logging the same
event before adding a new log call.
