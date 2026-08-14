// Local dev default — empty apiBaseUrl means relative paths, which
// ng serve's proxy.conf.json forwards to the local backend. Deployments
// overwrite this exact file (dist/frontend/browser/config.js) with the
// real backend origin as a post-build step, not a rebuild. See
// index.html and runtime-config.ts for why this is a plain script
// setting a window global, not a JSON asset fetched into a JS module.
window.__appConfig = {
  apiBaseUrl: "",
};
