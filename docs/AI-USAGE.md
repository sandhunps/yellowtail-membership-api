# AI Usage in Yellowtail POC

This document is my honest account of how I used Claude (Claude Code, Sonnet 5) to build this
backend — the actual prompts I gave at each step, why I phrased them the way I did, what the
model produced before any code existed, where I deliberately kept it out of the loop, and where
it got things wrong. It's written in the order I think best explains the process, not the order
I originally asked for it in. The full raw prompt sequence is in
[PROMPT-LOG.md](PROMPT-LOG.md) as an appendix; this document is the reasoning behind it.

I own every line in this repository. Claude wrote most of the code, but every architectural
decision, every business rule, and every external-system action (Cloudflare account, credentials,
data cleanup) was mine, and I reviewed everything it produced before it landed.

---

## 1. Tools and models

| Tool | Used for | Used by |
|---|---|---|
| Claude Code (Sonnet 5) | All code generation, architecture analysis, test writing, debugging | Me, driving Claude Code interactively in one continuous session |
| `dotnet` CLI (build/test/`ef migrations`) | Building, testing, generating and applying EF Core migrations | Claude, via its shell tool |
| Docker / Docker Compose | Local Postgres 16 for development | Claude set it up; I ran Docker Desktop itself |
| `git` | Status checks before any destructive operation, diff review | Claude (read-only checks); I did the actual `git commit`s myself outside this session |
| AWS SDK for .NET (`AWSSDK.S3`) | Pre-signed URL generation against Cloudflare R2's S3-compatible API | Claude |
| Serilog | Structured logging (console + rolling file sinks) | Claude |
| FluentValidation | Request validation, wired through a custom global action filter | Claude |
| xUnit, Moq, coverlet | Unit tests and coverage collection | Claude |
| Azure Data Studio | Browsing the Postgres database visually | Me — Claude has no access to my desktop apps, only screenshots I shared |
| Cloudflare dashboard | Creating the R2 bucket, enabling the public dev URL, creating the API token | Me — same reason, plus this is a credentialed account Claude shouldn't be acting on |
| My own terminal | Running `dotnet user-secrets set` for real credentials | Me, deliberately — see §5 |

One model, one continuous session, no sub-agents or separate chat threads — everything below
happened in a single conversation with full history available throughout.

---

## 2. How I kept context across the session

This wasn't a short exercise — it ran across a full backend build, a provider pivot, and several
rounds of refactoring. A few concrete things kept it coherent instead of drifting:

**The codebase itself was the source of truth, not memory.** Before editing almost any file that
had been touched more than a turn or two earlier, Claude re-read it rather than assuming it
remembered the exact current contents. The tooling actively supported this: any time a file
changed on disk since Claude last read it — including changes I made directly, outside the chat —
the next tool call surfaced a note flagging exactly what had changed and instructing Claude to
treat it as deliberate rather than silently overwrite it. That happened routinely once I started
poking at files myself.

**Two documents (this one and the prompt log) exist specifically to make decisions checkable
later**, including by me in this interview. They're not just a deliverable — I used them mid-session
too, e.g. asking Claude to "list down all our context and plans" partway through Phase 1 as a
sanity check before moving on.

**A concrete example of context being maintained correctly:** when I later asked for Cloudflare
R2 image storage, I opened with *"the flow is the same as earlier"* rather than re-explaining the
requirement from scratch. Claude correctly recognized this meant the same signed-URL,
backend-never-touches-bytes architecture already agreed for Cloudinary, and only asked about the
R2-specific delta (bucket, account ID, public URL, credentials) instead of re-litigating the whole
design.

**A concrete example of context being tested deliberately, and drift being caught:** when I asked
to revert the Cloudinary work, I said *"revert all uncommitted changes"* — a broader instruction
than I actually meant. Claude ran `git status` before touching anything (rather than assuming
scope) and found that most of the session's work was already committed; the only uncommitted
changes were the Cloudinary feature and one earlier, unrelated `.http` file deletion. I then
clarified *"only skip the cloudinary featue"* — Claude had already reasoned through this correctly
(reverting the Cloudinary code while re-applying the separate `.http` deletion, since that was an
already-settled decision, not part of "this"), so my clarification confirmed rather than corrected
its plan. I include this because it's exactly the kind of moment where a less careful process
would have run a blind `git reset --hard` and either wiped uncommitted work or resurrected a file
I'd already asked to have removed.

