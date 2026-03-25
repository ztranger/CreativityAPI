using System.Net.Http.Json;
using API.Contracts.Auth;

namespace CreativityUI.Features.Auth.Services;

public sealed class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("auth/register", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthRegisterResponse>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("Register response is empty.");
        }

        return result;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("auth/login", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("Login response is empty.");
        }

        return result;
    }
}
