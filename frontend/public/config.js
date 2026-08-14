// Local dev default — empty apiBaseUrl means relative paths, which
// ng serve's proxy.conf.json forwards to the local backend. Deployments
// overwrite this exact file (dist/frontend/browser/config.js) with the
// real backend origin as a post-build step, not a rebuild. See
// index.html and runtime-config.ts for why this is a plain script
// setting a window global, not a JSON asset fetched into a JS module.
window.__appConfig = {
  apiBaseUrl: "",
  // Cloudflare's published always-passes test site key, paired with the
  // backend's own committed test secret — see CLAUDE.md's Auth section.
  // Deployments overwrite this with the real production site key, same as
  // apiBaseUrl above.
  turnstileSiteKey: "1x00000000000000000000AA",
};
