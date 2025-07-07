namespace CleanArch.Web.Api.Controllers;

public class BookReview
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Reviewer { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
