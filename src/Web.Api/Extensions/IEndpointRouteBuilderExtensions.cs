using System.Diagnostics.CodeAnalysis;

namespace CleanArch.Web.Api.Extensions;

public static class IEndpointRouteBuilderExtensions
{
    public static RouteHandlerBuilder MapGet(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern = ""
    )
    {
        EnsureNotAnonymous(handler);

        return builder.MapGet(pattern, handler).WithName(handler.Method.Name);
    }

    public static RouteHandlerBuilder MapPost(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern = ""
    )
    {
        EnsureNotAnonymous(handler);

        return builder.MapPost(pattern, handler).WithName(handler.Method.Name);
    }

    public static RouteHandlerBuilder MapPut(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern
    )
    {
        EnsureNotAnonymous(handler);

        return builder.MapPut(pattern, handler).WithName(handler.Method.Name);
    }

    public static RouteHandlerBuilder MapDelete(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern
    )
    {
        EnsureNotAnonymous(handler);

        return builder.MapDelete(pattern, handler).WithName(handler.Method.Name);
    }

    private static void EnsureNotAnonymous(Delegate handler)
    {
        if (string.IsNullOrWhiteSpace(handler.Method.Name) || handler.Method.Name.Contains("lambda"))
            throw new ArgumentException(
                "Anonymous methods (lambdas) are not supported. Use a named method.",
                nameof(handler)
            );
    }
}
