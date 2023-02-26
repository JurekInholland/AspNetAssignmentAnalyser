using Azure.Storage.Blobs;
using Services;
using Services.BlobStorageService;
using Services.EmailService;
using Services.FileUploadService;
using Services.SnakeTestService;

var builder = WebApplication.CreateBuilder(args);

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

var connectionString = builder.Configuration.GetValue<string>("AzureWebJobsStorage");
if (connectionString != null)
    builder.Services.AddSingleton(_ => new BlobServiceClient(connectionString));

builder.Services.Configure<AppConfig>(cfg =>
{
    cfg.SendGridApiKey = builder.Configuration.GetValue<string>("SendGridApiKey") ?? string.Empty;
    cfg.SendGridFromEmail = builder.Configuration.GetValue<string>("SendGridFromEmail") ?? string.Empty;
    cfg.SendGridToEmail = builder.Configuration.GetValue<string>("SendGridToEmail") ?? string.Empty;
    cfg.UserHeaderKey = builder.Configuration.GetValue<string>("UserHeaderKey") ?? "x-userid";
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
