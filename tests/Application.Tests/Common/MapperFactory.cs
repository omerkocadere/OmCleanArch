using CleanArch.Application.Common.Mappings;

namespace CleanArch.Application.Tests.Common;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ApplicationMappingProfile>();
        });

        return configuration.CreateMapper();
    }
}
