using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [new IdentityResources.OpenId(), new IdentityResources.Profile()];

    public static IEnumerable<ApiScope> ApiScopes => [new ApiScope("auctionApp", "Auction App Full Access")];

    public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                AllowedScopes = { "openid", "profile", "auctionApp" },
                RedirectUris = { GetPostmanRedirectUri() },
                ClientSecrets = [new Secret("NotASecret".Sha256())],
                AllowedGrantTypes = { GrantType.ResourceOwnerPassword },
            },
        ];

    private static string GetPostmanRedirectUri()
    {
        // Ideally, fetch from configuration/environment; fallback to default if not set
        var uri = Environment.GetEnvironmentVariable("POSTMAN_REDIRECT_URI");
        return !string.IsNullOrWhiteSpace(uri) ? uri : "https://www.getpostman.com/oauth2/callback"; // fallback for local/dev
    }
}
