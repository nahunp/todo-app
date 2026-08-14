# CLAUDE.md

Read this first, every session. It's the durable reference — architecture,
conventions, gotchas already learned the hard way. History (what happened,
which PR, when) lives in `docs/daily-notes/`, not here; don't duplicate it
into this file as things progress. Keep this file about what's *true now*
and *why*, not a changelog.

## What this repo is

A Todo app built to learn Clean Architecture, SOLID, and design patterns
end to end. `backend/` (.NET) + `frontend/` (Angular) + SQL Server, one
repo. **Android (Gemini-driven) and iOS (DeepSeek-driven) clients are
planned as separate repos**, each consuming this backend's API — this repo
doesn't grow `android/`/`ios/` folders; those get their own CLAUDE.md when
they exist. This repo's job is to keep `backend/` clean enough that any
client, in any repo, can consume it without guessing.

## Environment reality (don't assume otherwise)

Despite earlier session notes claiming "no .NET SDK / no Node available" —
that's stale. This working environment actually has `dotnet`, `node`/`npm`,
`sqlcmd`, and `gh` all on `PATH` (verify with a quick `--version` check if
unsure; PowerShell sessions here sometimes start with a stale `PATH` — see
"PATH gotcha" below). **Build it, test it, run it, and hit real endpoints
against the real database before claiming something works.** This project's
history includes several bugs (a JSON-serialization gotcha, an EF Core
`.Include()` misuse, a `PUT`-vs-`PATCH` mismatch) that only surfaced by
actually running the code — never assume a diff is correct just because it
reads correctly.

**PATH gotcha**: if `dotnet`/`node`/`npm` report "not recognized" in a
PowerShell call, the session's inherited `PATH` is stale (usually because
something was installed mid-session). Fix per-call:
```powershell
$env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [Environment]::GetEnvironmentVariable("Path", "User")
```

## Stack

