namespace CleanArch.Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand(string? DisplayName, string? Description, string? City, string? Country) : ICommand;
