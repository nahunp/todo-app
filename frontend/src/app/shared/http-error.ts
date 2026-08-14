import { HttpErrorResponse } from '@angular/common/http';

// Every backend error is a ProblemDetails-shaped JSON body (see
// GlobalExceptionHandler.cs) — `detail` for most cases (domain rule
// violations, not-found, login rejection), `errors` for FluentValidation
// failures (the standard ASP.NET Core field->messages[] shape), `title` as
// a last resort for the generic 500 case, which deliberately carries no
// detail. Angular's own HttpErrorResponse.message is a generic technical
// string ("Http failure response for <url>: 401 Unauthorized") that's not
// fit to show a user — this pulls the actual reason out of the response
// body instead. status 0 is Angular's shape for "the request never got a
// response at all" (offline, DNS failure, CORS rejection, or — the one
// that actually prompted this — Azure SQL's free-tier cold start taking
// long enough that it read as a hang rather than a slow success).
export function extractErrorMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) {
      return "Couldn't reach the server. Check your connection and try again — the free-tier database can take a little while to wake up after sitting idle.";
    }

    const body = err.error as { detail?: string; errors?: Record<string, string[]>; title?: string } | null;
    if (body?.detail) return body.detail;
    if (body?.errors) {
      const messages = Object.values(body.errors).flat();
      if (messages.length) return messages.join(' ');
    }
    if (body?.title) return body.title;

    return `Something went wrong (${err.status}). Please try again.`;
  }

  return 'Something went wrong. Please try again.';
}
