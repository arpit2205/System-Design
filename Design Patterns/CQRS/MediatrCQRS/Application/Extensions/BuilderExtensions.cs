using Application.Middlewares;

namespace Application.Extensions;

public static class BuilderExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}