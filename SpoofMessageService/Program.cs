using RuleRoleHelper.ServiceRealizations;
using RuleRoleHelper.Services;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SettingsHelper;
using SpoofMessageService;
using SpoofMessageService.Models;
using SpoofMessageService.ServiceRealizations;
using SpoofMessageService.ServiceRealizations.Consumers;
using SpoofMessageService.ServiceRealizations.Events;
using SpoofMessageService.ServiceRealizations.Repositories;
using SpoofMessageService.ServiceRealizations.Validators;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;
using SpoofMessageService.Services.Repositories;
using SpoofMessageService.Services.Validators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.SetBaseSettingsWithFactory<SpoofMessageServiceContext>();

builder.Services.AddHostedService<NotificationService>();
builder.Services.AddHostedService<ChatUserConsumerService>();
builder.Services.AddHostedService<UserSESConsumerService>();
builder.Services.AddHostedService<UserSSSConsumerService>();
builder.Services.AddHostedService<UserAvatarConsumerService>();
builder.Services.AddHostedService<ChatAvatarConsumerService>();
builder.Services.AddHostedService<ChatConsumerService>();
builder.Services.AddHostedService<FileMetadatumConsumerService>();

builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();
builder.Services.AddScoped<IChatAvatarService, ChatAvatarService>();
builder.Services.AddScoped<IChatUserService, ChatUserService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFileMetadatumService, FileMetadatumService>();

builder.Services.AddScoped<IUserValidator, UserValidator>();
builder.Services.AddScoped<IMessageValidator, MessageValidator>();
builder.Services.AddScoped<IChatUserValidator, ChatUserValidator>();
builder.Services.AddScoped<IRuleParserService, RuleParserService>();
builder.Services.AddScoped<IFileMetadatumValidator, FileMetadatumValidator>();

builder.Services.AddScoped<IRuleService, RuleService>();

builder.Services.AddScoped<ISearchRepository, SearchRepository>();
builder.Services.AddScoped<IChatUserRepository, ChatUserRepository>();
builder.Services.AddScoped<IFileMetadatumRepository, FileMetadatumRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddTransient<IFileTokenService, FileTokenService>();

builder.Services.AddSingleton<IUserEventsService, UserEventsService>();

builder.Services.AddSingleton(
    builder.Configuration.GetSection("TokenHeader")
    .Get<TokenHeaderCover>()!);

WebApplication app = builder.Build();
app.MapHub<ChatHub>("/chat");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapSwagger();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
