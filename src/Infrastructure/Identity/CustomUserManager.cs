using CleanArch.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Identity;

/// <summary>
/// Custom UserManager that adds domain events when users are created.
/// This ensures domain events are fired regardless of where UserManager.CreateAsync is called.
/// </summary>
public sealed class CustomUserManager : UserManager<ApplicationUser>
{
    public CustomUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<CustomUserManager> logger
    )
        : base(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger
        ) { }

    /// <summary>
    /// Overrides CreateAsync to add domain events after successful user creation.
    /// This method is called by both CreateAsync(user) and CreateAsync(user, password) overloads.
    /// </summary>
    public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
    {
        // Call the base CreateAsync method to create the user
        var result = await base.CreateAsync(user);

        if (result.Succeeded)
        {
            // Add domain event after successful user creation
            var userCreatedEvent = new UserCreatedDomainEvent(
                Guid.NewGuid(),
                user.Id,
                user.Email ?? user.UserName ?? "unknown", // userName parameter
                user.Email ?? "unknown" // email parameter
            );
            user.AddDomainEvent(userCreatedEvent);

            // Note: The domain event will be processed when SaveChanges is called
            // by the SaveChanges interceptor in the calling context
        }

        return result;
    }
}
