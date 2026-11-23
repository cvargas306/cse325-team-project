namespace MyFavesApp.Data;

public class SavedMovie
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public string ImdbId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Year { get; set; } = "";
    public string? PosterUrl { get; set; }

    public string Status { get; set; } = "Want to Watch";   // or "Watched"
    public int Rating { get; set; }                         // 0-5
    public string? Note { get; set; }

    public DateTime AddedUtc { get; set; } = DateTime.UtcNow;
}