using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Application.Chats;
using AppChatsService = API.Application.Chats.ChatsService;

namespace API.ApiService.Features.Chats;

public static class ChatsEndpoints
{
    public static IEndpointRouteBuilder MapChatsEndpoints(this IEndpointRouteBuilder app)
    {
        var chats = app.MapGroup("/chats").RequireAuthorization();

        chats.MapPost("/", (CreateChatRequest request, ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var result = chatsService.CreateChat(
                currentUserId.Value,
                new CreateChatCommand(
                    request.ChatType,
                    request.Title,
                    request.Description,
                    request.ParticipantUserIds ?? []));

            return ToHttpResult(result, details => Results.Created($"/chats/{details.Chat.Id}", ToDetailsResponse(details)));
        });

        chats.MapGet("/", (ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var items = chatsService.GetChats(currentUserId.Value)
                .Select(ToSummaryResponse)
                .ToArray();

            return Results.Ok(new ChatsListResponse(items));
        });

        chats.MapGet("/{chatId:long}", (long chatId, ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var result = chatsService.GetChat(chatId, currentUserId.Value);
            return ToHttpResult(result, details => Results.Ok(ToDetailsResponse(details)));
        });

        chats.MapPatch("/{chatId:long}", (long chatId, UpdateChatRequest request, ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var result = chatsService.UpdateChat(chatId, currentUserId.Value, new UpdateChatCommand(request.Title, request.Description));
            return ToHttpResult(result, details => Results.Ok(ToDetailsResponse(details)));
        });

        chats.MapPost("/{chatId:long}/participants", (long chatId, AddParticipantRequest request, ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var result = chatsService.AddParticipant(chatId, currentUserId.Value, new AddParticipantCommand(request.UserId, request.Role));
            return ToHttpResult(result, details => Results.Ok(ToDetailsResponse(details)));
        });

        chats.MapDelete("/{chatId:long}/participants/{userId:int}", (long chatId, int userId, ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var result = chatsService.RemoveParticipant(chatId, currentUserId.Value, userId);
            return ToHttpResult(result, _ => Results.NoContent());
        });

        chats.MapPost("/{chatId:long}/messages", (long chatId, SendMessageRequest request, ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var result = chatsService.SendTextMessage(
                chatId,
                currentUserId.Value,
                new SendMessageCommand(request.Text, request.ReplyToMessageId));

            return ToHttpResult(result, message => Results.Created($"/chats/{chatId}/messages/{message.Id}", ToMessageResponse(message)));
        });

        chats.MapGet("/{chatId:long}/messages", (long chatId, int? limit, long? before, ClaimsPrincipal principal, AppChatsService chatsService) =>
        {
            var currentUserId = GetCurrentUserId(principal);
            if (currentUserId is null)
            {
                return Results.Unauthorized();
            }

            var result = chatsService.GetMessages(chatId, currentUserId.Value, limit, before);
            return ToHttpResult(result, messages => Results.Ok(new MessagesListResponse(messages.Select(ToMessageResponse).ToArray())));
        });

        return app;
    }

    private static int? GetCurrentUserId(ClaimsPrincipal principal)
    {
        var rawUserId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(rawUserId, out var userId)
            ? userId
            : null;
    }

    private static IResult ToHttpResult<T>(ServiceResult<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return onSuccess(result.Value);
        }

        var error = new ErrorResponse(
            Code: result.ErrorCode.ToString(),
            Message: result.ErrorMessage ?? "Unexpected error.");

        return result.ErrorCode switch
        {
            ServiceErrorCode.BadRequest => Results.BadRequest(error),
            ServiceErrorCode.Forbidden => Results.StatusCode(StatusCodes.Status403Forbidden),
            ServiceErrorCode.NotFound => Results.NotFound(error),
            ServiceErrorCode.Conflict => Results.Conflict(error),
            _ => Results.Problem(error.Message)
        };
    }

    private static ChatSummaryResponse ToSummaryResponse(ChatSummary chat) =>
        new(
            Id: chat.Id,
            ChatType: chat.ChatType,
            Title: chat.Title,
            Description: chat.Description,
            CreatedBy: chat.CreatedBy,
            LastMessageId: chat.LastMessageId,
            IsArchived: chat.IsArchived,
            CreatedAt: chat.CreatedAt,
            UpdatedAt: chat.UpdatedAt);

    private static ChatDetailsResponse ToDetailsResponse(ChatDetails details) =>
        new(
            Chat: ToSummaryResponse(details.Chat),
            Participants: details.Participants
                .Select(p => new ChatParticipantResponse(
                    UserId: p.UserId,
                    Role: p.Role,
                    JoinedAt: p.JoinedAt,
                    LeftAt: p.LeftAt))
                .ToArray());

    private static MessageResponse ToMessageResponse(MessageItem item) =>
        new(
            Id: item.Id,
            ChatId: item.ChatId,
            SenderUserId: item.SenderUserId,
            Text: item.Text,
            CreatedAt: item.CreatedAt,
            ReplyToMessageId: item.ReplyToMessageId);
}
