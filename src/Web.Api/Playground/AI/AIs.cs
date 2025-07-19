using CleanArch.Application.Common.Models;
using CleanArch.Web.Api.Extensions;
using Microsoft.Extensions.AI;

namespace CleanArch.Web.Api.Playground.AI;

public class AIs : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this, "[Play]").MapGet(ChatMessage, "chat-message");
    }

    public async Task<IResult> ChatMessage(IChatClient chatClient)
    {
        throw new ArgumentNullException(nameof(chatClient));
        var message = new ChatMessage(ChatRole.User, "What is .Net?");

        var response = await chatClient.GetResponseAsync(message);

        var result = Result.Create(response);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
