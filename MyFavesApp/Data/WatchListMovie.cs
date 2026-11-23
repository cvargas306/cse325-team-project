namespace MyFavesApp.Data;

public class WatchlistMovie //links a list to a saved movie, so one movie can be on several lists
{
    public int UserWatchlistId { get; set; }
    public UserWatchlist? UserWatchlist { get; set; }

    public int SavedMovieId { get; set; }
    public SavedMovie? SavedMovie { get; set; }
}