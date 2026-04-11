using AdditionalHelpers.ServiceRealizations;
using Microsoft.AspNetCore.Http.Features;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SettingsHelper;
using SpoofFileParser;
using SpoofFileParser.FileMetadataParser;
using SpoofFileParser.FileMetadataParser.Audio;
using SpoofFileService;
using SpoofFileService.Models;
using SpoofFileService.ServiceRealizations;
using SpoofFileService.ServiceRealizations.Repositories;
using SpoofFileService.ServiceRealizations.Validators;
using SpoofFileService.Services;
using SpoofFileService.Services.Repositories;
using SpoofFileService.Services.Validators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.SetBaseSettingsWithFactory<SpoofFileServiceContext>();
builder.Services.Configure<FileSettings>(
    builder.Configuration.GetSection("FileSettings"));

builder.Services.AddTransient<IFileRepository, FileRepository>();
builder.Services.AddTransient<IExtensionRepository, ExtensionRepository>();
builder.Services.AddTransient<IFilePublisherService, FilePublisherService>();

builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IExtensionService, ExtensionService>();

builder.Services.AddTransient<IFileValidator, FileValidator>();
builder.Services.AddTransient<IExtensionValidator, ExtensionDTOValidator>();
builder.Services.AddTransient<IExtensionValidator, ExtensionDTOValidator>();

builder.Services.AddTransient<IFileWorkerService, LocalFileWorkerService>();
builder.Services.AddTransient<IFingerprintService, FingerprintService>();
builder.Services.AddTransient<IFileTokenService, FileTokenService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1024L * 1024 * 1024 * 10; // 10 ГБ
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024L * 1024 * 1024 * 10; // 500 МБ
});
ParserFactory factory = new(
    new()
    {
        [FileType.Image] =
            new ImageMetadataParser(new()
            {
                ["jpeg"] = new(0, 2, 2, 2, true, [[0xFF, 0xC0], [0xFF, 0xC1], [0xFF, 0xC2]], 5),
                ["jpg"] = new(0, 2, 2, 2, true, [[0xFF, 0xC0], [0xFF, 0xC1], [0xFF, 0xC2]], 5),
                ["png"] = new(16, 4, 20, 4, true),
                ["bmp"] = new(18, 4, 22, 4, false),
                ["gif"] = new(6, 2, 8, 2, false),
            }),
        [FileType.Audio] = new AudioMetadataParser(new()
        {
            ["mp3"] = new Mp3MetadataParser()
        })
    },
    new FileMetadataParser());
JsonSerializerService serializer = new();

builder.Services.AddSingleton<IFileClassifier>(new FileClassifier(factory, await serializer.Deserialize<ExtensionRoadMap[]>(File.OpenRead("startup\\FileExtensions.json"))));

builder.Services.AddSingleton(
    builder.Configuration.GetSection("TokenHeader")
    .Get<TokenHeaderCover>()!);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
    app.UseSwagger();
    app.MapSwagger();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
