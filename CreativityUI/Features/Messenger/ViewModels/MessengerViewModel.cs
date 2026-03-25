using System.Collections.ObjectModel;
using API.Contracts.Chats;
using CreativityUI.Features.Auth.ViewModels;
using CreativityUI.Features.Messenger.Services;
using CreativityUI.Services.Api;

namespace CreativityUI.Features.Messenger.ViewModels;

public sealed class MessengerViewModel : ViewModelBase
{
    private const short GroupChatType = 2;

    private readonly CreativityApiClient _apiClient;
    private readonly ILastChatStore _lastChatStore;
    private ChatSummaryResponse? _selectedChat;
    private bool _isBusy;
    private bool _isSendingMessage;
    private string _statusMessage = "Загрузка чатов...";
    private string _draftMessage = string.Empty;

    public MessengerViewModel(CreativityApiClient apiClient, ILastChatStore lastChatStore)
    {
        _apiClient = apiClient;
        _lastChatStore = lastChatStore;
        Chats = [];
        Messages = [];
        CreateChatCommand = new Command(async () => await CreateChatAsync(), () => !IsBusy && !IsSendingMessage);
        SendMessageCommand = new Command(async () => await SendMessageAsync(), CanSendMessage);
    }

    public ObservableCollection<ChatSummaryResponse> Chats { get; }

    public ObservableCollection<MessageResponse> Messages { get; }

    public Command CreateChatCommand { get; }
    public Command SendMessageCommand { get; }

    public ChatSummaryResponse? SelectedChat
    {
        get => _selectedChat;
        set
        {
            if (SetProperty(ref _selectedChat, value))
            {
                OnPropertyChanged(nameof(SelectedChatTitle));
                UpdateCommandState();
                _ = SelectChatAsync(value);
            }
        }
    }

    public string DraftMessage
    {
        get => _draftMessage;
        set
        {
            if (SetProperty(ref _draftMessage, value))
            {
                UpdateCommandState();
            }
        }
    }

    public string SelectedChatTitle =>
        SelectedChat is null
            ? "Выберите чат"
            : string.IsNullOrWhiteSpace(SelectedChat.Title) ? $"Chat #{SelectedChat.Id}" : SelectedChat.Title;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                UpdateCommandState();
            }
        }
    }

    public bool IsSendingMessage
    {
        get => _isSendingMessage;
        private set
        {
            if (SetProperty(ref _isSendingMessage, value))
            {
                UpdateCommandState();
            }
        }
    }

    public async Task InitializeAsync()
    {
        await LoadChatsAsync();
    }

    public async Task LoadChatsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка чатов...";

            var response = await _apiClient.GetChatsAsync();
            Chats.Clear();
            foreach (var chat in response?.Chats ?? [])
            {
                Chats.Add(chat);
            }

            if (Chats.Count == 0)
            {
                Messages.Clear();
                SelectedChat = null;
                await _lastChatStore.ClearLastSelectedChatIdAsync();
                StatusMessage = "Чатов пока нет. Создайте новый чат.";
                return;
            }

            await RestoreSelectedChatAsync();
            StatusMessage = $"Чатов: {Chats.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки чатов: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RestoreSelectedChatAsync()
    {
        var savedChatId = await _lastChatStore.GetLastSelectedChatIdAsync();
        if (savedChatId is null)
        {
            SelectedChat = Chats[0];
            return;
        }

        SelectedChat = Chats.FirstOrDefault(x => x.Id == savedChatId.Value) ?? Chats[0];
    }

    private async Task SelectChatAsync(ChatSummaryResponse? chat)
    {
        if (chat is null)
        {
            Messages.Clear();
            UpdateCommandState();
            return;
        }

        try
        {
            await _lastChatStore.SaveLastSelectedChatIdAsync(chat.Id);
            var response = await _apiClient.GetChatMessagesAsync(chat.Id);
            Messages.Clear();
            foreach (var message in response?.Messages ?? [])
            {
                Messages.Add(message);
            }

            StatusMessage = Messages.Count == 0
                ? "В этом чате пока нет сообщений."
                : $"Сообщений: {Messages.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки сообщений: {ex.Message}";
        }
        finally
        {
            UpdateCommandState();
        }
    }

    private async Task CreateChatAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            StatusMessage = "Не удалось открыть форму создания чата.";
            return;
        }

        var title = await page.DisplayPromptAsync("Новый чат", "Введите название чата:", "Создать", "Отмена");
        if (title is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            StatusMessage = "Название чата не может быть пустым.";
            return;
        }

        var description = await page.DisplayPromptAsync("Новый чат", "Введите описание (опционально):", "Далее", "Пропустить");
        var request = new CreateChatRequest(
            ChatType: GroupChatType,
            Title: title.Trim(),
            Description: string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ParticipantUserIds: null);

        try
        {
            IsBusy = true;
            var createdChat = await _apiClient.CreateChatAsync(request);
            await LoadChatsAsync();

            if (createdChat?.Chat is not null)
            {
                SelectedChat = Chats.FirstOrDefault(x => x.Id == createdChat.Chat.Id) ?? SelectedChat;
            }

            StatusMessage = "Чат успешно создан.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка создания чата: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSendMessage()
    {
        return !IsBusy
            && !IsSendingMessage
            && SelectedChat is not null
            && !string.IsNullOrWhiteSpace(DraftMessage);
    }

    private async Task SendMessageAsync()
    {
        if (SelectedChat is null)
        {
            StatusMessage = "Сначала выберите чат.";
            return;
        }

        var text = DraftMessage?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            StatusMessage = "Введите текст сообщения.";
            return;
        }

        try
        {
            IsSendingMessage = true;
            var request = new SendMessageRequest(Text: text);
            var sentMessage = await _apiClient.SendMessageAsync(SelectedChat.Id, request);

            if (sentMessage is not null)
            {
                Messages.Add(sentMessage);
            }
            else
            {
                await SelectChatAsync(SelectedChat);
            }

            DraftMessage = string.Empty;
            StatusMessage = "Сообщение отправлено.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка отправки: {ex.Message}";
        }
        finally
        {
            IsSendingMessage = false;
        }
    }

    private void UpdateCommandState()
    {
        CreateChatCommand.ChangeCanExecute();
        SendMessageCommand.ChangeCanExecute();
    }
}
