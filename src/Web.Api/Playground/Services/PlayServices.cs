using CleanArch.Web.Api.Playground.Pdf.Services;

namespace CleanArch.Web.Api.Playground.Services;

public static class PlayServices
{
    public static void AddPlayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<InvoiceFactory>();
    }
}
