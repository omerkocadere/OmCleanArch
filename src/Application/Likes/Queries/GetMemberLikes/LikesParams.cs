using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Likes.Queries.GetMemberLikes;

public class LikesParams : PagingParams
{
    public Guid? MemberId { get; set; }
    public string Predicate { get; set; } = "liked"; // "liked", "likedBy", "mutual"
}
