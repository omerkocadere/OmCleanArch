using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Application.TodoLists.GetTodos;
using CleanArch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Web.Api.EndpointsPlay.QueryBenchmarks
{
    [MemoryDiagnoser]
    [Config(typeof(Config))]
    public class BenchmarkService(ApplicationDbContext dbContext)
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);

                AddJob(
                    Job.Default.WithMinWarmupCount(1)
                        .WithMaxWarmupCount(3)
                        .WithMinIterationCount(1)
                        .WithMaxIterationCount(3)
                );
            }
        }

        [Benchmark(Baseline = true)]
        public List<TodoListDto> GetTodoLists_WithN1Problem()
        {
            // Simulate a small delay for demonstration purposes
            Thread.Sleep(2000);

            // N+1 query problem: Each TodoList triggers a separate query for its items
            var todoLists = dbContext.TodoLists.ToList();
            var result = new List<TodoListDto>();

            foreach (var list in todoLists)
            {
                // For each list, a separate query is executed (N+1 problem)
                var items = dbContext
                    .TodoItems.Where(i => i.ListId == list.Id)
                    .Select(i => new TodoItemDto
                    {
                        Id = i.Id,
                        ListId = i.ListId,
                        Title = i.Title,
                        Note = i.Note,
                        Priority = (int)i.Priority,
                        Done = i.Done,
                    })
                    .ToList();

                result.Add(
                    new TodoListDto
                    {
                        Id = list.Id,
                        Title = list.Title ?? string.Empty,
                        Colour = list.Colour?.Code,
                        Items = items,
                    }
                );
            }
            return result;
        }

        [Benchmark]
        public List<TodoListDto> GetTodoLists_Optimized()
        {
            // Optimized version using Include to fetch all data in a single query
            return dbContext
                .TodoLists.Include(l => l.Items)
                .Select(l => new TodoListDto
                {
                    Id = l.Id,
                    Title = l.Title ?? string.Empty,
                    Colour = l.Colour != null ? l.Colour.Code : null,
                    Items = l
                        .Items.Select(i => new TodoItemDto
                        {
                            Id = i.Id,
                            ListId = i.ListId,
                            Title = i.Title,
                            Note = i.Note,
                            Priority = (int)i.Priority,
                            Done = i.Done,
                        })
                        .ToList(),
                })
                .ToList();
        }
    }
}