**A moment I want to be transparent about, not just Claude's:** while wiring up Cloudflare R2, I
ran the `dotnet user-secrets set` commands myself as instructed — but then pasted the terminal
output, real Access Key ID and Secret Access Key included, into the chat. Claude flagged this
immediately, explained that the value was now in a transcript outside the intended secure
boundary, and told me to treat the token as compromised and rotate it — which I did. Nothing
technical went wrong here (the secret was never used anywhere insecure by Claude), but it's a
real example of a mistake happening mid-session and being caught and corrected in the same turn.

---

## 3. The process, phase by phase

### 3.0 Before the brief existed

The very first thing I asked, before giving any real requirements, was to see the folder
structure, then:

> "Add actual controllers"

Claude built a `MembersController` with an in-memory repository against the default
`dotnet new webapi` scaffold. I did this on purpose — a quick, low-stakes way to see how Claude
approaches a totally unscoped request before I'd committed to any real design, not because I
wanted this code kept. Once I gave the actual project brief, Claude flagged on its own that this
scaffold predated real requirements and would need rebuilding — it wasn't asked to notice that,
it noticed because the brief obviously conflicted with what was already there (no Postgres, no
`Role` field, no soft delete). That code was later deleted entirely and rebuilt from the ground up.

### 3.1 Architecture and requirements — before any real code

This is the phase I was most deliberate about, and it's documented in the most depth because it's
where the biggest, hardest-to-reverse decisions got made. I gave Claude an explicit role
constraint up front:

> "You are helping me design the backend architecture and implementation strategy BEFORE coding.
> Provide analysis, not implementation. Ask clarifying questions when needed. Suggest
> alternatives with trade-offs. Do not generate code until I ask."

**Why I prompted it this way:** I wanted the model to surface the questions and trade-offs I
hadn't thought of yet, without it jumping straight to an implementation I'd then have to unpick.
Explicitly forbidding code was the mechanism for that — it forces the model to actually commit to
reasoning in words I can push back on, instead of hiding assumptions inside code I'd have to
reverse-engineer to disagree with.

Claude responded not with a design, but with four clarifying questions (tenant routing, whether
the sports catalog was global or per-branch, project structure, CQRS vs. a plain service layer) —
exactly matching what I'd asked for. I answered directly, including deferring multi-tenancy
entirely for a later phase.

I then ran a structured requirements pass, deliberately over-specifying the categories I wanted
covered and pre-answering some of them inline:

> "## Phase 1.1: Scope Clarification & Requirements — [data model / multi-tenancy / scalability /
> extensibility / API / security questions, several pre-answered]"

**Why:** I wanted comprehensive coverage without wasting a round-trip on questions I already knew
the answer to, and pre-answering forces me to actually think through my own requirements while
writing the prompt, not just react to the model's questions. What I deliberately left out: I
never gave file size limits, exact phone/email format rules, or a fixed number of branches —
those were things I wanted the *model* to ask about, to see whether it would notice gaps I hadn't
flagged as categories.

Two rounds of targeted questions followed (member fields, photo validation, guest members, roles;
then growth timeline, pagination, filtering, sorting), plus one Claude raised on its own after
noticing my own answers created an ambiguity — whether "inactive" and "soft-deleted" were the same
flag. I hadn't asked that question; it noticed the gap between two of my own answers and asked
before assuming.

Midway through, I dropped in a separate, already-written decision document for photo storage
(Cloudinary, with full rationale for rejecting S3/local disk/Postgres BLOB/Azure-for-now). Claude
folded it into the validation rule already being designed rather than treating it as a new
open question — and specifically recommended *against* hardcoding a Cloudinary-domain check in
the URL validator, because my own document named Azure Blob as the intended production migration
target. That's a case of it connecting two pieces of information I'd given it at different times
without me having to point out the connection myself.

Next, I asked for a neutral trade-off analysis with an explicit constraint:

> "For each option, provide: 1. Pros... 2. Cons... 3. Folder structure... 4. What this signals to
> client... 5. Extensibility story... Don't recommend yet. Just analyze the trade-offs.
>
> we have decided to go with Ntire arcihitecture and has implemeted the code"

**Why "don't recommend yet":** I wanted the comparison on the record independent of whatever
Claude's own preference might be, specifically so I could point to it later as an artifact of
*my* reasoning, not the model's opinion — and notably, by the time I asked this, I'd already made
and implemented the decision in the earlier round. This prompt wasn't the decision mechanism; it
was me asking for the formal justification after the fact, which Claude's own summary explicitly
called out rather than pretending the analysis had driven the choice.

