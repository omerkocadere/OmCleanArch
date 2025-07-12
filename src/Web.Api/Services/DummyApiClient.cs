using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;

namespace CleanArch.Web.Api.Services;

public class DummyApiClient(HttpClient httpClient)
{
    public async Task<Result<string>> GetHelloMessageAsync()
    {
        try
        {
            var content = await httpClient.GetStringAsync("/hellofromdummy");
            return Result.Success(content);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<string>(DummyApiErrors.ConnectionFailed);
        }
        catch (TaskCanceledException)
        {
            return Result.Failure<string>(DummyApiErrors.Timeout);
        }
        catch (Exception)
        {
            return Result.Failure<string>(DummyApiErrors.UnexpectedError);
        }
    }

    public static class DummyApiErrors
    {
        public static readonly Error RequestFailed = Error.Problem(
            "DummyApi.RequestFailed",
            "Failed to get response from Dummy API"
        );

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
