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

    public async Task SignOutAsync()
    {
        SecureStorage.Default.Remove("sports_access_token");
        SecureStorage.Default.Remove("sports_active_club");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
