using SpoofEntranceService.ServiceRealizations.Consumers;
using SpoofEntranceService.ServiceRealizations.Publishers;
using SpoofEntranceService.ServiceRealizations.Repositories;
using SpoofEntranceService.ServiceRealizations.Validators;
using SpoofEntranceService.ServiceRealizations;
using SpoofEntranceService.Services.Repositories;
using SpoofEntranceService.Services.Publishers;
using SpoofEntranceService.Services.Validators;
using SpoofEntranceService.Services;
using SpoofEntranceService.Models;
using SettingsHelper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

//swagger api documentation service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.SetBaseSettingsWithFactory<SpoofEntranceServiceContext>();

builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUserEntryRepository, UserRepository>();

builder.Services.AddTransient<ITokenValidator, TokenValidator>();
builder.Services.AddTransient<ISessionValidator, SessionValidator>();
builder.Services.AddTransient<IUserEntryValidator, UserEntryValidator>();

//logic services
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IUserEntryService, UserEntryService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddTransient<IUserPublisherService, UserPublisherService>();

builder.Services.AddHostedService<UserConsumerService>();

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