Finally, I asked for a two-model data design comparison (data-level tenant filtering vs.
schema-per-tenant), again inviting a preference this time. Before answering, Claude flagged that
my requirement recap ("complete data isolation," a tenancy approach to choose) seemed to reopen
the multi-tenancy deferral I'd already settled — and asked which I meant, rather than silently
designing full tenancy as if I'd changed my mind. I confirmed the deferral stood; the two-model
comparison became a **documented Phase 2 target**, not something built into the MVP.

**What was kept, changed, and discarded from this phase:** N-Tier + conventional service layer +
Controllers, kept and built. CQRS/MediatR and minimal APIs, analyzed and explicitly rejected.
Data-level tenant filtering (Model A), kept as the *recorded future design only* — no `BranchId`
exists anywhere in the MVP schema. Schema-per-tenant (Model B), discarded outright, not just
deferred, because of a specific failure mode Claude called out: Npgsql connection pooling can
serve one tenant's schema on a connection still carrying another tenant's `search_path` — an
intermittent, hard-to-catch bug class, worse than anything Model A risks.

### 3.2 Building the real API

Once the design was settled:

> "now can we start buildig as per our plan? where should we start?"

**Why so open-ended:** at this point I genuinely wanted to see whether Claude's proposed build
order matched sound engineering judgment — data layer first, then a real database, then services,
then the API surface, then a manual smoke test, then automated tests — before committing to it. If
it had proposed starting somewhere I disagreed with (say, the API layer before the schema
existed), that would have told me something about how much I needed to steer the rest of the
build. It didn't; the order matched what I'd have chosen myself.

Claude checked for a local Postgres, found none reachable, and asked me directly how I wanted to
run one rather than guessing (Docker Compose, Homebrew, or an existing instance) — I chose Docker
Compose. From there it built the `Member`/`Sport`/`MemberSport` entities, `YellowtailDbContext`
with a soft-delete query filter, the EF Core migration, the repository and service layers, and the
controllers — then actually ran the API against the real database and exercised every endpoint
with `curl` before calling any of it done, rather than just asserting the build succeeded. Port
5432 turned out to be occupied first by Postgres.app, then (after I moved to 5433 and later asked
to move back) by a native system PostgreSQL 17 `launchd` service — Claude found this by actually
inspecting running processes, not guessing, and gave me the exact `sudo launchctl` command to run
myself rather than running a privileged command on my machine on its own.

### 3.3 Iterating on the API surface

A few rounds here were me deliberately pushing back on my own earlier design once I could see it
running as real code:

> "is thi sthe best way to handle filterrs? what if in the future the amount od codtions increse?
> Can I have a geneic way to do it? I my company project I used query options a string wgich
> thenresloved"

**Why I framed it this way, mentioning my own company's pattern:** I wanted to see whether Claude
would just agree with a pattern I'd used before because I'd cited it as precedent, or actually
reason about whether it fit *this* codebase. It didn't just agree — it explained the trade-off
(a generic string-filter loses compile-time type safety and Swagger documentation fidelity) and
proposed a narrower fix that solved my actual complaint (the controller signature growing) without
adopting the heavier pattern. I asked it to explain the proposal in more depth before approving it.

Similarly, I pasted my own controller code with manually-injected `IValidator<T>` instances and
asked directly whether it was correct and whether it would clutter the controller as more actions
were added. Claude confirmed the concern was valid and proposed a global `IAsyncActionFilter`
instead — a single cross-cutting mechanism, consistent with how I'd already decided to keep
concerns centralized rather than scattered. I approved and it was built and verified against real
requests (valid/invalid payloads, actions with no registered validator).

### 3.4 Cleaning up as the codebase grew

> "Can we clean up the program.cs class and move service regustery to an extesion methoda as well
> call db context and what others you suggest?"

**Why I left "what others you suggest" open:** by this point `Program.cs` had several unrelated
concerns tangled together, and I wanted to see if Claude would notice the *other* half of the
mess (the middleware pipeline) without me having to spell it out. It did, and proposed splitting
both the service registration and the middleware pipeline into extension methods matching the
project's own three-layer structure — which is what got built, verified with a full build, test
run, and live request smoke test afterward.

I also asked a direct factual question rather than accepting an earlier explanation at face
value:

> "Is this required builder.Services.AddProblemDetails();"

**Why:** Claude had explained a few turns earlier why this line existed, and I wanted to know if
that explanation actually held up, not just re-hear it. Claude chose to test it empirically —
commented the line out, ran the app, and it crashed at startup — rather than restate its earlier
reasoning. That earlier reasoning turned out to be *incomplete*: I cover the actual gap in §5.

### 3.5 Testing

> "Now lets add unit tests. try to achive most code coverage"

