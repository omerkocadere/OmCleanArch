using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Messages.Queries.GetMessages;

public class MessageParams : PagingParams
{
    public Guid? MemberId { get; set; }
    public string Container { get; set; } = "Inbox";
}
