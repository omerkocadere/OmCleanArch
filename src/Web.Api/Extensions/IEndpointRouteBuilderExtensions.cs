using System.Diagnostics.CodeAnalysis;

namespace CleanArch.Web.Api.Extensions;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapGet(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern = ""
    )
    {
        EnsureNotAnonymous(handler);

        builder.MapGet(pattern, handler).WithName(handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapPost(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern = ""
    )
    {
        EnsureNotAnonymous(handler);

        builder.MapPost(pattern, handler).WithName(handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapPut(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern
    )
    {
        EnsureNotAnonymous(handler);

        builder.MapPut(pattern, handler).WithName(handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapDelete(
        this IEndpointRouteBuilder builder,
        Delegate handler,
        [StringSyntax("Route")] string pattern
    )
    {
        EnsureNotAnonymous(handler);

        builder.MapDelete(pattern, handler).WithName(handler.Method.Name);

        return builder;
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
