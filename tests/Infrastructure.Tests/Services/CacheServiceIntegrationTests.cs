using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Configuration;
using CleanArch.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Tests.Services;

public class CacheServiceIntegrationTests
{
    [Fact]
    public void Should_Register_MemoryCache_When_Provider_Is_Memory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Cache:Provider", "Memory"},
                {"Cache:DefaultTimeoutMinutes", "30"}
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddMemoryCache();
        services.AddLogging();
        
        // Configure cache options
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        
        var cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();

        if (cacheOptions.Provider.Equals(CacheProviders.Memory, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetService<ICacheService>();

        // Assert
        Assert.NotNull(cacheService);
        Assert.IsType<MemoryCacheService>(cacheService);
    }

    [Fact]
    public async Task Should_Store_And_Retrieve_Data_From_Cache()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLogging();
        services.Configure<CacheOptions>(options =>
        {
            options.Provider = CacheProviders.Memory;
            options.DefaultTimeoutMinutes = 30;
        });
        
        services.AddScoped<ICacheService, MemoryCacheService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        var testData = new TestCacheData { Id = 1, Name = "Test Item" };
        var cacheKey = "test:item:1";

        // Act
        await cacheService.SetAsync(cacheKey, testData, TimeSpan.FromMinutes(5));
        var retrievedData = await cacheService.GetAsync<TestCacheData>(cacheKey);

        // Assert
        Assert.NotNull(retrievedData);
        Assert.Equal(testData.Id, retrievedData.Id);
        Assert.Equal(testData.Name, retrievedData.Name);
    }

    [Fact]
    public async Task Should_Use_Version_Based_Cache_Invalidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLogging();
        services.Configure<CacheOptions>(options =>
        {
            options.Provider = CacheProviders.Memory;
            options.DefaultTimeoutMinutes = 30;
        });
        
        services.AddScoped<ICacheService, MemoryCacheService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        var prefix = "users";
        var key = "profile:123";

        // Act
        var initialVersion = await cacheService.GetVersionAsync(prefix);
        var versionedKey1 = await cacheService.BuildVersionedKeyAsync(prefix, key);
        
        await cacheService.InvalidateVersionAsync(prefix);
        
        var newVersion = await cacheService.GetVersionAsync(prefix);
        var versionedKey2 = await cacheService.BuildVersionedKeyAsync(prefix, key);

        // Assert
        Assert.Equal(1, initialVersion);
        Assert.Equal(2, newVersion);
        Assert.Equal($"{prefix}:{key}:v{initialVersion}", versionedKey1);
        Assert.Equal($"{prefix}:{key}:v{newVersion}", versionedKey2);
        Assert.NotEqual(versionedKey1, versionedKey2);
    }

    private class TestCacheData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}