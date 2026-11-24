using System.Text.Json.Serialization;

namespace MyFavesApp.Services;

public class OmdbService(HttpClient httpClient, IConfiguration config)
{
    private readonly string _apiKey = config["Omdb:ApiKey"] ?? "";

    public async Task<List<OmdbSearchItem>> SearchMoviesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var url = $"https://omdbapi.com/?apikey={_apiKey}&type=movie&s={Uri.EscapeDataString(query.Trim())}";
        var response = await httpClient.GetFromJsonAsync<OmdbSearchResponse>(url);
        if (response?.Response == "True" && response.Search is not null)
            return response.Search;

        return [];
    }
    public async Task<OmdbMovieDetail?> GetMovieDetailsAsync(string imdbId)
    {
        if (string.IsNullOrWhiteSpace(imdbId)) return null;

        var url = $"https://omdbapi.com/?apikey={_apiKey}&i={Uri.EscapeDataString(imdbId)}&plot=full";
        var detail = await httpClient.GetFromJsonAsync<OmdbMovieDetail>(url);

        return detail?.Response == "True" ? detail : null;
    }

    public async Task<List<WatchmodeSource>> GetUsStreamingSourcesAsync(string imdbId, string watchmodeApiKey)
    {
        if (string.IsNullOrWhiteSpace(imdbId) || string.IsNullOrWhiteSpace(watchmodeApiKey))
            return [];

        var searchUrl = $"https://api.watchmode.com/v1/search/?apiKey={watchmodeApiKey}&search_field=imdb_id&search_value={Uri.EscapeDataString(imdbId)}";
        var searchResult = await httpClient.GetFromJsonAsync<WatchmodeSearchResponse>(searchUrl);

        var titleId = searchResult?.TitleResults?.FirstOrDefault()?.Id;
        if (titleId is null) return [];

        var sourcesUrl = $"https://api.watchmode.com/v1/title/{titleId}/sources/?apiKey={watchmodeApiKey}";
        var sources = await httpClient.GetFromJsonAsync<List<WatchmodeSource>>(sourcesUrl) ?? [];

        return sources
            .Where(s => string.Equals(s.Region, "US", StringComparison.OrdinalIgnoreCase))
            .GroupBy(s => s.Name)
            .Select(g => g.First())
            .ToList();
    }
}

public class OmdbSearchResponse
{
    [JsonPropertyName("Search")]
    public List<OmdbSearchItem>? Search { get; set; }

    [JsonPropertyName("Response")]
    public string? Response { get; set; }
}

public class OmdbSearchItem
{
    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("Year")]
    public string Year { get; set; } = "";

    [JsonPropertyName("imdbID")]
    public string ImdbId { get; set; } = "";

    [JsonPropertyName("Poster")]
    public string Poster { get; set; } = "";

    public string SafePosterUrl =>
        !string.IsNullOrWhiteSpace(Poster) && Poster != "N/A"
            ? Poster
            : "images/posters/movie-poster-1.png"; // Fallback to your existing placeholder poster
}
public class OmdbMovieDetail
{
    [JsonPropertyName("Title")] public string Title { get; set; } = "";
    [JsonPropertyName("Year")] public string Year { get; set; } = "";
    [JsonPropertyName("Rated")] public string Rated { get; set; } = "";
    [JsonPropertyName("Released")] public string Released { get; set; } = "";
    [JsonPropertyName("Runtime")] public string Runtime { get; set; } = "";
    [JsonPropertyName("Genre")] public string Genre { get; set; } = "";
    [JsonPropertyName("Director")] public string Director { get; set; } = "";
    [JsonPropertyName("Actors")] public string Actors { get; set; } = "";
    [JsonPropertyName("Plot")] public string Plot { get; set; } = "";
    [JsonPropertyName("Language")] public string Language { get; set; } = "";
    [JsonPropertyName("Poster")] public string Poster { get; set; } = "";
    [JsonPropertyName("imdbID")] public string ImdbId { get; set; } = "";
    [JsonPropertyName("Response")] public string? Response { get; set; }

    public string SafePosterUrl =>
        !string.IsNullOrWhiteSpace(Poster) && Poster != "N/A"
            ? Poster
            : "images/posters/movie-poster-1.png";
}

public class WatchmodeSearchResponse
{
    [JsonPropertyName("title_results")]
    public List<WatchmodeTitleResult>? TitleResults { get; set; }
}

public class WatchmodeTitleResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class WatchmodeSource
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("region")] public string Region { get; set; } = "";
    [JsonPropertyName("web_url")] public string WebUrl { get; set; } = "";
    [JsonPropertyName("price")] public decimal? Price { get; set; }

    public string DisplayType => Type.ToLowerInvariant() switch
    {
        "sub" => "Subscription",
        "free" => "Free",
        "rent" => Price.HasValue ? $"Rent · ${Price:0.00}" : "Rent",
        "buy" => Price.HasValue ? $"Buy · ${Price:0.00}" : "Buy",
        _ => Type
    };

    public string BadgeText => Name.Length >= 2 ? Name[..2].ToUpperInvariant() : Name.ToUpperInvariant();
}
