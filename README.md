# TodoApp

A Todo app built step by step to learn Clean Architecture, SOLID principles, and design patterns — .NET (backend) + Angular (frontend) + SQL Server, deployed to Azure.

**Live**: [zealous-meadow-0c73a9610.7.azurestaticapps.net](https://zealous-meadow-0c73a9610.7.azurestaticapps.net) (frontend) · [todoapp-api-us3zbx.azurewebsites.net](https://todoapp-api-us3zbx.azurewebsites.net) (backend API)

**Backend** (`backend-ci.yml`, triggers on `backend/**`):

| [`development`](https://github.com/nahunp/todo-app/tree/development) | [`release`](https://github.com/nahunp/todo-app/tree/release) | [`master`](https://github.com/nahunp/todo-app/tree/master) |
|:---:|:---:|:---:|
| [![development](https://img.shields.io/github/actions/workflow/status/nahunp/todo-app/backend-ci.yml?branch=development&label=development)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml?query=branch%3Adevelopment) | [![release](https://img.shields.io/github/actions/workflow/status/nahunp/todo-app/backend-ci.yml?branch=release&label=release)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml?query=branch%3Arelease) | [![master](https://img.shields.io/github/actions/workflow/status/nahunp/todo-app/backend-ci.yml?branch=master&label=master)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml?query=branch%3Amaster) |

**Frontend** (`frontend-ci.yml`, triggers on `frontend/**`):

| [`development`](https://github.com/nahunp/todo-app/tree/development) | [`release`](https://github.com/nahunp/todo-app/tree/release) | [`master`](https://github.com/nahunp/todo-app/tree/master) |
|:---:|:---:|:---:|
| [![development](https://img.shields.io/github/actions/workflow/status/nahunp/todo-app/frontend-ci.yml?branch=development&label=development)](https://github.com/nahunp/todo-app/actions/workflows/frontend-ci.yml?query=branch%3Adevelopment) | [![release](https://img.shields.io/github/actions/workflow/status/nahunp/todo-app/frontend-ci.yml?branch=release&label=release)](https://github.com/nahunp/todo-app/actions/workflows/frontend-ci.yml?query=branch%3Arelease) | [![master](https://img.shields.io/github/actions/workflow/status/nahunp/todo-app/frontend-ci.yml?branch=master&label=master)](https://github.com/nahunp/todo-app/actions/workflows/frontend-ci.yml?query=branch%3Amaster) |

Each row's three columns run left to right in promotion order, matching the `development` → `release` → `master` pipeline described below, so each badge's label names its own branch. Every badge is live: green means the latest commit on that branch builds and every test passes, red means something's broken (or, for frontend, that nothing has touched `frontend/` on that branch yet). Click a badge to jump straight to that branch's run history.

## Structure

```
backend/
  TodoApp.sln
  src/
    Domain/           # Entities, value objects, domain events — zero external dependencies
    Application/       # CQRS commands/queries (MediatR), validation (FluentValidation)
    Infrastructure/    # EF Core, SQL Server — implements Application's persistence interfaces
    WebApi/             # Minimal API endpoints, composition root, Swagger
  tests/
    TodoApp.Domain.UnitTests/
    TodoApp.Application.UnitTests/

frontend/             # Angular — Copilot's territory, see .github/copilot-instructions.md
docs/
  api/openapi.json     # The API contract shared between backend and frontend
```

Backend is Claude's territory, frontend is Copilot's — same repo, split by
folder rather than by repo, so branch protection/CI/the promotion pipeline
below cover both without duplicating any of it. `docs/api/openapi.json` is
the one thing both sides need to agree on; everything else about each
side's internals stays that side's own concern.

## Branching

Release pipeline: `development` → `release` → `master`, simulating a
staging/production promotion flow. In a real project this might be a sprint
release every 2 weeks; here it moves faster since we're practicing:

- `development` — integration branch for ongoing work. `feature/*`, `fix/*`,
  `chore/*` branches are PR'd in here, reviewed and merged by hand.
- `release` — staging. Automatically promoted from `development` every day
  at 00:00 America/Mexico_City (see `promote-development-to-release.yml`).
  No manual step — merges itself once `build-and-test` passes.
- `master` — production/stable baseline. Automatically promoted from
  `release` every 2 days, same time (see `promote-release-to-master.yml`),
  standing in for a release cutoff.
- `teflon` — sandbox, no CI gate. For proving something out or reproducing a
  bug without putting a red check on someone else's real PR. Auto-synced
  with `development` on every push (see `sync-teflon.yml`) — always assume
  it's up to date. Branch off `teflon`, not off `development`, to try
  something; nothing here is expected to land in `development` directly.

`development`, `release`, and `master` are all protected: changes must go
through a PR, and `build-and-test` must pass before it can merge. `release`
and `master` additionally have `strict` status checks turned **off** — they're
one-way promotion targets, so requiring the source branch to already contain
the target's latest commit just deadlocks every promotion after the first
(learned that one the hard way). `teflon` has none of these restrictions, on
purpose.

## Deployment

All Azure, all free tier, resource group `rg-todoapp` (Central US — East US
and West US 2 both rejected new SQL server creation on this subscription at
setup time; not a resource-group-specific constraint, just try another
region if it happens again):

| Resource | Name | Purpose |
|---|---|---|
| App Service (Linux, F1 free) | `todoapp-api-us3zbx` | Backend API |
| Azure SQL (free offer, serverless) | `todoapp-sql-aeyls0` / `TodoAppDb` | Database |
| Static Web App (free) | `todoapp-web-vawdeh` | Frontend |

No CI/CD pipeline for deployment yet — both sides are deployed by hand from
a local build. To redeploy:

**Backend**:
```powershell
cd backend
dotnet publish src/WebApi/TodoApp.WebApi.csproj -c Release -o publish-out
Compress-Archive -Path publish-out/* -DestinationPath webapi.zip -Force
az webapp deploy --resource-group rg-todoapp --name todoapp-api-us3zbx --src-path webapi.zip --type zip
```
Migrations don't run automatically in production (see `Program.cs`'s
comment on why) — apply them explicitly, pointed at the Azure connection
string via an environment variable override (works because
`ApplicationDbContextFactory` checks environment variables after User
Secrets):
```powershell
$env:ConnectionStrings__DefaultConnection = "<the Azure SQL connection string>"
cd backend
dotnet ef database update --project src/Infrastructure/TodoApp.Infrastructure.csproj --startup-project src/Infrastructure/TodoApp.Infrastructure.csproj
Remove-Item Env:\ConnectionStrings__DefaultConnection
```
(Running `dotnet ef` from the CLI, not Package Manager Console, and using
Infrastructure as both `--project` and `--startup-project` — WebApi's own
`Design`/`Tools` references are `PrivateAssets="all"` in Infrastructure's
`.csproj`, so they don't flow transitively to WebApi; using WebApi as
`--startup-project` fails with "doesn't reference
Microsoft.EntityFrameworkCore.Design".)

**Frontend**: `frontend/public/config.js` carries the backend's URL as a
runtime `window.__appConfig` global (see `runtime-config.ts`'s doc comment
for why this isn't a build-time `environment.ts` — that approach was tried
first and quietly broke `TodoListService`'s calls). The committed
`config.js` is the local-dev default (empty `apiBaseUrl`); redeploying
means overwriting that one file in the build output before pushing, not
committing a different value:
```powershell
cd frontend
npm run build -- --configuration production
'window.__appConfig = { apiBaseUrl: "https://todoapp-api-us3zbx.azurewebsites.net" };' | Set-Content dist/frontend/browser/config.js -NoNewline
npx @azure/static-web-apps-cli deploy --app-location dist/frontend/browser --deployment-token <token> --env production
```
The deployment token is in the Static Web App's Azure Portal blade (or
`az staticwebapp secrets list`) — not committed anywhere.

**Known limitation**: Azure SQL's free-tier database is serverless and
auto-pauses after a period of no activity, taking tens of seconds to
resume. The backend retries transient failures during that resume
(`EnableRetryOnFailure()`), but a request landing during a genuinely cold
resume can still be slow, or in rare cases time out outright — acceptable
for a free-tier learning-project deployment, not something you'd want on
anything real users depend on staying responsive.
