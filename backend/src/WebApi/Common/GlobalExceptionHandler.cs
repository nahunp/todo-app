using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Exceptions;
using ApplicationValidationException = TodoApp.Application.Common.Exceptions.ValidationException;
using NotFoundException = TodoApp.Application.Common.Exceptions.NotFoundException;

namespace TodoApp.WebApi.Common;

/// <summary>
/// The payoff for a decision made all the way back when DomainException was
/// first written: "the API layer can catch DomainException specifically and
/// turn it into a 400 Bad Request, while anything else falls through as a
/// 500." This is that catch. Registered once via AddExceptionHandler in
/// Program.cs — no per-endpoint try/catch anywhere.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            // FluentValidation failures — malformed requests, caught before
            // any handler runs. Field-level errors, ASP.NET Core's own
            // ValidationProblemDetails shape.
            ApplicationValidationException validationException => new ValidationProblemDetails(validationException.Errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            },

            // A domain invariant was violated (e.g. TodoListNameInvalidException,
            // DuplicateTodoItemTitleException) — the request was well-formed,
            // but what it asked for breaks a business rule.
            DomainException domainException => new ProblemDetails
            {
                Title = "A business rule was violated.",
                Detail = domainException.Message,
                Status = StatusCodes.Status400BadRequest,
            },

            // The request was well-formed and valid, but what it points at
            // (a TodoListId/TodoItemId) doesn't exist.
            NotFoundException notFoundException => new ProblemDetails
            {
                Title = "The requested resource was not found.",
                Detail = notFoundException.Message,
                Status = StatusCodes.Status404NotFound,
            },

            // Two requests raced on the same row (classic case: a
            // double-submitted delete — confirmed live, a rapid double-click
            // fired two DELETE requests for the same list; the first
            // succeeded, the second's SaveChangesAsync affected 0 rows
            // instead of the expected 1). By the time this request's write
            // reached the database, the row the first request already
            // removed was gone - functionally the same as it never having
            // existed for this request, so 404 rather than a generic 500.
            DbUpdateConcurrencyException => new ProblemDetails
            {
                Title = "The requested resource was not found.",
                Detail = "It may have just been modified or deleted by another request.",
                Status = StatusCodes.Status404NotFound,
            },

            // Login rejected the email/password pair — thrown from
            // LoginCommandHandler, deliberately not distinguishing "no such
            // user" from "wrong password" in the message either.
            UnauthorizedAccessException unauthorizedException => new ProblemDetails
            {
                Title = "Authentication failed.",
                Detail = unauthorizedException.Message,
                Status = StatusCodes.Status401Unauthorized,
            },

            // Anything else is a bug or an infrastructure failure, not
            // something the caller did wrong — no exception details leak
            // into the response.
            _ => new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
            },
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        // problemDetails is statically typed ProblemDetails (the switch
        // expression's common type across branches), but the actual object
        // is a ValidationProblemDetails when validation failed. Passing the
        // runtime type explicitly is required — otherwise
        // System.Text.Json serializes against the *static* type and
        // silently drops Errors, since that property only exists on the
        // derived type. Found by actually curling the endpoint, not by
        // inspection.
        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

        return true;
    }
}
