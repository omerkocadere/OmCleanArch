using MongoDB.Entities;
using Newtonsoft.Json.Linq;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
{
    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdatedDate = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.LastModified))
            .Project(x => x.LastModified.ToString())
            .ExecuteFirstAsync();

        var json = await httpClient.GetStringAsync(
            config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdatedDate
        );

        var arr = JArray.Parse(json);
        foreach (var obj in arr)
        {
            obj["id"] = obj["id"]?.ToString();
        }
        var items = arr.ToObject<List<Item>>();
        return items ?? [];
    }
}
