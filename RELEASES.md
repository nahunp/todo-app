# Releases

## v1.0.0 — 2026-08-14

First and, for now, final release of the web app. This project's purpose was
always to practice designing, building, and deploying an enterprise-grade,
cloud-ready application end to end — architecture, auth, CI/CD, all of it —
using AI coding agents (Claude and GitHub Copilot) as the actual developers.
That goal is met: the app is feature-complete for its original scope, live,
and deploying itself automatically. Development effort now shifts to mobile
(Android and iOS clients, planned as their own separate repos consuming this
backend's API — see `CLAUDE.md`) — this repo isn't expected to get new
features after this tag, though it may still get fixes.

**Live**: [frontend](https://zealous-meadow-0c73a9610.7.azurestaticapps.net) · [backend API](https://todoapp-api-us3zbx.azurewebsites.net)

### What's in v1.0

**Core todo functionality**
- Create, rename, and delete lists; add, rename, remove, complete, and
  reopen items.
- Per-item priority (Low/Medium/High), due date, and category
  (Work/Personal/Health), styled per a small design system generated from a
  Cloud-Dancer-inspired palette.
- Due-date state (Overdue/Today/Upcoming) computed at read time, not stored.

**Accounts & security**
- Email/password accounts (ASP.NET Core Identity + JWT Bearer), every list
  scoped to its owner — a non-owner touching someone else's list gets 404,
  not 403, so existence can't be inferred.
- A live password-requirements checklist on the registration form, driven by
  an API endpoint so it can never drift from what's actually enforced.
- Cloudflare Turnstile CAPTCHA on registration, verified server-side.
- Terms of Service and Privacy Policy pages, linked from the app footer —
  this being a personal learning project, not a company, is stated plainly.

**Architecture**
- Clean Architecture on the backend (Domain → Application → Infrastructure →
  WebApi), CQRS via MediatR, FluentValidation, EF Core 8 + SQL Server.
- Angular 22 on the frontend — standalone components, Signals, no NgRx.
- Versioned API (`/api/v1`) with a generated OpenAPI contract, so future
  clients (mobile included) have a stable surface to build against.

**Infrastructure & delivery**
- Deployed on Azure (App Service, Azure SQL serverless, Static Web Apps),
  entirely free tier.
- Three-stage branch pipeline (`development` → `release` → `master`) with
  required CI on every stage.
- Deployment to production is now automated on every push to `master` —
  federated (OIDC) login, no stored deploy credentials.

### Known limitations (by design, not oversights)

- No refresh tokens — access tokens are 60 minutes, single desktop client
  was the assumption for v1.
- Categories are a fixed 4-value enum, not user-defined.
- Database migrations are a deliberate manual deploy step, not automated —
  see `Program.cs`'s own comment on why.
- Azure SQL's free tier is serverless and auto-pauses; a request landing
  during a cold resume can be slow (mitigated with retry-on-failure, not
  eliminated).

### How it was built

Backend was Claude's territory, frontend was GitHub Copilot's — a real
attempt at simulating a two-developer team (documented in `CLAUDE.md` and
`.github/copilot-instructions.md`), including a proper review flow, daily
notes per side, and GitHub issues for cross-team findings. All under one
GitHub account for now; a real second developer account is a possible future
step.
