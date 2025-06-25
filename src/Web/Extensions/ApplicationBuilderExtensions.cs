namespace CleanArch.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        // Adds middleware to the HTTP request pipeline that exposes the generated
        // OpenAPI JSON document as a web endpoint (commonly at /swagger/v1/swagger.json).
        app.UseOpenApi();
        app.UseSwaggerUi();

        return app;
    }
}
