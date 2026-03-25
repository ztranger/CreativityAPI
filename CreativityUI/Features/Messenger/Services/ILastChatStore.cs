namespace CreativityUI.Features.Messenger.Services;

public interface ILastChatStore
{
    Task<long?> GetLastSelectedChatIdAsync();
    Task SaveLastSelectedChatIdAsync(long chatId);
    Task ClearLastSelectedChatIdAsync();
}
