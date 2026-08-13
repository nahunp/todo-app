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

## Not yet decided — check `frontend/` itself once it exists

`ng new` interactively picks some things (testing framework — Karma/Jasmine
vs. Jest/Vitest — SCSS vs. plain CSS, routing) that aren't fixed yet as of
this file being written. Whatever's actually in `frontend/angular.json` and
`package.json` is the real answer, not this document — update this section
once the scaffold exists if the actual choices diverge from assumptions
made here.

## CI

`frontend-ci.yml` runs on any change under `frontend/**` — build + test,
mirroring `backend-ci.yml`'s gate on `development`/`release`/`master`.
