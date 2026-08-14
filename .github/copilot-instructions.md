# Copilot instructions — frontend/

This file scopes GitHub Copilot to the `frontend/` folder in this repo. The
backend (`backend/`) is Claude's territory — Copilot shouldn't need to touch
it, and shouldn't guess at its internals. Everything Copilot needs to know
about the backend is the API contract below.

## The API contract

The backend's OpenAPI spec is checked in at [`docs/api/openapi.json`](../docs/api/openapi.json)
— regenerated from the running `TodoApp.WebApi` project (`/swagger/v1/swagger.json`)
whenever an endpoint changes. Treat it as the source of truth for request/response
shapes, not the C# source. If a capability the frontend needs isn't in that
file yet, that's a sign to ask for the endpoint rather than guess its shape.

Local dev API base URL: `http://localhost:5080` (see `backend/src/WebApi/Properties/launchSettings.json`).

## Conventions

- **Standalone components**, no `NgModule`s — matches modern Angular CLI defaults.
- **Signals** for local component state, not NgRx. This app doesn't have the
  cross-cutting state complexity that justifies a state management library.
- **Angular's built-in `HttpClient`** with functional interceptors (not the
  older class-based `HttpInterceptor`).
- Feature-folder organization mirroring the backend's vertical slices where
  it makes sense (e.g. a `todo-lists/` folder), not a `components/` /
  `services/` / `models/` split by technical layer.
- Business rules and validation are backend-authoritative (see `TodoList`/
  `TodoItem` in `backend/src/Domain/Entities/`) — client-side validation
  here is for UX responsiveness, not the source of truth. Don't re-implement
  invariants like title length limits without a reason beyond "avoid a
  round-trip"; the backend enforces them regardless.
- Auth/security scheme: not designed yet. If the API contract doesn't show
  an auth requirement, don't add one speculatively.

## What `ng new` actually scaffolded

- **Angular 22.1**, standalone components, routing enabled.
- **Vitest** for unit tests (`@angular/build:unit-test` builder) — not
  Karma/Jasmine. `npm test` runs once and exits; no watch-mode flags needed.
- **Plain CSS**, no SCSS — kept simple on purpose for an app this size.
- **No SSR** — this is a client-rendered SPA talking to a separate API, SSR's
  hydration complexity buys nothing here.
- Node 24.x in CI (`frontend-ci.yml`), matching what was verified locally.

If any of the above stops being true (someone adds SCSS, switches test
runners, etc.), update this section — it should track `frontend/`'s actual
`package.json`/`angular.json`, not the other way around.

## CI

`frontend-ci.yml` runs on any change under `frontend/**` — build + test,
mirroring `backend-ci.yml`'s gate on `development`/`release`/`master`.

## Daily notes

Log frontend work in [`docs/daily-notes/frontend/`](../docs/daily-notes/frontend/) —
one file per day, `YYYY-MM-DD.md`, using that folder's `TEMPLATE.md`.
Backend has its own separate log in `docs/daily-notes/backend/`; don't
write to that one. See [`docs/daily-notes/README.md`](../docs/daily-notes/README.md)
for why the split exists.

## Scope

Stay inside `frontend/`. Repo-root tooling — `.github/workflows/`,
`.githooks/`, build scripts, anything that affects the backend or the repo
as a whole — is out of scope for a frontend PR. If something like that
seems genuinely needed, raise it as its own conversation instead of
bundling it into a feature PR; it needs review from whoever owns that
territory, not a silent addition.

## Review flow

Claude reviews every PR out of `frontend/` before Diego merges it —
agreed after the first PR shipped a feature that didn't actually work
(missing backend endpoint, CORS, a wrong assumption about `fetch()` vs
`HttpClient`) plus an out-of-scope addition that turned out to be broken
(a pre-commit hook that flagged any file containing the word "password").
None of that would've cost more than a few minutes to catch pre-merge.

Flow: Copilot opens a PR → Diego asks Claude to review it → Claude approves
or flags concrete issues (with evidence — actually running the code, not
just reading it) → Diego merges. This file should stay accurate enough
that most issues get caught before the PR even opens, not after.

**Always open a new branch/PR of your own.** Never push commits onto a
branch or PR you didn't open — even one that looks related, even one still
open. That's exactly how a real fix (switching to HttpClient, adding
proxy.conf.json) ended up merged unreviewed: it landed as a second commit
on someone else's already-in-flight docs PR instead of its own PR, so it
rode through on that PR's approval instead of getting its own look.

## Frontend findings become issues, not just chat

When Claude finds a frontend bug or gap — in PR review, or from actually
clicking through the running app (UAT, not just `curl`) — it gets filed as
a GitHub issue labeled `frontend`, not just mentioned in conversation.
Check [open `frontend`-labeled issues](https://github.com/nahunp/todo-app/issues?q=is%3Aissue+is%3Aopen+label%3Afrontend)
before starting new work; one of them might already be the next thing to
do. Issue #30 is the first of these — three things found by actually
running the app in a browser (`prompt()`/`confirm()` silently failing in
some browser contexts, a leftover Angular scaffold placeholder never
deleted from `app.html`, no way to mark an item complete) that no amount
of API-level testing would have caught.
