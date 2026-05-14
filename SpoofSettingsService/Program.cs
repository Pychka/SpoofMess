using Microsoft.EntityFrameworkCore;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SecurityLibrary.TokensRealizations;
using SettingsHelper;
using SpoofSettingsService.Models;
using SpoofSettingsService.ServiceRealizations;
using SpoofSettingsService.ServiceRealizations.MessageBrokers.Consumers;
using SpoofSettingsService.ServiceRealizations.MessageBrokers.Outboxers;
using SpoofSettingsService.ServiceRealizations.MessageBrokers.Publishers;
using SpoofSettingsService.ServiceRealizations.Repositories;
using SpoofSettingsService.ServiceRealizations.Validators;
using SpoofSettingsService.Services;
using SpoofSettingsService.Services.MessageBrokers;
using SpoofSettingsService.Services.Repositories;
using SpoofSettingsService.Services.Validators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.SetBaseSettings();

builder.Services.AddDbContextFactory<SpoofSettingsServiceContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("Sql"), x =>
{
    x.MapEnum<OutboxStatus>("OutboxStatus");
    x.ConfigureDataSource(ndsb =>
    {
        //ndsb.MapComposite<PermissionResult>("permission_result");
        
    });
}));

builder.Services.AddSingleton<IUserMessageBrokerService, UserPublisherService>();
builder.Services.AddSingleton<IChatUserPublisherService, ChatUserPublisherService>();
builder.Services.AddSingleton<IChatPublisherService, ChatPublisherService>();
builder.Services.AddSingleton<IUserAvatarPublisherService, UserAvatarPublisherService>();
builder.Services.AddSingleton<IChatAvatarPublisherService, ChatAvatarPublisherService>();

builder.Services.AddHostedService<FileMetadatumConsumerService>();
builder.Services.AddHostedService<UserConsumerService>();
builder.Services.AddHostedService<ChatUserOutboxer>();

builder.Services.AddTransient<IFileMetadatumValidator, FileMetadatumValidator>();
builder.Services.AddTransient<IChatAvatarValidator, ChatAvatarValidator>();
builder.Services.AddTransient<IChatValidator, ChatValidator>();
builder.Services.AddTransient<IChatUserValidator, ChatUserValidator>();
builder.Services.AddTransient<IUserAvatarValidator, UserAvatarValidator>();
builder.Services.AddTransient<IStickerPackValidator, StickerPackValidator>();
builder.Services.AddTransient<IStickerValidator, StickerValidator>();
builder.Services.AddTransient<IUserValidator, UserValidator>();
builder.Services.AddTransient<IRuleValidator, RuleValidator>();

builder.Services.AddScoped<IFileMetadatumRepository, FileMetadatumRepository>();
builder.Services.AddScoped<IChatAvatarRepository, ChatAvatarRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatUserRepository, ChatUserRepository>();
builder.Services.AddScoped<IStickerPackRepository, StickerPackRepository>();
builder.Services.AddScoped<IStickerRepository, StickerRepository>();
builder.Services.AddScoped<IUserAvatarRepository, UserAvatarRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatTypeRepository, ChatTypeRepository>();
builder.Services.AddScoped<IRuleRepository, RuleRepository>();
builder.Services.AddScoped<IChatUserOutboxRepository, ChatUserOutboxRepository>();

builder.Services.AddScoped<IChatAvatarService, ChatAvatarService>();
builder.Services.AddScoped<IFileMetadatumService, FileMetadatumService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatTypeService, ChatTypeService>();
builder.Services.AddScoped<IChatUserService, ChatUserService>();
builder.Services.AddScoped<IStickerPackService, StickerPackService>();
builder.Services.AddScoped<IStickerService, StickerService>();
builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRuleService, RuleService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddTransient<IFileTokenService, FileTokenService>();
builder.Services.AddTransient<IAttachmentAccessTokenService, AttachmentAccessTokenService>();
builder.Services.AddSingleton(
    builder.Configuration.GetSection("TokenHeader")
    .Get<LivedTokenHeaderCover>()!);
builder.Services.AddSingleton(
    builder.Configuration.GetSection("TokenHeader")
    .Get<TokenHeaderCover>()!);

builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

app.UseSwaggerUI();
app.UseSwagger();
app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.UseSwagger();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors();
app.MapControllers();

app.Run();