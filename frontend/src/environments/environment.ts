// Local dev default. Empty apiBaseUrl means relative paths (/api/v1/...),
// which `ng serve`'s proxy.conf.json forwards to the local backend
// (http://localhost:5080). Swapped for environment.prod.ts on production
// builds via angular.json's fileReplacements.
export const environment = {
  production: false,
  apiBaseUrl: '',
};
