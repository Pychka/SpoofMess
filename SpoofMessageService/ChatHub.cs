using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
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

        Result<IntermediateMessage> result = await _messageService.SendMessage(request, userId);
        if (!result.Success)
            return;
        Result<List<ChatUser>> users = await _chatUserService.GetMembers(result.Body!.ChatId);
        MessageDTO message = new(
                result.Body!.Id,
                result.Body.ChatId,
                result.Body.SenderLogin,
                result.Body.SenderName,
                null,
                null,
                result.Body.OriginalFileName,
                result.Body.Text,
                result.Body.SendAt,
                []
            );
        if (users.Success)
            await Parallel.ForEachAsync(users.Body!, async (user, token) =>
            {
                if (Users.TryGetValue(user.Key2, out ConcurrentDictionary<Guid, UserConnection>? connections))
                {
                    foreach (var connection in connections.Values)
                    {
                        await Clients.Client(connection.Ip).SendAsync("new-message", message with
                        {
                            UserAvatarToken = result.Body.SenderAvatar is null
                            ? null
                            : _fileTokenService.CreateToken(
                                user.Key2,
                                result.Body.SenderAvatar.Value),
                            UserAvatarId = result.Body.SenderAvatar is null
                            ? null
                            : Hasher.GetKey(result.Body.SenderAvatar.Value.ToByteArray()),
                            Attachments = [.. result.Body.Attachments.Select(x => 
                            new CommonObjects.Requests.Attachments.Attachment(
                                Hasher.GetKey(x.Id.ToByteArray()),
                                _fileTokenService.CreateToken(user.Key2, x.Id),
                                x.OriginalFileName,
                                x.Category,
                                x.Metadata,
                                x.FileSize))]
                        }, token);
                    }
                }
            });
    }

    public async Task DeleteMessage(DeleteMessageRequest request)
    {
        Guid userId = ClaimService.GetUserId(Context.User);

        Result result = await _messageService.DeleteMessage(request, userId);
    }

    public async Task EditMessage(EditMessageRequest request)
    {
        Guid userId = ClaimService.GetUserId(Context.User);

        Result result = await _messageService.EditMessage(request, userId);
    }
}
