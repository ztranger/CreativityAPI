using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using API.Contracts.Chats;
using CreativityUI.Features.Auth.Services;
using CreativityUI.Features.Auth.ViewModels;
using CreativityUI.Features.Messenger.Services;
using CreativityUI.Services.Api;

namespace CreativityUI.Features.Messenger.ViewModels;

public sealed class MessengerViewModel : ViewModelBase
{
    private const short GroupChatType = 2;

    private readonly CreativityApiClient _apiClient;
    private readonly ILastChatStore _lastChatStore;
    private readonly IAuthTokenStore _authTokenStore;
    private readonly List<ChatSummaryResponse> _allChats = [];
    private readonly List<MessageResponse> _allMessages = [];
    private ChatListItemViewModel? _selectedChatItem;
    private ChatSummaryResponse? _selectedChat;
    private bool _isBusy;
    private bool _isSendingMessage;
    private string _statusMessage = "Загрузка чатов...";
    private string _draftMessage = string.Empty;
    private string _searchQuery = string.Empty;
    private int? _currentUserId;

    public MessengerViewModel(
        CreativityApiClient apiClient,
        ILastChatStore lastChatStore,
        IAuthTokenStore authTokenStore)
    {
        _apiClient = apiClient;
        _lastChatStore = lastChatStore;
        _authTokenStore = authTokenStore;
        ChatItems = [];
        MessageItems = [];
        CreateChatCommand = new Command(async () => await CreateChatAsync(), () => !IsBusy && !IsSendingMessage);
        SendMessageCommand = new Command(async () => await SendMessageAsync(), CanSendMessage);
    }

    public ObservableCollection<ChatListItemViewModel> ChatItems { get; }

    public ObservableCollection<MessageItemViewModel> MessageItems { get; }

    public Command CreateChatCommand { get; }
    public Command SendMessageCommand { get; }

    public ChatListItemViewModel? SelectedChatItem
    {
        get => _selectedChatItem;
        set
        {
            if (SetProperty(ref _selectedChatItem, value))
            {
                SetSelectedChatVisualState();
                _selectedChat = value?.Chat;
                OnPropertyChanged(nameof(SelectedChatTitle));
                OnPropertyChanged(nameof(SelectedChatSubtitle));
                UpdateCommandState();
                _ = SelectChatAsync(value?.Chat);
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
        _selectedChat is null
            ? "Выберите чат"
            : string.IsNullOrWhiteSpace(_selectedChat.Title) ? $"Chat #{_selectedChat.Id}" : _selectedChat.Title;

    public string SelectedChatSubtitle =>
        _selectedChat is null
            ? "Откройте чат из списка слева"
            : "last seen recently";

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ApplyChatFilter();
            }
        }
    }

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
        await EnsureCurrentUserIdAsync();
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
            _allChats.Clear();
            _allChats.AddRange((response?.Chats ?? []).OrderByDescending(x => x.UpdatedAt));
            ApplyChatFilter();

            if (ChatItems.Count == 0)
            {
                MessageItems.Clear();
                _allMessages.Clear();
                SelectedChatItem = null;
                await _lastChatStore.ClearLastSelectedChatIdAsync();
                StatusMessage = "Чатов пока нет. Создайте новый чат.";
                return;
            }

