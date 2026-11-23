using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyFavesApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<SavedMovie> SavedMovies => Set<SavedMovie>();
    public DbSet<UserWatchlist> UserWatchlists => Set<UserWatchlist>(); // Represents the collection of user watchlists in the database
    public DbSet<WatchlistMovie> WatchlistMovies => Set<WatchlistMovie>(); // Represents the collection of watchlist-movie relationships in the database

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);  // Call the base method to ensure Identity tables are configured

        builder.Entity<SavedMovie>()    // Configure the SavedMovie entity
            .HasIndex(m => new { m.UserId, m.ImdbId }) // Create a unique index on UserId and ImdbId to prevent duplicate entries for the same user and movie
            .IsUnique(); // Ensure that each user can only save a specific movie once
        builder.Entity<WatchlistMovie>() // Configure the WatchlistMovie entity
            .HasKey(wm => new { wm.UserWatchlistId, wm.SavedMovieId }); // Define a composite primary key using UserWatchlistId and SavedMovieId to uniquely identify each relationship between a watchlist and a saved movie
    }
}
