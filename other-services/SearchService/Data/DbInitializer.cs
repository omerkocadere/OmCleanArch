using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Data;

public static class DbInitializer
{
    public static async Task InitDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        await DB.InitAsync(
            "SearchDb",
            MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection"))
        );

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();

        if (count == 0)
        {
            logger.LogWarning("No items found in the database.");

            var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "auctions.json");
            var json = await File.ReadAllTextAsync(filePath);
            var items = JsonSerializer.Deserialize<List<Item>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (items == null)
            {
                logger.LogError("Failed to deserialize auctions.json to Item list.");
                return;
            }

            await DB.SaveAsync(items);
            // if (auctionDtos == null)
            // {
            //     logger.LogError("Failed to deserialize auctions.json");
            //     return;
            // }

            // // AuctionDto'dan Item entity'ye dönüştür
            // var items = auctionDtos
            //     .Select(dto => new Item
            //     {
            //         AuctionId = dto.id,
            //         Make = dto.make,
            //         Model = dto.model,
            //         Color = dto.color,
            //         Mileage = dto.mileage,
            //         Year = dto.year,
            //         ImageUrl = dto.imageUrl,
            //     })
            //     .ToList();

            // logger.LogInformation("Seeding {Count} items from auctions.json", items.Count);
            // if (items.Count > 0)
            //     await DB.SaveAsync(items);
        }
    }
}
