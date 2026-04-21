using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
using CommonObjects.Responses;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using System.Collections.Concurrent;

namespace SpoofMessageService;


[Authorize]
public class ChatHub(
        IMessageService messageService,
        IUserService userService,
        IChatUserService chatUserService,
        IFileTokenService fileTokenService
    ) : Hub
{
    private readonly IChatUserService _chatUserService = chatUserService;
    private readonly IMessageService _messageService = messageService;
    private readonly IUserService _userService = userService;
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly static ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, UserConnection>> Users = [];

    public override async Task OnConnectedAsync()
    {
        Guid userId = ClaimService.GetUserId(Context.User);
        Guid sessionId = ClaimService.GetSessionId(Context.User);

        await _userService.ChangeConnectionState(userId, true);
        if (Users.TryGetValue(userId, out ConcurrentDictionary<Guid, UserConnection>? sessions))
            sessions.TryAdd(sessionId, new(Context.ConnectionId, sessionId));
        else
            Users.TryAdd(userId, new() { [sessionId] = new(Context.ConnectionId, sessionId) });
        Result<List<ChatUser>> chatUsers = await _chatUserService.GetChats(userId);
        if (chatUsers.Success)
            foreach (var chatUser in chatUsers.Body!)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{chatUser.Chat.UniqueName}");
            }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid userId = ClaimService.GetUserId(Context.User);
        Guid sessionId = ClaimService.GetSessionId(Context.User);

        await _userService.ChangeConnectionState(userId, false);
        if (Users.TryGetValue(userId, out ConcurrentDictionary<Guid, UserConnection>? sessions))
            sessions.TryRemove(sessionId, out _);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageRequest request)
    {
        Guid userId = ClaimService.GetUserId(Context.User);

        Result<MessageDTO> result = await _messageService.SendMessage(request, userId);
        if (!result.Success)
            throw new ApplicationException(result.Error ?? result.Message);
    }

    public async Task DeleteMessage(Guid chatId, Guid messageId)
    {
        Guid userId = ClaimService.GetUserId(Context.User);

        Result result = await _messageService.DeleteMessage(messageId, chatId, userId);
    }

    public async Task EditMessage(EditMessageRequest request)
    {
        Guid userId = ClaimService.GetUserId(Context.User);

        Result<EditMessageResponse> result = await _messageService.EditMessage(request, userId);

        if (!result.Success)
            throw new ApplicationException(result.Error ?? result.Message);
        Result<List<ChatUser>> users = await _chatUserService.GetMembers(result.Body!.ChatId);
        EditMessageResponse message = new(
                result.Body!.Id,
                result.Body.ChatId,
                result.Body.SenderLogin,
                result.Body.SenderName,
                null,
                null,
                result.Body.OriginalAvatarName,
                result.Body.Text,
                result.Body.LastModified,
                []
            );
        Guid? avatarId = result.Body.UserAvatarId is null ? null : new(result.Body.UserAvatarId);
        if (users.Success)
            await Parallel.ForEachAsync(users.Body!, async (user, token) =>
            {
                if (Users.TryGetValue(user.Key2, out ConcurrentDictionary<Guid, UserConnection>? connections))
                {
                    foreach (var connection in connections.Values)
                    {
                        await Clients.Client(connection.Ip).SendAsync("edited-message", message with
                        {
                            UserAvatarToken = avatarId is null
                            ? null
                            : _fileTokenService.CreateToken(
                                user.Key2,
                                avatarId.Value),
                            UserAvatarId = result.Body.UserAvatarId is null
                            ? null
                            : Hasher.GetKey(result.Body.UserAvatarId),
                            Attachments = [.. result.Body.Attachments.Select(x =>
                            new EditAttachment(
                                x.IsAdded,
                                Hasher.GetKey(x.Id),
                                _fileTokenService.CreateToken(user.Key2, new(x.Id)),
                                x.OriginalFileName,
                                x.Category,
                                x.Metadata,
                                x.Size))]
                        }, token);
                    }
                }
            });
    }
}
