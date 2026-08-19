# AI Usage in Yellowtail POC

**Overview:** This document details how Claude was used to design the Yellowtail POC backend, including the actual prompts given, what Claude produced, and which decisions were made by the developer versus proposed by Claude. It reflects the real conversation history — nothing here is illustrative or hypothetical.

**Session date:** 2026-08-19
**Tool:** Claude Code (Sonnet 5)
**Approach:** Architecture-first prompting — requirements and architecture were clarified and decided *before* any implementation, using structured multiple-choice questions to keep every decision explicitly in the developer's hands.

**Status: Phase 1 (Architecture & Requirements Design) — in progress. No implementation has started against the final schema.**

---

## Session start: initial scaffold (superseded)

Before the project brief was provided, the developer asked to see the folder structure and then prompted:

**Prompt:**
```
Add actual controllers
```

**What Claude generated:** A `MembersController` with an in-memory repository, a `Member` entity, and a service layer — built against the default `dotnet new webapi` template, before any real requirements existed (no Postgres, no `Role`, no soft-delete, no Cloudinary).

**Status:** Explicitly flagged by Claude as provisional once the real project brief arrived, and confirmed by the developer as needing rework once the actual schema is finalized. This code still sits uncommitted in the working tree and should not be treated as current design.

---

## Phase 1: Architecture & Requirements Design

### Project brief & initial architecture questions

**Prompt (full project brief, as given):**
```
# Yellowtail POC - Sports Club Membership System
## Project Brief
- Client: Amsterdam Sports Inc (sports club)
- System: Membership management (members, sports, multi-tenant)
- MVP: Members list, single member view, create/edit/delete, sports list
- Backend only (API-driven)
- Multi-tenant (multiple branches)
- No auth required for POC
- Quality & extensibility critical (client is skeptical)

## Initial Constraints
- Framework: .NET 8 + ASP.NET Core (TBD: minimal APIs or controllers, TBD: CQRS needed?)
- Data: PostgreSQL
- Entities: Member (name, photo), Sport (name), M2M junction table
- Scale: ~100-500 members/branch initially
- Photos: Store URL only (front-end/CDN handles upload)

## Your Role in This Conversation
You are helping me design the backend architecture and implementation strategy BEFORE coding.
- Provide analysis, not implementation
- Ask clarifying questions when needed
- Suggest alternatives with trade-offs
- Do not generate code until I ask

## Context Preservation
This message establishes the project context. Reference it in follow-ups:
- "Given our multi-tenancy model..." (I'll decide this in Phase 1)
- "Based on our Clean Architecture choice..." (TBD)
- "For our chosen error handling strategy..." (TBD)
```

**What Claude did:** Flagged that the just-built controller scaffold predated this brief and would need rework, then asked four clarifying questions before proposing any design:
1. How should a request identify its branch (tenant), given no auth exists?
2. Is the Sports catalog global or per-branch?
3. What project structure fits, given the client's quality/extensibility concern?
4. Should the application layer use CQRS, or a conventional service layer?

**Developer's answers:**
1. *"can we skip mutitenecty for later MVP? And focus on other MVPs?"* — multi-tenancy deferred to a later phase.
2. Global catalog.
3. Pragmatic N-Tier, extended with a Domain layer.
4. Conventional service layer (no CQRS/MediatR).

**Kept / changed / discarded:** All four answers locked in as-is. For the deferred multi-tenancy item, Claude proposed a mitigation — keep all data access routed through repositories now, so tenancy can be added later as a filter, not a rewrite — which the developer accepted without change.

---

### 1.1 Scope Clarification & Requirements

**Prompt (as given):**
```
## Phase 1.1: Scope Clarification & Requirements

I need to clarify requirements before making architecture decisions.

Please ask me clarifying questions about:

1. Data Model Questions
   - Member photos: What's the size limit per photo? File types (JPEG, PNG only)?
   - Should members have additional attributes (email, phone, age, skill level)?
   - Can a member have no sports assigned (guest member)? Or must have at least one?

2. Multi-Tenancy Questions (Can skip since we are planning for later MVP)
   - Should sports be global or per-tenant?
   - Can branches see each other's member lists, or strict isolation?
   - Will a member ever belong to multiple branches?

3. Scalability Questions
   - Expected growth: 100-500 members/branch over 1 year? 1 month?
   - Number of branches: 5, 50, 500? (Can skip since later MVP)
   - Report needs: Any analytics? No

4. Extensibility Questions
   - Will members have roles (admin, coach, member)? Or all equal? (Yes better to consider)
   - Will sports have attributes (difficulty, max players)? (not yet but needs to cater for extending in the future)
   - Any future features hinted (training schedules, fees/payments)? (Not yet)

5. API Questions
   - Should pagination have limits (max 100 members per page)?
   - Any filtering needs beyond sport name?
   - Sorting preferences?

6. Security & Admin Questions
   - Can admins delete members (soft delete or hard delete)? (Soft delete)
   - Who creates branches/tenants (manual, self-service)? (Currently Manual, Later MVP)
   - Any audit trail needed? (Yes, but not in this MVP)

Please ask these questions. I'll provide answers, then we move to architecture decisions.
```

