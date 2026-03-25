using System.Net.Http.Json;
using API.Contracts.Chats;

namespace CreativityUI.Services.Api;

public sealed class CreativityApiClient
{
    private readonly HttpClient _httpClient;

    public CreativityApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChatsListResponse?> GetChatsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<ChatsListResponse>("chats", cancellationToken);
    }

    public async Task<ChatDetailsResponse?> CreateChatAsync(CreateChatRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("chats", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChatDetailsResponse>(cancellationToken: cancellationToken);
    }
}
