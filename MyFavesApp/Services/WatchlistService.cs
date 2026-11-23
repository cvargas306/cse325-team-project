using Microsoft.EntityFrameworkCore;
using MyFavesApp.Data;

namespace MyFavesApp.Services;

public record WatchlistSummary(int Id, string Name, int MovieCount);
public class WatchlistService(ApplicationDbContext db)
{
    public Task<List<SavedMovie>> GetMoviesAsync(string userId) => // Retrieve the list of saved movies for a specific user, ordered by the date they were added (most recent first)
        db.SavedMovies
          .Where(m => m.UserId == userId)
          .OrderByDescending(m => m.AddedUtc)
          .ToListAsync();

    public Task<bool> IsSavedAsync(string userId, string imdbId) =>
        db.SavedMovies.AnyAsync(m => m.UserId == userId && m.ImdbId == imdbId); // Check if a movie is already saved for the user

    public async Task<bool> AddAsync(string userId, SavedMovie movie)
    {
        if (await IsSavedAsync(userId, movie.ImdbId)) // Check if the movie is already saved for the user
            return false;   // already saved

        movie.UserId = userId;
        db.SavedMovies.Add(movie);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task RemoveAsync(string userId, string imdbId)
    {
        var movie = await db.SavedMovies    // Find the saved movie for the user by IMDb ID
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ImdbId == imdbId); // If the movie is not found, return without doing anything
        if (movie is null) return;

        db.SavedMovies.Remove(movie); // Remove the movie from the database
        await db.SaveChangesAsync();    // Save the changes to the database after removing the movie
    }
    public Task<List<WatchlistSummary>> GetListsAsync(string userId) =>
    db.UserWatchlists
      .Where(w => w.UserId == userId)
      .OrderBy(w => w.Name)
      .Select(w => new WatchlistSummary(w.Id, w.Name, w.Movies.Count))
      .ToListAsync();

    public async Task<bool> CreateListAsync(string userId, string name) // Create a new watchlist for the user with the specified name
    {
        name = name.Trim();
        if (name.Length == 0) return false;
        if (await db.UserWatchlists.AnyAsync(w => w.UserId == userId && w.Name == name))
            return false;   // already have a list with that name

        db.UserWatchlists.Add(new UserWatchlist { UserId = userId, Name = name });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task DeleteListAsync(string userId, int listId) // Delete a watchlist for the user by its ID
    {
        var list = await db.UserWatchlists
            .FirstOrDefaultAsync(w => w.Id == listId && w.UserId == userId);
        if (list is null) return;

        db.UserWatchlists.Remove(list);   // your saved movies stay saved
        await db.SaveChangesAsync();
    }
    public async Task<HashSet<int>> GetListIdsForMovieAsync(string userId, string imdbId)
    {
        var ids = await db.WatchlistMovies
            .Where(x => x.UserWatchlist!.UserId == userId && x.SavedMovie!.ImdbId == imdbId)
            .Select(x => x.UserWatchlistId)
            .ToListAsync();
        return ids.ToHashSet();
    }

    // Puts the movie on the list, or takes it off if it's already there.
    // Returns true if the movie is on the list afterwards.
    public async Task<bool> ToggleInListAsync(string userId, int listId, SavedMovie movie)
    {
        var list = await db.UserWatchlists
            .FirstOrDefaultAsync(w => w.Id == listId && w.UserId == userId);
        if (list is null) return false;

        var saved = await db.SavedMovies
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ImdbId == movie.ImdbId);
        if (saved is null)
        {
            movie.UserId = userId;
            db.SavedMovies.Add(movie);
            await db.SaveChangesAsync();
            saved = movie;
        }

        var link = await db.WatchlistMovies
            .FirstOrDefaultAsync(x => x.UserWatchlistId == listId && x.SavedMovieId == saved.Id);

        if (link is null)
        {
            db.WatchlistMovies.Add(new WatchlistMovie { UserWatchlistId = listId, SavedMovieId = saved.Id });
            await db.SaveChangesAsync();
            return true;
        }

        db.WatchlistMovies.Remove(link);
        await db.SaveChangesAsync();
        return false;
    }
    public Task<UserWatchlist?> GetListAsync(string userId, int listId) =>
    db.UserWatchlists.FirstOrDefaultAsync(w => w.Id == listId && w.UserId == userId);

    public Task<List<SavedMovie>> GetListMoviesAsync(string userId, int listId) =>
        db.WatchlistMovies
          .Where(x => x.UserWatchlistId == listId && x.UserWatchlist!.UserId == userId)
          .Select(x => x.SavedMovie!)
          .ToListAsync();

    public async Task RemoveFromListAsync(string userId, int listId, int savedMovieId)
    {
        var link = await db.WatchlistMovies.FirstOrDefaultAsync(x =>
            x.UserWatchlistId == listId &&
            x.SavedMovieId == savedMovieId &&
            x.UserWatchlist!.UserId == userId);
        if (link is null) return;

        db.WatchlistMovies.Remove(link);
        await db.SaveChangesAsync();
    }
}