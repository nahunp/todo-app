# TodoApp

A Todo app built step by step to learn Clean Architecture, SOLID principles, and design patterns — .NET (backend) + Angular (frontend) + SQL Server, deploying to Azure eventually.

[![Backend CI (development)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml/badge.svg?branch=development)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml)
[![Backend CI (release)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml/badge.svg?branch=release)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml)
[![Backend CI (master)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml/badge.svg?branch=master)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml)

The badges above are live: green means the latest commit on that branch builds and every test passes, red means something's broken. Same colored checkmark/X shows up on every commit and every pull request.

## Structure

```
backend/
  TodoApp.sln
  src/
    Domain/                     # Entities, value objects, domain events — zero external dependencies
  tests/
    TodoApp.Domain.UnitTests/   # xUnit tests for the Domain layer
```

More layers (Application, Infrastructure, Web API) and the Angular frontend land as we build them.

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
