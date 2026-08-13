# TodoApp

A Todo app built step by step to learn Clean Architecture, SOLID principles, and design patterns — .NET (backend) + Angular (frontend) + SQL Server, deploying to Azure eventually.

[![Backend CI (development)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml/badge.svg?branch=development)](https://github.com/nahunp/todo-app/actions/workflows/backend-ci.yml)
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

- `master` — stable baseline, updated at milestones
- `development` — integration branch for ongoing work
- `feature/*`, `chore/*` — one per unit of work, PR'd into `development`
- `teflon` — sandbox branch, no CI gate. For proving something out, reproducing
  a bug, or testing an idea without putting a red check on someone else's real
  PR. Auto-synced with `development` on every push (see
  `sync-teflon.yml`) — always assume it's up to date. To try something, branch
  off `teflon`, not off `development`; nothing here is expected to land in
  `development` directly.

`master` and `development` are protected: changes must go through a PR, and
the `build-and-test` check must pass before it can merge. `teflon` has neither
restriction on purpose.
