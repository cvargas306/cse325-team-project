namespace MyFavesApp.Data;

public class UserWatchlist
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public List<WatchlistMovie> Movies { get; set; } = [];
}