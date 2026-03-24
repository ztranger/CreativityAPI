using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.Tests;

[TestClass]
public class ChatsApiTests
{
    [TestMethod]
    public async Task ChatsEndpointsRequireAuthorization()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.API_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        await resourceNotificationService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/chats");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ChatManagementAndMessagesFlowWorks()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.API_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        await resourceNotificationService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        var httpClient = app.CreateHttpClient("apiservice");

        var first = await RegisterUserAsync(httpClient, "+79000000001", "Owner", "owner");
        var second = await RegisterUserAsync(httpClient, "+79000000002", "Member", "member");
        var third = await RegisterUserAsync(httpClient, "+79000000003", "Guest", "guest");

        var createChatResponse = await SendAuthorizedAsync(
            httpClient,
            first.Token,
            HttpMethod.Post,
            "/chats",
            new
            {
                chatType = 2,
                title = "MVP Chat",
                description = "Chat created in tests",
                participantUserIds = new[] { second.UserId }
            });
        Assert.AreEqual(HttpStatusCode.Created, createChatResponse.StatusCode);

        using var createdDoc = JsonDocument.Parse(await createChatResponse.Content.ReadAsStringAsync());
        var chatId = createdDoc.RootElement.GetProperty("chat").GetProperty("id").GetInt64();

        var chatsResponse = await SendAuthorizedAsync(httpClient, first.Token, HttpMethod.Get, "/chats");
        Assert.AreEqual(HttpStatusCode.OK, chatsResponse.StatusCode);

        var chatDetailsResponse = await SendAuthorizedAsync(httpClient, first.Token, HttpMethod.Get, $"/chats/{chatId}");
        Assert.AreEqual(HttpStatusCode.OK, chatDetailsResponse.StatusCode);

        var addParticipantResponse = await SendAuthorizedAsync(
            httpClient,
            first.Token,
            HttpMethod.Post,
            $"/chats/{chatId}/participants",
            new { userId = third.UserId, role = 1 });
        Assert.AreEqual(HttpStatusCode.OK, addParticipantResponse.StatusCode);

        var sendMessageResponse = await SendAuthorizedAsync(
            httpClient,
            first.Token,
            HttpMethod.Post,
            $"/chats/{chatId}/messages",
            new { text = "hello from tests" });
        Assert.AreEqual(HttpStatusCode.Created, sendMessageResponse.StatusCode);

        var listMessagesResponse = await SendAuthorizedAsync(httpClient, second.Token, HttpMethod.Get, $"/chats/{chatId}/messages");
        Assert.AreEqual(HttpStatusCode.OK, listMessagesResponse.StatusCode);
        using (var messagesDoc = JsonDocument.Parse(await listMessagesResponse.Content.ReadAsStringAsync()))
        {
            var messages = messagesDoc.RootElement.GetProperty("messages");
            Assert.IsTrue(messages.GetArrayLength() >= 1);
            Assert.AreEqual("hello from tests", messages[messages.GetArrayLength() - 1].GetProperty("text").GetString());
        }

        var removeParticipantResponse = await SendAuthorizedAsync(
            httpClient,
            first.Token,
            HttpMethod.Delete,
            $"/chats/{chatId}/participants/{third.UserId}");
        Assert.AreEqual(HttpStatusCode.NoContent, removeParticipantResponse.StatusCode);

        var removedUserAccess = await SendAuthorizedAsync(httpClient, third.Token, HttpMethod.Get, $"/chats/{chatId}");
        Assert.AreEqual(HttpStatusCode.Forbidden, removedUserAccess.StatusCode);

        var updateChatResponse = await SendAuthorizedAsync(
            httpClient,
            first.Token,
            HttpMethod.Patch,
            $"/chats/{chatId}",
            new { title = "MVP Chat Updated", description = "updated by owner" });
        Assert.AreEqual(HttpStatusCode.OK, updateChatResponse.StatusCode);
    }

    private static async Task<(int UserId, string Token)> RegisterUserAsync(
        HttpClient httpClient,
        string phone,
        string displayName,
        string username)
    {
        var response = await httpClient.PostAsJsonAsync("/auth/register", new
        {
            phone,
            displayName,
            username
        });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var userId = json.RootElement.GetProperty("user").GetProperty("id").GetInt32();
        var token = json.RootElement.GetProperty("token").GetString();
        Assert.IsFalse(string.IsNullOrWhiteSpace(token));
        return (userId, token!);
    }

    private static async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpClient httpClient,
        string token,
        HttpMethod method,
        string url,
        object? payload = null)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        return await httpClient.SendAsync(request);
    }
}
