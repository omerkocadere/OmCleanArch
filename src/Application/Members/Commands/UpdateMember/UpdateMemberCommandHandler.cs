using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(
    IApplicationDbContext context,
    ICurrentUser userContext,
    IIdentityService identityService
) : ICommandHandler<UpdateMemberCommand>
{
    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        // Validate user context
        if (userContext.UserId is null)
        {
            return Result.Failure(MemberErrors.NotFound);
        }

        var userId = userContext.UserId.Value; // Extract the Guid value

        // Start atomic transaction
        using var transaction = await context.BeginTransactionAsync(cancellationToken);

        // Load member with tracking for updates
        Member? member = await context.Members.SingleOrDefaultAsync(m => m.Id == userId, cancellationToken);

        if (member is null)
        {
            return Result.Failure(MemberErrors.NotFound);
        }

        // Update member properties if provided
        member.DisplayName = request.DisplayName ?? member.DisplayName;
        member.Description = request.Description ?? member.Description;
        member.City = request.City ?? member.City;
        member.Country = request.Country ?? member.Country;

        // Update ApplicationUser properties through IIdentityService if needed
        if (request.DisplayName != null)
        {
            var updateResult = await identityService.UpdateUserAsync(
                userId, // Pass Guid directly
                displayName: request.DisplayName
            );

            if (!updateResult.IsSuccess)
            {
                return updateResult;
            }
        }

        // Save member changes
        context.Members.Update(member);
        await context.SaveChangesAsync(cancellationToken);

        // Commit transaction - all operations successful
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