Claude scoped this deliberately rather than chasing 100% blindly: full coverage on `Services` and
`API` (business logic, controllers, validators, filters — the things worth testing with mocks),
and explicitly flagged that the `Data` layer (EF Core repositories, `DbContext` query filters,
Postgres-specific `ILike` usage) can't be meaningfully unit tested without a real database, and
would need Testcontainers-based integration tests as a separate, later decision rather than
something to fold in silently. I agreed with leaving that as a flagged gap rather than writing
hollow mocked tests against it. Ended at 61 tests (later 66, after the duplicate-validation work),
98.5% line / 100% branch coverage on the in-scope layers.

### 3.6 Image storage: Cloudinary, then a full pivot to Cloudflare R2

> "Okay now lets add Image stogae endpoins uinsg Cloudinary. what all things we should do. let us
> first ideate"

**Why "let us first ideate" instead of just asking for the endpoint:** I already knew from Phase
1 that Cloudinary was the chosen provider, but *how* the backend should integrate with it was a
real open question with meaningfully different implementations (backend never touches the file
vs. backend proxies the upload). I wanted to see the design space laid out before picking one, not
have Claude quietly pick the simpler option for me. It correctly identified the central fork
(signed-URL vs. backend-proxy) and asked me to choose rather than assuming.

I chose the signed-URL approach; Claude built the full `PhotosController` + `PhotoUploadService`
flow, User Secrets for the API credentials, and a manual test script, and verified the endpoint's
wiring (it correctly rejected requests with no credentials configured, with a clean error rather
than a crash).

I later asked to switch providers to Cloudflare R2, and separately, to revert the Cloudinary work
entirely and drop it (see the git-revert example in §2). **Why the pivot happened at all:** this
wasn't Claude getting anything wrong — Cloudinary worked as designed. It was a real business
decision on my end to use R2 instead, and I anchored the new request to the already-agreed
architecture (see §2) rather than re-explaining it. Claude rebuilt the same shape against R2's
S3-compatible API (`AWSSDK.S3`, presigned PUT URLs), and this time I actually walked through
creating the real Cloudflare bucket, public URL, and API token myself (Claude gave me the exact
steps, since it has no access to my Cloudflare account), and we verified a real end-to-end upload
— a real image, uploaded through the real presigned URL, confirmed publicly readable, then used
as a real member's `photoUrl` through the actual API. Cloudinary was never verified with a live
upload in the same way, since I pivoted before providing real credentials for it — worth being
precise about, since it's a meaningful difference in how thoroughly each was actually proven out.

### 3.7 Architecture review, without asking for any code

Two prompts in this session asked for nothing to be built at all — just my own reasoning tested
against real production patterns I'd used elsewhere:

> "why is the YellowtailDbContext is a calss and why does it nit have an iterface?"

> "In My PRODUCTIONCODE [pasted a full envelope-response pattern: `ApiResponseDto<T>`,
> `ResponseCode` enum, always-200 responses] THIS IS HOW ALL API IS STRUCTURED... WILL IT BE GOOD
> TO ADOSPT YHIS?"

**Why I asked these as open questions rather than instructions:** both are cases where I had a
real alternative in hand (a pattern from my own production experience) and wanted to know if
Claude would just defer to precedent because I'd presented it as "how we do it," or actually push
back if it didn't fit. It pushed back on both — explained why the repository interfaces are
already the correct abstraction boundary (an interface on `DbContext` itself would be redundant
given nothing else touches it directly), and why the always-200 envelope pattern actively fights
standard HTTP tooling and would mean throwing away the `ProblemDetails`/status-code system already
built and tested. I didn't adopt either production pattern into this codebase.

The same thing happened once more, later, when I shared a custom base-exception class
(`R2QException`, with a `ResponseCode` enum and message-parameters dictionary) while asking for
validation on member email/phone. Claude again recommended against introducing it, on consistency
grounds with the earlier envelope-pattern decision, and pointed out the existing
`ValidationFailedException` already covered the actual need without a new type.

### 3.8 Later feature and structure changes

A handful of smaller, more surgical prompts rounded out the build:

- *"Is it a nice touch to group contrcts in API to differn folders baswdn on controller"* → I
  asked this as a genuine question first; once Claude flagged the one real wrinkle (`SportResponse`
  is shared across two controllers, not owned by one), I confirmed the split with *"Yes.. keep
  shared un touched group rest"* and it was executed exactly that way.
- *"Remove joined from a and joineef to filters"* → a straightforward removal, executed cleanly
  across all five layers the field touched (API contract, controller, service model, service, data
  query, repository), with the two affected tests updated in the same pass.
