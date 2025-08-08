using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Comments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Web.Api.Controllers;

/// <summary>
/// Demo controller to test soft delete functionality.
/// This demonstrates the modern EF Core soft delete implementation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SoftDeleteDemoController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SoftDeleteDemoController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new comment for testing soft delete.
    /// </summary>
    [HttpPost("comments")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        var comment = new Comment
        {
            Content = request.Content,
            AuthorName = request.AuthorName,
            Email = request.Email,
            PostId = request.PostId,
            Status = CommentStatus.Approved,
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(new { comment.Id, Message = "Comment created successfully" });
    }

    /// <summary>
    /// Gets all comments (only non-deleted ones due to global query filter).
    /// </summary>
    [HttpGet("comments")]
    public async Task<IActionResult> GetComments()
    {
        var comments = await _context
            .Comments.Select(c => new
            {
                c.Id,
                c.Content,
                c.AuthorName,
                c.Email,
                c.PostId,
                c.Status,
                c.Created,
                c.IsDeleted,
                c.DeletedAt,
                c.DeletedBy,
            })
            .ToListAsync();

        return Ok(new { Comments = comments, Count = comments.Count });
    }

    /// <summary>
    /// Gets all comments including soft-deleted ones using IgnoreQueryFilters().
    /// </summary>
    [HttpGet("comments/all")]
    public async Task<IActionResult> GetAllCommentsIncludingDeleted()
    {
        var comments = await _context
            .Comments.IgnoreQueryFilters() // This bypasses the soft delete filter
            .Select(c => new
            {
                c.Id,
                c.Content,
                c.AuthorName,
                c.Email,
                c.PostId,
                c.Status,
                c.Created,
                c.IsDeleted,
                c.DeletedAt,
                c.DeletedBy,
            })
            .ToListAsync();

        return Ok(new { Comments = comments, Count = comments.Count });
    }

    /// <summary>
    /// Soft deletes a comment using EF Core's Remove method.
    /// The interceptor will convert this to a soft delete automatically.
    /// </summary>
    [HttpDelete("comments/{id}")]
    public async Task<IActionResult> SoftDeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        // This will trigger soft delete via the interceptor
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Comment soft deleted successfully" });
    }

    /// <summary>
    /// Manually soft deletes a comment using the SoftDelete() method.
    /// </summary>
    [HttpPatch("comments/{id}/soft-delete")]
    public async Task<IActionResult> ManualSoftDeleteComment(int id, [FromBody] SoftDeleteRequest request)
    {
        var comment = await _context
            .Comments.IgnoreQueryFilters() // Need this to find already soft-deleted items
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound();
        }

        if (comment.IsDeleted)
        {
            return BadRequest("Comment is already soft deleted");
        }

        // Manual soft delete using the entity method
        comment.SoftDelete(request.DeletedBy);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Comment manually soft deleted successfully" });
    }

    /// <summary>
    /// Restores a soft-deleted comment.
    /// </summary>
    [HttpPatch("comments/{id}/restore")]
    public async Task<IActionResult> RestoreComment(int id)
    {
        var comment = await _context
            .Comments.IgnoreQueryFilters() // Need this to find soft-deleted items
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound();
        }

        if (!comment.IsDeleted)
        {
            return BadRequest("Comment is not soft deleted");
        }

        // Restore the soft-deleted comment
        comment.Restore();
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Comment restored successfully" });
    }

    /// <summary>
    /// Gets soft delete statistics.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetSoftDeleteStats()
    {
        var activeCount = await _context.Comments.CountAsync();
        var deletedCount = await _context.Comments.IgnoreQueryFilters().Where(c => c.IsDeleted).CountAsync();
        var totalCount = await _context.Comments.IgnoreQueryFilters().CountAsync();

        return Ok(
            new
            {
                ActiveComments = activeCount,
                DeletedComments = deletedCount,
                TotalComments = totalCount,
                DeletionRate = totalCount > 0 ? Math.Round((double)deletedCount / totalCount * 100, 2) : 0,
            }
        );
    }
}

public record CreateCommentRequest(string Content, string AuthorName, string Email, int PostId);

public record SoftDeleteRequest(Guid? DeletedBy);
