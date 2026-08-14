// Production: Static Web Apps serves only static files (no built-in proxy
// to an external App Service on the Free tier — "linked backends" that
// would allow that are a Standard-plan feature), so API calls need the
// backend's real, absolute origin. CORS on the backend (Cors:AllowedOrigins)
// is what makes cross-origin calls from this app's own origin actually work.
export const environment = {
  production: true,
  apiBaseUrl: 'https://todoapp-api-us3zbx.azurewebsites.net',
};