The developer pre-answered several sub-questions inline (no analytics, soft delete, audit trail deferred, branch creation manual). Claude asked the remaining open items across two rounds of four questions each (the question tool caps at four per round).

**Round 1 — data-model-shaping questions:**
| Question | Answer |
|---|---|
| Additional Member attributes beyond name + photo? | Email address, Phone number, Date of birth |
| Photo URL validation strategy? | Developer asked a clarifying counter-question instead: *"what is the best place to store the photo, yes I will only be storing the url. But where will the actual photo be stored?"* |
| Can a member have zero sports? | Zero sports allowed |
| Add a Role field now, or defer? | Add a simple Role field now (Member/Coach/Admin), unenforced |

Claude answered the photo-storage question (explained typical upload patterns: client uploads directly to object storage/CDN, backend only ever receives a URL) and proposed "URL format + must be HTTPS" as the validation rule, pending confirmation.

**Mid-flow interjection — a separate pre-made decision document:**
```
Yellowtail POC: Image & File Storage Decision - Cloudinary
...
DECISION: Use Cloudinary Free Tier for Image Storage
[full rationale: cost, multi-tenant folder isolation, CDN, image optimization,
rejected AWS S3 / local disk / Postgres BLOB / Azure Blob-for-POC]
```
Claude incorporated this rather than treating it as a new open question: confirmed HTTPS + well-formed URL validation works cleanly against Cloudinary today and Azure Blob at production (the document's own stated migration path), and deliberately recommended **not** hardcoding a check for Cloudinary's domain for that reason.

**Round 2 — API-behavior questions:**
| Question | Answer |
|---|---|
| Growth timeline to 100-500 members/branch? | Organic growth over the first year |
| Members-list pagination limits? | Default 20, max 100 |
| Filters beyond sport? | Active/inactive status, Name search (partial match), Joined-date range |
| Sorting? | Alphabetical by name (default) |

**Follow-up question** (raised by Claude after noticing the developer's own answers created an ambiguity — soft-delete and the "active/inactive" filter could be the same flag or two different concepts):
> *"Is 'inactive' the same thing as 'soft-deleted', or a separate membership status?"*
> **Answer:** Same flag — one `IsActive` bool used for both.

**Kept / changed / discarded:** All answers locked in as the MVP data/API shape. Nothing from this round was later reversed.

---

### 1.2 Technology Stack & Framework Analysis

**Prompt (as given):**
```
## Phase 1.2: Technology Stack & Framework Selection

Now I need to decide on the framework & architecture approach.

Given the answers above, analyze these options:

Option A: ASP.NET Core + Minimal APIs + Repository Pattern (no CQRS)
Option B: ASP.NET Core Controllers + Repository + Clean Architecture (no CQRS)
Option C: ASP.NET Core + Repository + CQRS/MediatR

For each option, provide:
1. Pros for this specific POC
2. Cons for this specific POC
3. Folder structure
4. What this signals to client
5. Extensibility story

Don't recommend yet. Just analyze the trade-offs.

we have decided to go with Ntire arcihitecture and has implemeted the code
```

**Important note on sequencing:** This request arrived *after* the developer had already chosen N-Tier + conventional service layer, in the very first architecture-questions round above — and after that scaffold was already running. So this analysis served as a documented, formal justification for a decision already made collaboratively, not as the mechanism that produced the decision. Worth stating plainly: no architectural decision in this project was made by accepting Claude's output uncritically — decisions came first, through direct questions to the developer; write-ups came after, on request.

**What Claude generated:** A full trade-off analysis of all three options — pros/cons specific to this POC, folder structure per option, client-perception signal, and extensibility story — without a recommendation, as instructed. Condensed:

| | A: Minimal APIs | B: Controllers (built) | C: CQRS/MediatR |
|---|---|---|---|
| Files per operation | Fewest | Moderate | Most |
| Matches what's already running | No | **Yes** | No |
| Validation/ProblemDetails "for free" | No | Yes (`[ApiController]`) | Via pipeline behavior |
| Risk of reading as over-engineered to a pragmatic reviewer | Low | Low | Real |
| Risk of reading as under-structured to an enterprise reviewer | Some | Low | None |

**Kept / changed / discarded:** Option B reconfirmed as the standing choice (already implemented). Option A and C recorded as considered-and-rejected, with reasons on file for a future client question about why they weren't used.

---

### 1.3 Data Model & Multi-Tenancy Strategy

**Prompt (as given):**
```
## Phase 1.3: Data Model & Multi-Tenancy Strategy

Based on our requirements, design two data models:

### Requirement Recap
- Members belong to one branch (tenant)
- Sports are global (shared across branches)
- Members can play multiple sports
- Each branch has complete data isolation
- Multi-tenancy approach: [DATA-LEVEL FILTERING / SCHEMA-LEVEL / YOUR PREFERENCE]

### Model A: Data-Level Filtering (Single DB, Tenant-Aware Queries)
[entity definitions, tenant filtering, leakage safeguards, photo storage]

### Model B: Schema-Level Isolation (One Schema Per Tenant)
[entity definitions, tenant routing, migration strategy, photo storage]

For each model: relationship cardinality, failure modes, SQL pseudocode or EF Core fluent API.
Don't generate entity code yet. Just schema design.
```

**What Claude did before answering:** Flagged that this prompt's requirement recap ("complete data isolation," a concrete tenancy approach to choose) appeared to reopen the multi-tenancy deferral decided earlier, rather than silently designing it as if that decision had changed. Asked the developer directly which was intended: build tenancy now, or document it as the Phase 2 plan while the MVP ships single-tenant.

**What Claude generated (after that flag):** A full comparison of Model A (data-level filtering via EF Core global query filters, with concrete leakage safeguards — never trust `BranchId` from a request body, a marker-interface convention so new entities can't silently skip scoping) versus Model B (schema-per-tenant, with its migration-replay cost and a specific called-out failure mode: Npgsql connection pooling can serve one tenant's schema on a connection still carrying another tenant's `search_path`, an intermittent bug class much harder to catch than Model A's static, review-catchable failure modes). Since the prompt invited "YOUR PREFERENCE," Claude gave a light-touch recommendation for Model A, with reasoning (Model B still needs a shared-schema exception for the global `Sport` table anyway, so it doesn't even deliver the "complete isolation" the requirement recap asked for).

**Developer's answer:** *"yes.. please go with the planned MVP"* — confirming the original deferral stands. Model A adopted as the **documented Phase 2 target design only**; the MVP itself remains single-tenant with no `BranchId` column.

**Kept / changed / discarded:** Model A's mechanism (query filter + marker-interface convention) kept as the recorded future design. Model B discarded outright — not just for this MVP, but as a live candidate at all, given the connection-pooling risk. Actual tenancy implementation itself deferred, per the original decision.

---

## Where Claude Pushed Back Rather Than Complying Silently

1. **Provisional scaffold** — flagged the very first `MembersController` (built before the project brief existed) as due for rework rather than presenting it as finished work once real requirements arrived.
2. **Scope-reopening in 1.3** — noticed the Phase 1.3 prompt's requirement recap conflicted with the earlier multi-tenancy deferral and asked which was intended, instead of assuming the deferral had been reversed.
3. **Photo URL validation** — recommended against a Cloudinary-specific validation rule (e.g. checking the `res.cloudinary.com` domain) specifically because the developer's own Cloudinary decision document named Azure Blob as the intended production migration target — a narrower, provider-agnostic rule serves both.

## Decision Ownership

Every architectural and data-model decision in this document was made by the developer, via direct multiple-choice questions or explicit written confirmation. Claude's role was consistently: propose options with named trade-offs, flag inconsistencies or risks when they appeared, and never proceed past an open decision without an explicit answer. No code has been accepted or rejected yet, because none has been generated against the final schema — that review will populate a "Where Claude got it wrong" section once Phase 2 begins.

## Not Yet Reached

Phase 2 (entity classes & `DbContext`), Phase 3 (API endpoints against the real schema), Phase 4 (error-handling middleware), Phase 5 (tests) have not started. The only controller/repository code in the repository is the pre-requirements scaffold noted at the top of this document, which is expected to be replaced, not extended, once implementation begins.

## Tools & Model

- **Claude Code (Sonnet 5)** — single continuous session, full conversation context maintained across every decision recorded above.

---

## Summary

| Phase | Claude's Contribution | Developer's Contribution | Outcome |
|---|---|---|---|
| Initial scaffold | Generated a provisional controller before requirements existed | Requested it, then supplied the real brief that superseded it | Flagged for rework, not deleted |
| Architecture kickoff | Asked 4 targeted questions instead of assuming defaults | Decided: defer tenancy, global sports, N-Tier, service layer | Architecture direction set |
| 1.1 Scope Clarification | 8 targeted questions across 2 rounds + 1 follow-up on a self-identified ambiguity | Answered all; supplied an independent Cloudinary decision doc mid-flow | Full data/API shape defined |
| 1.2 Stack Analysis | Neutral A/B/C trade-off write-up, no recommendation (as instructed) | Confirmed already-implemented Option B, documented for the record | Formal justification on file |
| 1.3 Multi-Tenancy Strategy | Flagged a scope inconsistency before answering; delivered Model A/B comparison with failure modes | Confirmed original deferral stands; adopted Model A as Phase 2 target only | Tenancy design documented, not built |

**Status:** Architecture and requirements are fully decided for the MVP. No implementation has begun against this design.
