using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Web.Api.Services;

public class DummyApiClient(HttpClient httpClient)
{
    public Task<Result<List<TodoItem>>> GetToDoItemsAsync() =>
        ExecuteApiCallAsync(async () =>
        {
            var items = await httpClient.GetFromJsonAsync<List<TodoItem>>("/todoitems");
            return items ?? [];
        });

    public Task<Result<string>> GetHelloMessageAsync() =>
        ExecuteApiCallAsync(() => httpClient.GetStringAsync("/hellofromdummy"));

    private static async Task<Result<T>> ExecuteApiCallAsync<T>(Func<Task<T>> apiCall)
    {
        try
        {
            var result = await apiCall();
            return Result.Success(result);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<T>(DummyApiErrors.ConnectionFailed);
        }
        catch (TaskCanceledException)
        {
            return Result.Failure<T>(DummyApiErrors.Timeout);
        }
        catch (Exception)
        {
            return Result.Failure<T>(DummyApiErrors.UnexpectedError);
        }
    }

    public static class DummyApiErrors
    {
        public static readonly Error ConnectionFailed = Error.Problem(
            "DummyApi.ConnectionFailed",
            "Unable to connect to Dummy API"
        );

        public static readonly Error Timeout = Error.Problem("DummyApi.Timeout", "Request to Dummy API timed out");

        public static readonly Error UnexpectedError = Error.Problem(
            "DummyApi.UnexpectedError",
            "An unexpected error occurred while calling Dummy API"
        );
    }
}
