using Microsoft.Extensions.AI;

namespace CleanArch.Web.Api.Playground.Services;

public static class PlayServices
{
    public static void AddPlayServices(this IServiceCollection services, IConfiguration configuration)
    {
        var openAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(openAIApiKey))
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");

        services.AddChatClient(new OpenAI.Chat.ChatClient("gpt-4o-mini", openAIApiKey).AsIChatClient());
    }
}
