using AdditionalHelpers;
using DataHelpers.ServiceRealizations;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofEntranceService.Models;
using SpoofEntranceService.ServiceRealizations;
using SpoofEntranceService.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

//data services
builder.Services.AddDbContext<SpoofEntranceServiceDbContext>(x => x.UseSqlServer("Server=.;Database=SpoofEntranceServiceDB;Trusted_Connection=True;TrustServerCertificate=True"));

builder.Services.AddTransient<IRepository, Repository>();
builder.Services.AddTransient<ILocalCacheService, LocalCacheService>();
builder.Services.AddTransient<ICacheService, CacheService>();

//logic services
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddTransient<IUserEntryService, UserEntryService>();
builder.Services.AddTransient<ITokenService, TokenService>();

//additional services
builder.Services.AddTransient<ILoggerService, FileLoggerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
