using Microsoft.Maui.Storage;

namespace CreativityUI.Features.Messenger.Services;

public sealed class PreferencesLastChatStore : ILastChatStore
{
    private const string LastSelectedChatIdKey = "messenger.last_selected_chat_id";

    public Task<long?> GetLastSelectedChatIdAsync()
    {
        var value = Preferences.Default.Get(LastSelectedChatIdKey, string.Empty);
        if (long.TryParse(value, out var chatId) && chatId > 0)
        {
            return Task.FromResult<long?>(chatId);
        }

        return Task.FromResult<long?>(null);
    }

    public Task SaveLastSelectedChatIdAsync(long chatId)
    {
        if (chatId > 0)
        {
            Preferences.Default.Set(LastSelectedChatIdKey, chatId.ToString());
        }

        return Task.CompletedTask;
    }

    public Task ClearLastSelectedChatIdAsync()
    {
        Preferences.Default.Remove(LastSelectedChatIdKey);
        return Task.CompletedTask;
    }
}
