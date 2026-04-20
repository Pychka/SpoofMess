using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Results;
using Microsoft.AspNetCore.SignalR;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations;

public class NotificationService : BackgroundService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUserEventsService _userService;
    private readonly ILoggerService loggerService;
    private readonly IChatUserService _chatUserService;
    public NotificationService(IServiceScopeFactory serviceScope)
    {
        IServiceScope scope = serviceScope.CreateScope();
        _hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
        _userService = scope.ServiceProvider.GetRequiredService<IUserEventsService>();
        loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
        _chatUserService = scope.ServiceProvider.GetRequiredService<IChatUserService>();
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _userService.UserUpdated += UserUpdated;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async void UserUpdated(UpdateUserInfo user, Guid userId)
    {
        Result<List<ChatUser>> chatUsers = await _chatUserService.GetChats(userId);
        if (chatUsers.Success)
            foreach (var chat in chatUsers.Body!)
                await _hubContext.Clients.Group($"chat-{chat.Chat.UniqueName}").SendAsync("user-updated", user);
    }

    public override void Dispose()
    {
        _userService.UserUpdated -= UserUpdated;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
