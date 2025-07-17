using BenchmarkDotNet.Running;
using CleanArch.Infrastructure.Data;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.EndpointsPlay.QueryBenchmarks;

// NOTE: Running BenchmarkDotNet from within a web application is not recommended for production use.
// - BenchmarkRunner.Run blocks the HTTP request until all benchmarks finish.
// - BenchmarkDotNet is designed for console apps; its output is not returned to the HTTP client.
// - Running benchmarks in a web request can exhaust thread pool resources and impact server responsiveness.
// - Use this only for local development or testing, not in production environments.
// - For best practice, run benchmarks from a console app or test project, not via HTTP endpoints.

public class QueryBenchmark : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this, "[Play]").MapGet(GetAllTodoItemsAsync);
    }

    private static async Task<IResult> GetAllTodoItemsAsync(ApplicationDbContext db)
    {
        //Debugging
        // var benchmarkService = new BenchmarkService(db);
        // var p = benchmarkService.GetTodoLists_Optimized();

        //Comment me after first execution, please.
        // IWillPopulateData();

        // await Task.Run(() => BenchmarkRunner.Run<BenchmarkService>());
        BenchmarkRunner.Run<BenchmarkService>();
        return Results.Ok("Benchmark completed");
    }
}