- **Backend**: .NET 8, ASP.NET Core Minimal APIs (not Controllers), EF Core 8 + SQL Server, MediatR **12.4.1 pinned** (v13+ requires a commercial license — Lucky Penny Software, mid-2025; don't bump the major version without checking licensing again), FluentValidation, Swashbuckle.
- **Frontend**: Angular 22, standalone components, Signals (not NgRx), Vitest (not Karma), plain CSS, no SSR. Full detail in [`.github/copilot-instructions.md`](.github/copilot-instructions.md) — that file is Copilot's onboarding doc; don't duplicate its content here, link to it.
- **Local dev DB**: SQL Server Express, `localhost\SQLEXPRESS`, Windows Authentication. Connection string lives in **.NET User Secrets**, never in source, never pasted into chat — see `backend/src/Infrastructure/Persistence/ApplicationDbContextFactory.cs`'s doc comment.

## Backend architecture — Clean Architecture, dependency rule enforced by project references

```
Domain          -> references nothing (no PackageReference at all, enforced by convention + a comment in its .csproj)
Application     -> references only Domain
Infrastructure  -> references only Application (Domain comes along transitively)
WebApi          -> references Application + Infrastructure (composition root)
```

- **Domain** (`backend/src/Domain/`): `TodoItem`, `TodoList`. `TodoList` is
  the **aggregate root** — the only supported entry point for anything that
  needs cross-item consistency (currently: no duplicate item titles within
  a list). Entities have no public setters; mutation only through named
  methods (`MarkComplete`/`Reopen`/`Rename`/`AddItem`/`RemoveItem`/etc.),
  each enforcing its own invariant and raising domain events where that
  matters (`TodoItemCompletedEvent`).
  - **Known, accepted gap**: `TodoItem.Rename` is still `public` (needed so
    it stays independently testable). Nothing at the compiler level stops
    bypassing `TodoList.RenameItem`'s uniqueness check by calling
    `item.Rename` directly — it's a convention (Application handlers always
    go through the aggregate), not an enforced one. Don't "fix" this by
    making it private; that breaks `TodoItemTests`.
  - `TodoItem` also carries `Priority` (`PriorityLevel`: Low/Medium/High,
    default Medium), `DueDate` (nullable), and `Category`
    (`TodoItemCategory`: None/Work/Personal/Health, default None) — driven
    by the Cloud Dancer brand standards' "todo-specific patterns" section
    (`docs/design/brand-standards.pdf`). `Priority`/`DueDate` were already
    scaffolded in Domain long before anything used them (dead code until
    the Application/WebApi layers caught up); `Category` is new.
    `TodoItem.GetDueDateState(asOf)` computes Overdue/Today/Upcoming/None
    from `DueDate` — a pure function taking "now" as a parameter, not
    calling `DateTimeOffset.UtcNow` itself, since it's a read-time
    projection (what today's date is *when you ask*), not a fact being
    recorded the way `CompletedAt` is. Never stored.
- **Application** (`backend/src/Application/`): CQRS via MediatR. One
  folder per feature, vertical-slice style —
  `TodoLists/Commands/<Verb><Noun>/` and `TodoLists/Queries/<Noun>/`, each
  with its `Command`/`Query` record, `Handler`, and (for commands)
  `Validator` in the same folder. **Gotcha, hit twice**: `IRequest` (no
  generic type param — a "returns nothing" command) pairs with
  `IRequestHandler<TRequest>`, whose `Handle` returns plain `Task` — **not**
  `Task<Unit>`. Getting this wrong is a real compile error (`CS0738`), not
  a style nit.
  - `IApplicationDbContext` is the persistence abstraction; `Infrastructure`
    implements it. Application never references EF Core's SQL Server
    provider, only the core package (for `DbSet<T>`).
  - `ValidationBehaviour` (MediatR pipeline) runs FluentValidation before
    every handler — this is what actually wires validators in; a
    `*Validator` class with nothing registering the pipeline is dead code.
  - Three exception types, three different meanings, all mapped by
    `WebApi/Common/GlobalExceptionHandler.cs`:
    `TodoApp.Application.Common.Exceptions.ValidationException` (malformed
    request → 400 with field errors), `TodoApp.Domain.Exceptions.DomainException`
    (well-formed request, breaks a business rule → 400), `NotFoundException`
    (well-formed request, target doesn't exist → 404). Anything else → 500,
    no details leaked.
- **Infrastructure** (`backend/src/Infrastructure/`): EF Core
  `ApplicationDbContext`, `IEntityTypeConfiguration<T>` classes in
  `Persistence/Configurations/`. `TodoList.Items` is a computed, get-only
  property over a private field (`_items`) — deliberate DDD encapsulation,
  not an oversight. EF Core maps it via
  `.Navigation(l => l.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field)`
  in `TodoListConfiguration`, and `.Include(l => l.Items)` works normally
  against that — **no `EF.Property<T>()` hack needed** (a real mistake made
  once already). `TodoItem`'s list FK is a shadow property, required
  (`IsRequired()` — an item only ever exists via `TodoList.AddItem`),
  cascade delete.
  - `ApplicationDbContextFactory` (`IDesignTimeDbContextFactory`) exists
    *only* for migration tooling (`Add-Migration`/`Update-Database`/
    `dotnet ef`), since there's no separate host project reading
    `appsettings.json` yet — the real app reads config normally via
    `AddInfrastructureServices(configuration)`.
  - **PMC gotcha**: Visual Studio's Package Manager Console keeps a
    project's compiled assembly loaded for the whole session and won't
    reload it after a rebuild — `Add-Migration`/`Update-Database` can
    silently run against stale code. Prefer `dotnet ef` on the CLI (fresh
    process every invocation) if PMC gives a confusing error after a fix.
- **WebApi** (`backend/src/WebApi/`): Minimal API endpoints, one
  `Map*Endpoints` extension per feature area. **All routes are versioned**,
  `/api/v1/todolists/...` — introduced once Android/iOS clients were on the
  roadmap, so a future breaking change adds `/v2` alongside `/v1` instead of
  forcing every client to update in lockstep. `docs/api/openapi.json` is
  regenerated from the live `/swagger/v1/swagger.json` whenever a route
  changes, and is the source of truth for any client (this repo's frontend,
  or a future Android/iOS repo) — treat it as the contract, not the C#
  source.
  - **Enums serialize as their string names** (`"High"`, `"Work"`), not
    raw ints — `builder.Services.ConfigureHttpJsonOptions` in `Program.cs`
    registers a global `JsonStringEnumConverter`. Two things that have to
    move together whenever a new enum-typed field is added: (1) that
    converter only changes *runtime* request/response bodies, not the
    generated OpenAPI schema — Swashbuckle doesn't read
    `JsonSerializerOptions.Converters` on its own, so
    `WebApi/Common/EnumSchemaFilter.cs` (registered via
    `options.SchemaFilter<EnumSchemaFilter>()` in the same `AddSwaggerGen`
    call) is what keeps `docs/api/openapi.json` honest about it — found by
    generating the spec and actually diffing it against a real
    request/response, not by assuming they'd agree once the converter was
    in. (2) `JsonStringEnumConverter` still accepts an in-range-for-int,
    out-of-range-for-the-enum value (e.g. `"priority": 99`) — that's what
    each `Set*` command's `.IsInEnum()` FluentValidation rule catches; the
    converter alone isn't enough.
  - **`BadHttpRequestException`** (a malformed request body — e.g. an enum
    string that matches no defined name) is mapped to 400 in
    `GlobalExceptionHandler`, same as the other "client sent something
    wrong" cases. Found live: it fell through to the generic 500 case
    before that mapping existed, which hid a client mistake behind a
    server-error-shaped response.

## Testing

- **Domain**: plain xUnit, no dependencies (`TodoApp.Domain.UnitTests`).
- **Application**: xUnit + **EF Core's InMemory provider**
  (`ApplicationDbContextFake`), not a mocking library — `DbSet<T>` is
  `IQueryable`, not just a list, and mocks it poorly. Each test gets its
  own uniquely-named in-memory database.
- No integration/E2E test project yet — "verified live" in PR descriptions
  means an actual human-run session (`dotnet run` + `curl`/browser), not an
  automated suite. Worth building a real integration-test project once
  there's a second consumer of the backend depending on it not regressing
  silently.
- `FluentValidation.TestHelper` is a **namespace**, not a separate NuGet
  package — it's bundled in the core `FluentValidation` package. (A wrong
  assumption that cost a debugging round once.)

## Process

- **Branching**: `development` → `release` (daily auto-promote) → `master`
  (every 2 days), all PR-gated with required `build-and-test`. `teflon` is
  an unprotected sandbox, auto-synced from `development`. See `README.md`
  for the full pipeline.
- **Territory split, this repo**: `backend/` is Claude's, `frontend/` is
  Copilot's — see `.github/copilot-instructions.md` for the frontend-side
  rules (scope, daily notes location, review flow). Don't touch
  `frontend/` source directly except as a documented, attributed exception
  (it's happened twice — always noted explicitly in the commit/PR when it
  does, never silent).
- **Current review flow** (subject to change back — check
  `copilot-instructions.md` for the live version): Copilot commits + pushes
  to its own branch but can't open PRs itself (lost GitHub API token, git
  push still works) → Diego pings Claude → Claude pulls it, actually runs
  it, opens the PR crediting Copilot's real authorship → Diego merges.
