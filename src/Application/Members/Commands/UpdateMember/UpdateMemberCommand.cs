using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Domain.Common;
using CleanArch.Domain.Members;

namespace CleanArch.Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand(string? DisplayName, string? Description, string? City, string? Country) : ICommand;

public class UpdateMemberCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<UpdateMemberCommand>
{
    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        Member? member = await context
            .Members.Include(m => m.User)
            .SingleOrDefaultAsync(m => m.Id == userContext.UserId, cancellationToken);

        if (member is null)
        {
            return Result.Failure(MemberErrors.NotFound);
        }

        // Update member properties if provided
        member.DisplayName = request.DisplayName ?? member.DisplayName;
        member.Description = request.Description ?? member.Description;
        member.City = request.City ?? member.City;
        member.Country = request.Country ?? member.Country;

        member.User.DisplayName = request.DisplayName ?? member.User.DisplayName;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
