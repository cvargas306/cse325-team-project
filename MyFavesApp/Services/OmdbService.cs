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
        Console.WriteLine(url);
        var response = await httpClient.GetFromJsonAsync<OmdbSearchResponse>(url);
        if (response?.Response == "True" && response.Search is not null)
            return response.Search;

        return [];
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
