using AdditionalHelpers;
using DataHelpers.ServiceRealizations;
using DataHelpers.ServiceRealizations.Cache;
using DataHelpers.ServiceRealizations.Cache.Memory;
using DataHelpers.ServiceRealizations.Cache.Redis;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofEntranceService.Models;
using SpoofEntranceService.Repositories;
using SpoofEntranceService.ServiceRealizations;
using SpoofEntranceService.Services;
using SpoofEntranceService.Validators;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

//swagger api documentation service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
//"Server=.;Database=SpoofEntranceService;Trusted_Connection=True;TrustServerCertificate=True"
//data services
Console.WriteLine(builder.Configuration.GetConnectionString("Redis"));
builder.Services.AddDbContext<SpoofEntranceServiceDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    configuration.ReconnectRetryPolicy = new LinearRetry(1000);

    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddTransient<ProcessQueueTasksService>();

//in-memory cache
builder.Services.AddTransient<Microsoft.Extensions.Caching.Memory.IMemoryCache, Microsoft.Extensions.Caching.Memory.MemoryCache>();
builder.Services.AddTransient<IMemoryCacheService, LocalCacheService>();

//redis
builder.Services.AddTransient<IRedisService, BaseRedisCache>();

//multi cache(in-memory + redis)
builder.Services.AddTransient<ICacheService, MultiCache>();

builder.Services.AddTransient<SessionRepository>();
builder.Services.AddTransient<TokenRepository>();
builder.Services.AddTransient<UserRepository>();

builder.Services.AddTransient<TokenValidator>();
builder.Services.AddTransient<SessionValidator>();
builder.Services.AddTransient<UserEntryValidator>();

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
