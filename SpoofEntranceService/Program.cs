using AdditionalHelpers;
using DataHelpers.ServiceRealizations;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SpoofEntranceService.Models;
using SpoofEntranceService.ServiceRealizations;
using SpoofEntranceService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
//"Server=.;Database=SpoofEntranceService;Trusted_Connection=True;TrustServerCertificate=True"
//data services
builder.Services.AddDbContext<SpoofEntranceServiceDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    configuration.ReconnectRetryPolicy = new LinearRetry(1000);

    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddTransient<IMemoryCache, MemoryCache>();

builder.Services.AddTransient<IRepository, Repository>();
builder.Services.AddTransient<ILocalCacheService, LocalCacheService>();
builder.Services.AddTransient<ICacheService, CacheService>();

//logic services
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddTransient<IUserEntryService, UserEntryService>();
builder.Services.AddTransient<ITokenService, TokenService>();

builder.Services.AddTransient<ILoggerService>(provider =>
    new ConsoleLoggerService(
        minLogLevel: Enum.Parse<AdditionalHelpers.LogLevel>(builder.Configuration["Logging:LogLevel"] ?? "Information")
    )
);
//additional services
/*builder.Services.AddTransient<ILoggerService>(provider =>
    new FileLoggerService(
        minLevel: Enum.Parse<AdditionalHelpers.LogLevel>(builder.Configuration["Logging:LogLevel"] ?? "Information"),
        directoryPath: builder.Configuration["Logging:DirectoryPath"] ?? "logs",
        maxSize: long.Parse(builder.Configuration["Logging:MaxSize"] ?? "51200"),
        maxFiles: int.Parse(builder.Configuration["Logging:MaxFiles"] ?? "10"),
        bufferSize: int.Parse(builder.Configuration["Logging:BufferSize"] ?? "4096")
    )
);*/

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();
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
