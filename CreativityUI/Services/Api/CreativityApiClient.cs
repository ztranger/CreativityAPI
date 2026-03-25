using System.Net.Http.Json;
using System.Net.Http.Headers;
using API.Contracts.Chats;
using CreativityUI.Features.Auth.Services;

namespace CreativityUI.Services.Api;

public sealed class CreativityApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthTokenStore _authTokenStore;

    public CreativityApiClient(HttpClient httpClient, IAuthTokenStore authTokenStore)
    {
        _httpClient = httpClient;
        _authTokenStore = authTokenStore;
    }

    public async Task<ChatsListResponse?> GetChatsAsync(CancellationToken cancellationToken = default)
    {
        await AttachBearerTokenAsync();
        return await _httpClient.GetFromJsonAsync<ChatsListResponse>("chats", cancellationToken);
    }

    public async Task<ChatDetailsResponse?> CreateChatAsync(CreateChatRequest request, CancellationToken cancellationToken = default)
    {
        await AttachBearerTokenAsync();
        using var response = await _httpClient.PostAsJsonAsync("chats", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChatDetailsResponse>(cancellationToken: cancellationToken);
    }

    private async Task AttachBearerTokenAsync()
    {
        var token = await _authTokenStore.GetTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }
}
