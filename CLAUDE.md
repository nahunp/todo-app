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

## Open / not yet designed

- **No auth or authorization at all.** Any client can see/edit every list.
  Needs its own design conversation (token scheme, identity provider, how
  `TodoList` gets ownership) before real user data or the Android/iOS
  clients show up — don't bolt it on inline without that conversation.
- **Not deployed anywhere.** Local SQL Server Express + `dotnet run` +
  `ng serve` only. README has said "deploying to Azure eventually" since
  day one; still eventually.
- **List CRUD is complete** as of this writing (create/rename/delete a
  list; add/rename/remove/complete/reopen an item) — if that stops being
  true, update this line, don't leave it stale.
