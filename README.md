# TodoApp

A Todo app built step by step to learn Clean Architecture, SOLID principles, and design patterns — .NET (backend) + Angular (frontend) + SQL Server, deploying to Azure eventually.

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