- **Frontend findings become GitHub issues** (`frontend` label), not just
  chat — filed whenever Claude finds a bug/gap reviewing a PR or clicking
  through the running app.
- **Secrets**: never ask for one in chat, never hardcode one, never accept
  a pasted token/password/connection string as a "fix." Say what config
  key and where (User Secrets, GitHub Actions secrets, etc.); the human
  enters the value themselves.
- **After a merge, verify the actual file content landed** — don't trust
  the "Merged" badge alone. This repo has hit a stale squash-merge that
  silently dropped commits once; the fix was diffing real trees, not
  re-reading PR descriptions.

## Auth

**Self-hosted ASP.NET Core Identity + JWT Bearer**, access-token-only
(no refresh tokens yet — deliberate fast-follow, not an oversight).
`TodoList.OwnerId` ties every list to the `ApplicationUser` that created
it; every existing command/query handler takes `ICurrentUserService` and
either filters by owner (`GetTodoListsQuery`) or calls
`list.EnsureOwnedBy(_currentUser.UserId)` right after loading
(`Application/Common/Security/OwnershipExtensions.cs`). A non-owner
touching someone else's list gets **404, not 403** — deliberate, so a
non-owner can't tell a resource exists at all (OWASP-aligned).

- `POST /api/v1/auth/register`, `POST /api/v1/auth/login`,
  `GET /api/v1/auth/password-policy` — the only unauthenticated routes.
  Everything under `/api/v1/todolists` requires a valid Bearer token
  (`RequireAuthorization()` on the route group).
- **Registration is hardened, since this app is meant to be shared
  publicly**: `AddIdentityCore`'s password policy is set explicitly
  (`Infrastructure/DependencyInjection.cs`) rather than left as unstated
  defaults — `GET /api/v1/auth/password-policy` exposes the live values
  so the frontend can build a real requirements checklist instead of
  guessing. `RegisterCommand` also requires a `CaptchaToken`
  (`ICaptchaService`/`TurnstileCaptchaService` — Cloudflare Turnstile),
  verified *before* Identity is ever touched, so a bot hammering
  `/register` doesn't cost a real `CreateUserAsync`/DB round-trip per
  attempt. `Captcha:SecretKey` in config: locally and in the committed
  `appsettings.json`, it's Cloudflare's own published always-passes test
  secret (`1x0000...AA`, paired with test site key `1x0000...AA` on the
  frontend) — not a real secret, safe to commit, specifically documented
  by Cloudflare for exactly this. Production uses the real secret for the
  Turnstile site actually registered against the deployed domain, set as
  an Azure App Setting like every other real secret — never in source.
