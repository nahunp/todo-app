// See index.html's comment on the <script src="config.js"> tag for the
// full story. Short version: config.js runs before Angular's bundle and
// sets a genuine window global (window.__appConfig) — not a JS module
// export — because a shared mutable object exported from a module turned
// out to *not* reliably stay a single instance across Angular's lazily
// loaded route chunks with this build (confirmed live: AuthService, in
// the eager main.js bundle, saw an updated value; TodoListService, only
// reachable via the lazy /lists route, still saw its own chunk's
// never-updated copy of the "same" object). window is immune to that —
// every chunk reads the one true runtime global, no bundler resolution
// involved at all.
declare global {
  interface Window {
    __appConfig?: { apiBaseUrl?: string; turnstileSiteKey?: string };
  }
}

export const runtimeConfig = {
  get apiBaseUrl(): string {
    return window.__appConfig?.apiBaseUrl ?? '';
  },
  // Public by design — Turnstile's site key is meant to ship in client
  // code (it identifies *which* site is asking for verification, not a
  // credential). The secret key that actually verifies tokens stays
  // backend-only and never reaches this file. Same runtime-config split
  // as apiBaseUrl: Cloudflare's published test key here for local dev,
  // the real production key set via config.js at deploy time.
  get turnstileSiteKey(): string {
    return window.__appConfig?.turnstileSiteKey ?? '1x00000000000000000000AA';
  },
};
