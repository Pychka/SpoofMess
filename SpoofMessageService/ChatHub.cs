using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
using CommonObjects.Responses;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SecurityLibrary;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using System.Collections.Concurrent;

namespace SpoofMessageService;

public class ConnectionTracker
{
    private readonly static ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, UserConnection>> Users = [];

    public bool Get(Guid userId, out ConcurrentDictionary<Guid, UserConnection>? user) =>
        Users.TryGetValue(userId, out user);

    public bool Add(Guid userId, ConcurrentDictionary<Guid, UserConnection> user) =>
        Users.TryAdd(userId, user);
}


[Authorize]
public class ChatHub(
        IMessageService messageService,
        IUserService userService,
        IChatUserService chatUserService,
        ConnectionTracker tracker
    ) : Hub
{
    private readonly IChatUserService _chatUserService = chatUserService;
    private readonly ConnectionTracker _tracker = tracker;
    private readonly IMessageService _messageService = messageService;
    private readonly IUserService _userService = userService;

    public override async Task OnConnectedAsync()
    {
        Guid userId = ClaimService.GetUserId(Context.User);
        Guid sessionId = ClaimService.GetSessionId(Context.User);

        await _userService.ChangeConnectionState(userId, true);
        if (_tracker.Get(userId, out ConcurrentDictionary<Guid, UserConnection>? sessions))
            sessions.TryAdd(sessionId, new(Context.ConnectionId, sessionId));
        else
            _tracker.Add(userId, new() { [sessionId] = new(Context.ConnectionId, sessionId) });
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
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
        if (_tracker.Get(userId, out ConcurrentDictionary<Guid, UserConnection>? sessions))
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
    }
}