- `AddIdentityCore` (not `AddIdentity`) in `Infrastructure/DependencyInjection.cs`
  — this is an API-only, no-cookie scenario, and `AddIdentity` would wire up
  a cookie auth scheme that conflicts with JWT Bearer as the sole scheme.
- Infrastructure targets plain `Microsoft.NET.Sdk`, not `.Sdk.Web`, so it
  needs `<FrameworkReference Include="Microsoft.AspNetCore.App" />` in its
  `.csproj` to get `AddDefaultTokenProviders()` and friends — the standard
  fix for "Identity in a class library."
- **`options.MapInboundClaims = false`** on `AddJwtBearer` in `Program.cs`
  is load-bearing, not cosmetic — without it, ASP.NET Core silently
  rewrites short claim names (`sub`) to long `ClaimTypes` URIs, and
  `CurrentUserService`'s `FindFirstValue(JwtRegisteredClaimNames.Sub)`
  quietly stops matching.
- Signing key lives in **User Secrets** as `Jwt:SigningKey`, alongside the
  connection string — never in `appsettings.json`, never committed.
  `Jwt:Issuer`/`Jwt:Audience` are non-secret and live in `appsettings.json`.
- **Environment gotcha, hit once**: if a tool/process reads or writes
  `%APPDATA%\Microsoft\UserSecrets\...\secrets.json` through a sandboxed
  shell, its filesystem view of paths outside the repo can be stale
  relative to what an unsandboxed shell (or the actual running app) sees.
  If a freshly-written User Secrets value doesn't seem to take effect,
  re-verify the file's content from a different tool before assuming the
  app or the config system is broken.

## Deployed

Live on Azure (free tier) — see README.md's "Deployment" section for
resource names, redeploy commands, and the one real known limitation
(serverless Azure SQL cold-start). Two gotchas worth knowing before
touching either service again:

- **Frontend backend-URL config is runtime, not build-time.** Don't add
  back an `environment.ts`/`environment.prod.ts` + `fileReplacements`
  setup — that was tried first and silently broke `TodoListService`'s
  calls (misdiagnosed at the time as an esbuild lazy-chunk bug; the real
  cause was unrelated — see `runtime-config.ts`'s doc comment for the
  full story). `public/config.js` sets `window.__appConfig` before
  Angular's bundle loads; that's the one and only place the deployed
  backend URL lives outside source.
- **A frontend service method existing doesn't mean a component uses
  it.** `todo-list.ts`'s `load()`/`create()` called `HttpClient` directly
  with hardcoded paths for a long time after `TodoListService` already
  had the right methods — easy to miss since it still worked locally
  (relative paths resolve fine against `ng serve`'s own proxy). Only
  surfaced once frontend and backend were genuinely different origins.
  If a service method exists, grep for who's actually calling it before
  assuming a bug is elsewhere.

## Open / not yet designed

- **Refresh tokens** — access tokens are 60 minutes, no refresh flow yet.
  Fine for now (single desktop client, short dev sessions); revisit before
  Android/iOS clients need to stay logged in across app restarts.
- **Deployment is automated as of today** — `backend-deploy.yml` /
  `frontend-deploy.yml` deploy on every push to `master` (see README.md's
  Deployment section). Database migrations are the deliberate exception —
  still a manual step, same reasoning as `Program.cs`'s comment on why
  they don't run on app startup either. If a change includes a migration,
  apply it by hand before merging the code change to `master`.
- **List CRUD is complete** as of this writing (create/rename/delete a
  list; add/rename/remove/complete/reopen an item; set an item's
  priority/due-date/category), with per-user ownership — if any of that
  stops being true, update this line, don't leave it stale.
- **Categories are a fixed 4-value enum** (None/Work/Personal/Health),
  matching the brand standards' fixed color mapping for exactly those
  three named categories. Not user-defined/arbitrary categories — that's
  a materially bigger feature (category management, color assignment)
  the brand guide doesn't ask for and this backend doesn't support yet.
