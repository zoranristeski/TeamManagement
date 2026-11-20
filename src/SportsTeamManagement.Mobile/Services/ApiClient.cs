using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SportsTeamManagement.Mobile.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SetTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await SecureStorage.Default.GetAsync("sports_access_token");
        _httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        await SetTokenAsync(cancellationToken);
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken cancellationToken = default)
    {
        await SetTokenAsync(cancellationToken);
        using var response = await _httpClient.PostAsJsonAsync(path, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync("sports_access_token", token);
        await SetTokenAsync();
    }

    public async Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { email, password }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The authentication response was invalid.");
        await SaveAuthAsync(auth);
        return auth;
    }

    public async Task<AuthResponse> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/auth/register",
            new { firstName, lastName, email, password, acceptTerms = true }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The authentication response was invalid.");
        await SaveAuthAsync(auth);
        return auth;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await SecureStorage.Default.GetAsync("sports_refresh_token");
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;
        using var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh", new { refreshToken }, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return;
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        if (auth is not null)
            await SaveAuthAsync(auth);
    }

    private async Task SaveAuthAsync(AuthResponse auth)
    {
        await SaveTokenAsync(auth.AccessToken);
        await SecureStorage.Default.SetAsync("sports_refresh_token", auth.RefreshToken);
    }

    public async Task SignOutAsync()
    {
        SecureStorage.Default.Remove("sports_access_token");
        SecureStorage.Default.Remove("sports_refresh_token");
        SecureStorage.Default.Remove("sports_active_club");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
