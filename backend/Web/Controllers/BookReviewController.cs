using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class BookReviewController(ILogger<BookReviewController> logger) : ControllerBase
{
    private static readonly string[] ReviewComments =
    [
        "Outstanding read",
        "Thought-provoking",
        "Well-written",
        "Engaging",
        "Mediocre",
        "Disappointing",
        "Inspirational",
        "Overrated",
        "Underrated",
        "A must-read",
    ];

    [HttpGet(Name = "GetBookReviews")]
    public IEnumerable<BookReview> Get()
    {
        logger.LogInformation("Getting book reviews");
        return
        [
            .. Enumerable
                .Range(1, 5)
                .Select(index => new BookReview
                {
                    Id = index,
                    Title = $"Book Title {index}",
                    Reviewer = $"Reviewer {index}",
                    Rating = Random.Shared.Next(1, 6),
                    Comment = ReviewComments[Random.Shared.Next(ReviewComments.Length)],
                }),
        ];
    }
}