            await RestoreSelectedChatAsync();
            StatusMessage = $"Чатов: {ChatItems.Count}";
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
            SelectedChatItem = ChatItems[0];
            return;
        }

        var targetChat = _allChats.FirstOrDefault(x => x.Id == savedChatId.Value) ?? _allChats[0];
        SelectedChatItem = ChatItems.FirstOrDefault(x => x.Chat.Id == targetChat.Id) ?? ChatItems[0];
    }

    private async Task SelectChatAsync(ChatSummaryResponse? chat)
    {
        if (chat is null)
        {
            MessageItems.Clear();
            _allMessages.Clear();
            UpdateCommandState();
            return;
        }

        try
        {
            await _lastChatStore.SaveLastSelectedChatIdAsync(chat.Id);
            var response = await _apiClient.GetChatMessagesAsync(chat.Id);
            _allMessages.Clear();
            _allMessages.AddRange((response?.Messages ?? []).OrderBy(x => x.CreatedAt));
            RebuildMessageItems();

            StatusMessage = MessageItems.Count == 0
                ? "В этом чате пока нет сообщений."
                : $"Сообщений: {MessageItems.Count}";
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
                SelectedChatItem = ChatItems.FirstOrDefault(x => x.Chat.Id == createdChat.Chat.Id) ?? SelectedChatItem;
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
            && _selectedChat is not null
            && !string.IsNullOrWhiteSpace(DraftMessage);
    }

    private async Task SendMessageAsync()
    {
        if (_selectedChat is null)
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
            var sentMessage = await _apiClient.SendMessageAsync(_selectedChat.Id, request);

            if (sentMessage is not null)
            {
                _allMessages.Add(sentMessage);
                _currentUserId ??= sentMessage.SenderUserId;
                MessageItems.Add(MapMessageItem(sentMessage));
            }
            else
            {
                await SelectChatAsync(_selectedChat);
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

    private void ApplyChatFilter()
    {
        var query = SearchQuery?.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allChats
            : _allChats.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.Title) && x.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(x.Description) && x.Description.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();

        ChatItems.Clear();
        foreach (var chat in filtered)
        {
            ChatItems.Add(MapChatItem(chat));
        }

        if (ChatItems.Count == 0)
        {
            return;
        }

        if (_selectedChat is not null)
        {
            _selectedChatItem = ChatItems.FirstOrDefault(x => x.Chat.Id == _selectedChat.Id);
            SetSelectedChatVisualState();
            OnPropertyChanged(nameof(SelectedChatItem));
        }
    }

    private ChatListItemViewModel MapChatItem(ChatSummaryResponse chat)
    {
        var title = string.IsNullOrWhiteSpace(chat.Title) ? $"Chat #{chat.Id}" : chat.Title;
        var subtitle = string.IsNullOrWhiteSpace(chat.Description) ? "Tap to open conversation" : chat.Description;
        var timeText = chat.UpdatedAt.LocalDateTime.ToString("HH:mm");
        var initials = BuildInitials(title);
        var unreadCount = chat.LastMessageId.HasValue && (_selectedChat is null || _selectedChat.Id != chat.Id) ? 1 : 0;

        return new ChatListItemViewModel(chat, title, subtitle, timeText, initials, unreadCount)
        {
            IsSelected = _selectedChat?.Id == chat.Id
        };
    }

    private void RebuildMessageItems()
    {
        MessageItems.Clear();
        foreach (var message in _allMessages)
        {
            MessageItems.Add(MapMessageItem(message));
        }
    }

    private MessageItemViewModel MapMessageItem(MessageResponse message)
    {
        var isOutgoing = _currentUserId.HasValue && message.SenderUserId == _currentUserId.Value;
        return new MessageItemViewModel(
            message,
            isOutgoing,
            !isOutgoing,
            isOutgoing ? "You" : $"User #{message.SenderUserId}",
            message.CreatedAt.LocalDateTime.ToString("HH:mm"));
    }

    private static string BuildInitials(string title)
    {
        var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0)
        {
            return "C";
        }

        if (words.Length == 1)
        {
            return words[0].Length >= 2
                ? words[0][..2].ToUpperInvariant()
                : words[0].ToUpperInvariant();
        }

        var initials = new StringBuilder(2);
        initials.Append(char.ToUpperInvariant(words[0][0]));
        initials.Append(char.ToUpperInvariant(words[1][0]));
        return initials.ToString();
    }

    private void SetSelectedChatVisualState()
    {
        foreach (var item in ChatItems)
        {
            item.IsSelected = _selectedChatItem is not null && item.Chat.Id == _selectedChatItem.Chat.Id;
        }
    }

    private async Task EnsureCurrentUserIdAsync()
    {
        if (_currentUserId.HasValue)
        {
            return;
        }

        var token = await _authTokenStore.GetTokenAsync();
        _currentUserId = TryExtractUserId(token);
    }

    private static int? TryExtractUserId(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            var padding = 4 - (payload.Length % 4);
            if (padding is > 0 and < 4)
            {
                payload = payload.PadRight(payload.Length + padding, '=');
            }

            var payloadBytes = Convert.FromBase64String(payload);
            using var json = JsonDocument.Parse(payloadBytes);

            if (!json.RootElement.TryGetProperty("sub", out var subClaim))
            {
                return null;
            }

            return subClaim.ValueKind switch
            {
                JsonValueKind.Number when subClaim.TryGetInt32(out var numberValue) => numberValue,
                JsonValueKind.String when int.TryParse(subClaim.GetString(), out var stringValue) => stringValue,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}

public sealed class ChatListItemViewModel : ViewModelBase
{
    private bool _isSelected;

    public ChatListItemViewModel(
        ChatSummaryResponse chat,
        string title,
        string subtitle,
        string timeText,
        string avatarInitials,
        int unreadCount)
    {
        Chat = chat;
        Title = title;
        Subtitle = subtitle;
        TimeText = timeText;
        AvatarInitials = avatarInitials;
        UnreadCount = unreadCount;
    }

    public ChatSummaryResponse Chat { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string TimeText { get; }
    public string AvatarInitials { get; }
    public int UnreadCount { get; }
    public bool HasUnread => UnreadCount > 0;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed record MessageItemViewModel(
    MessageResponse Message,
    bool IsOutgoing,
    bool IsIncoming,
    string SenderCaption,
    string TimeText);
