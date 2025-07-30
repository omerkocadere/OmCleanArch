using CleanArch.Application.Products.GetProductById;
using CleanArch.Application.Products.GetProducts;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Products : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProducts);
        groupBuilder.MapGet(GetProductById, "{id}");
    }

    public async Task<IResult> GetProducts(ISender sender)
    {
        var result = await sender.Send(new GetProductsQuery());
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetProductById(ISender sender, int id)
    {
        var result = await sender.Send(new GetProductByIdQuery(id));
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
