using SaaS.API.Middlewares;
using Serilog;

namespace SaaS.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("DefaultPolicy");
        app.UseSerilogRequestLogging();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<TokenVersionValidationMiddleware>();
        app.UseMiddleware<TenantAuthorizationMiddleware>();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
