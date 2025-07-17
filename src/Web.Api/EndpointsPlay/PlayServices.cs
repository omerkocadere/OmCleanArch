using CleanArch.Web.Api.EndpointsPlay.Pdfs;

namespace CleanArch.Web.Api.EndpointsPlay;

public static class PlayServices
{
    public static void AddPlayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<InvoiceFactory>();
    }
}
