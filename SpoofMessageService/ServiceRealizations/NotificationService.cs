using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Requests.Changes;
using CommonObjects.Results;
using Microsoft.AspNetCore.SignalR;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations;

public class NotificationService : BackgroundService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUserEventService _userService;
    private readonly IChatEventService _chatEventService;
    private readonly IMessageEventService _messageEventService;
    private readonly ILoggerService loggerService;
    private readonly IChatUserService _chatUserService;
    public NotificationService(IServiceScopeFactory serviceScope)
    {
        IServiceScope scope = serviceScope.CreateScope();
        _hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
        _userService = scope.ServiceProvider.GetRequiredService<IUserEventService>();
        loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
        _chatUserService = scope.ServiceProvider.GetRequiredService<IChatUserService>();
        _messageEventService = scope.ServiceProvider.GetRequiredService<IMessageEventService>();
        _chatEventService = scope.ServiceProvider.GetRequiredService<IChatEventService>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _userService.UserUpdated += UserUpdated;
        _messageEventService.OnMessageRecived += OnMessageRecived;
        _chatEventService.ChatUpdated += ChatUpdated;

        await Task.Delay(-1, stoppingToken);
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

    public override void Dispose()
    {
        _userService.UserUpdated -= UserUpdated;
        _messageEventService.OnMessageRecived -= OnMessageRecived;
        _chatEventService.ChatUpdated -= ChatUpdated;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
