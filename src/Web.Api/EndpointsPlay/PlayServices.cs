using CleanArch.Web.Api.EndpointsPlay.Pdfs;
using CleanArch.Web.Api.EndpointsPlay.QueryBenchmarks;

namespace CleanArch.Web.Api.EndpointsPlay;

public static class PlayServices
{
    public static void AddPlayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<InvoiceFactory>();
    }
}
