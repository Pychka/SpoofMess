using CommonObjects.DTO;
using CommonObjects.Requests.Changes;
using CommonObjects.Responses;
using CommonObjects.Results;
using Microsoft.AspNetCore.SignalR;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;
using System.Collections.Concurrent;

namespace SpoofMessageService.ServiceRealizations;

public class NotificationService : BackgroundService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUserEventService _userService;
    private readonly IChatEventService _chatEventService;
    private readonly IMessageEventService _messageEventService;
    private readonly IChatUserService _chatUserService;
    private readonly IChatService _chatService;
    private readonly IFileTokenService _fileTokenService;
    private readonly IAttachmentAccessTokenService _attachmentAccessTokenService;
    private readonly ConnectionTracker _tracker;
    public NotificationService(IServiceScopeFactory serviceScope)
    {
        IServiceScope scope = serviceScope.CreateScope();
        _hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
        _userService = scope.ServiceProvider.GetRequiredService<IUserEventService>();
        _chatUserService = scope.ServiceProvider.GetRequiredService<IChatUserService>();
        _messageEventService = scope.ServiceProvider.GetRequiredService<IMessageEventService>();
        _chatEventService = scope.ServiceProvider.GetRequiredService<IChatEventService>();
        _chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
        _attachmentAccessTokenService = scope.ServiceProvider.GetRequiredService<IAttachmentAccessTokenService>();
        _fileTokenService = scope.ServiceProvider.GetRequiredService<IFileTokenService>();
        _tracker = scope.ServiceProvider.GetRequiredService<ConnectionTracker>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _userService.UserUpdated += UserUpdated;
        _messageEventService.OnMessageRecived += OnMessageRecived;
        _messageEventService.OnMessageEdited += OnMessageEdited;
        _chatEventService.ChatUpdated += ChatUpdated;
        _chatEventService.ChatAvatarUpdated += ChatAvatarUpdated;
        _chatEventService.ChatCreated += ChatUserCreated;
        _messageEventService.OnMessageDeleted += OnMessageDeleted;

        await Task.Delay(-1, stoppingToken);
    }

    private async void OnMessageDeleted(Guid messageId, Chat chat)
    {
        await _hubContext.Clients.Group($"chat-{chat.UniqueName}").SendAsync("delete-message", messageId, chat.Id);
    }

    private async void OnMessageEdited(EditMessageResponse message, Chat chat)
    {
        await _hubContext.Clients.Group($"chat-{chat.UniqueName}").SendAsync("edited-message", message);
    }

    private async void OnMessageRecived(MessageDTO message, Chat chat)
    {
        await _hubContext.Clients.Group($"chat-{chat.UniqueName}").SendAsync("new-message", message);
    }

    private async void UserUpdated(UpdateUserInfo user, Guid userId)
    {
        Result<List<ChatUser>> chatUsers = await _chatUserService.GetChats(userId);
        if (chatUsers.Success)
            foreach (var chat in chatUsers.Body!)
                await _hubContext.Clients.Group($"chat-{chat.Chat.UniqueName}").SendAsync("user-updated", user);
    }

    private async void ChatUpdated(ChangeChatSettingsRequest request, Chat chat)
    {
        await _hubContext.Clients.Group($"chat-{chat.UniqueName}").SendAsync("chat-updated", request);
    }

    private async void ChatUserCreated(ChatUser chatUser)
    {
        Result<Chat> chatResult = await _chatService.Get(chatUser.Key1);
        if (!chatResult.Success)
            return;
        if (_tracker.Get(chatUser.Key2, out ConcurrentDictionary<Guid, UserConnection>? users))
            foreach (UserConnection userConnection in users.Values)
                await _hubContext.Groups.AddToGroupAsync(userConnection.Ip, $"chat-{chatResult.Body!.UniqueName}");
        await _hubContext.Clients.Group($"user-{chatUser.Key2}").SendAsync(
            "chat-user-created",
            new ChatUserDTO(
                chatResult.Body!.Id,
                chatResult.Body.UniqueName,
                chatResult.Body.LastModified,
                chatResult.Body!.Name,
                chatResult.Body.Avatar?.Metadata,
                chatResult.Body.OriginalFileName
                )
            {
                Rules = chatUser.Rules,
                AvatarId = chatResult.Body.AvatarId is null ? [] : Hasher.GetKey(chatResult.Body.AvatarId.Value.ToByteArray()),
                AvatarAccessToken = chatResult.Body.AvatarId is null ? [] : _attachmentAccessTokenService.CreateToken(chatResult.Body.AvatarId.Value),
                AvatarFileToken = chatResult.Body.AvatarId is null ? [] : _fileTokenService.CreateToken(chatUser.Key2, chatResult.Body.AvatarId.Value)
            });
    }


    private async void ChatAvatarUpdated(ChatAvatarResponse request)
    {
        await _hubContext.Clients.Group($"chat-{request.UniqueName}").SendAsync("chat-avatar-updated", request);
    }

    public override void Dispose()
    {
        _userService.UserUpdated -= UserUpdated;
        _messageEventService.OnMessageRecived -= OnMessageRecived;
        _chatEventService.ChatUpdated -= ChatUpdated;
        _chatEventService.ChatAvatarUpdated -= ChatAvatarUpdated;
        _messageEventService.OnMessageEdited -= OnMessageEdited;
        _chatEventService.ChatCreated -= ChatUserCreated;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
