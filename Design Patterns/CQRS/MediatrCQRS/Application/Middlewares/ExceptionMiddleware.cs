using System.Text.Json;
using FluentValidation;

namespace Application.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleServerException(context, ex);
        }

    }

    private static async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var response = new
        {
            Title = "Validation Failed",
            Errors = ex.Errors.Select(x => new
            {
                x.PropertyName,
                x.ErrorMessage
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task HandleServerException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            Title = "Server Error",
            Detail = ex.Message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}