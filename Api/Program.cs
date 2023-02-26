using Azure.Storage.Blobs;
using Services;
using Services.BlobStorageService;
using Services.EmailService;
using Services.FileUploadService;
using Services.SnakeTestService;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddSingleton(config);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<ISnakeTestService, SnakeTestService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

builder.Services.AddSingleton<SubmissionHub>();

var connectionString = config.GetValue<string>("AzureWebJobsStorage");
if (connectionString == null) throw new Exception("AzureWebJobsStorage variable is not set");
builder.Services.AddSingleton(_ => new BlobServiceClient(connectionString));

builder.Services.Configure<AppConfig>(cfg =>
{
    cfg.SendGridApiKey = config.GetValue<string>("SendGridApiKey") ?? string.Empty;
    cfg.SendGridFromEmail = config.GetValue<string>("SendGridFromEmail") ?? string.Empty;
    cfg.SendGridToEmail = config.GetValue<string>("SendGridToEmail") ?? string.Empty;
    cfg.UserHeaderKey = config.GetValue<string>("UserHeaderKey") ?? "x-userid";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapHub<SubmissionHub>("/api/signalr");

await app.RunAsync();