- Duplicate email/phone validation, requested alongside the exception-class question above — I
  used the multiple-choice tool to pin down exactly which validation I meant (format vs.
  uniqueness), chose duplicate-rejection for both fields, and Claude flagged on its own that
  enforcing it only on create (not update) would leave a real loophole, then implemented it on
  both. Before applying the resulting database migration, it checked the actual dev data first and
  found existing duplicates — including some from my own manual testing, not just its own — and
  asked how I wanted to handle them rather than silently picking rows to delete. I chose to clean
  it up myself.
- *"can you add lld for all servcie methods??"*, clarified to a specific `#region LLD` /
  `// step 1:` format I described exactly → built precisely to that format across every method in
  the three service classes, including converting one expression-bodied method to a block body
  so the region would actually fit inside it. This is a case where the requested style cuts
  against Claude's own default (minimal comments, no restating "what" code does) — I said so
  explicitly, and it followed the instruction exactly as given rather than pushing back or
  watering it down.

---

## 4. Where I did not use AI, and why

Almost all of the *code* in this repository was written by Claude — I want to be upfront about
that rather than overstate a hands-on-keyboard split that isn't real. What I kept for myself:

- **Every architectural and business decision.** Multi-tenancy deferral, global vs. per-branch
  sports, the single `IsActive` flag doing double duty, pagination defaults, the Cloudinary → R2
  pivot, the duplicate email/phone business rule, and rejecting both the envelope-response pattern
  and the generic-exception pattern from my own production code — all mine. Claude proposed
  options and trade-offs; I picked.
- **All external account setup.** Creating the Cloudflare account resources (R2 bucket, public
  development URL, API token) happened in my own browser session — Claude has no access to that
  account and said so plainly rather than pretending otherwise.
- **Handling real credentials.** Every `dotnet user-secrets set` command with a real value was run
  by me, in my own terminal, on purpose — this was a boundary I set deliberately (and, per §2, one
  I briefly broke myself by pasting output back into chat, caught immediately).
  Claude never had a real secret typed into it by me as an instruction to use directly.
- **Deciding what to do with duplicate test data** before applying the unique-index migration —
  Claude flagged the collision and could have picked a resolution itself, but since some of that
  data was mine, not just leftover from its own testing, I chose to clean it up myself rather than
  let it guess which rows mattered.
- **Rotating the leaked API token** in the Cloudflare dashboard, after Claude flagged it.

## 5. Where Claude got it wrong

**The clearest concrete mistake:** when I asked for entry/exit-style logging (§3, testing/logging
work not detailed above but present in the codebase), Claude added `LogWarning` calls immediately
before several `throw new NotFoundException(...)` / `throw new ValidationFailedException(...)`
statements in `MemberService`. I asked directly: *"do I need both?"* — and the honest answer was
no. ASP.NET Core's built-in `ExceptionHandlerMiddleware` already logs every exception it catches
at `Error` level, unconditionally, before Claude's own `GlobalExceptionHandler` even runs. So every
one of those code paths was already being logged once by the framework; Claude's new lines made it
twice, with near-identical text, and at the *wrong* relative severity to boot (a 404 isn't really
an `Error`). I asked to strip the redundant lines back out rather than suppress the framework's
logging, and that's what happened — the fix was small, but the root cause (not checking what was
already logging the same event before adding a new log call) was a real gap in that turn's
reasoning.

**A related, smaller instance of an incomplete mental model:** when I asked directly whether
`builder.Services.AddProblemDetails()` was actually required, Claude's honest answer — before
testing — would likely have understated its role; instead of asserting, it removed the line, ran
the app, and watched it fail at startup with a clear error naming the actual reason (a hard
dependency inside `UseExceptionHandler()` when no fallback path or `ProblemDetails` service is
configured, independent of whether the custom handler happens to catch everything at runtime).
The original explanation for why that line existed hadn't been wrong exactly, but it was
incomplete — I only found that out because I asked a pointed enough question to make verifying it
worthwhile, and Claude verified instead of just restating itself with more confidence.

I don't have a case in this project where Claude produced code that shipped with a real, unnoticed
functional bug — the closest calls are the two above, both caught within the same turn they came
up in, and both caught because I asked a direct question rather than accepting output at face
value. I think that says more about the value of asking pointed questions mid-build than about the
model rarely being wrong.

---

## Appendix

The full sequence of prompts, in order, with brief notes on what each led to, is in
[PROMPT-LOG.md](PROMPT-LOG.md).
