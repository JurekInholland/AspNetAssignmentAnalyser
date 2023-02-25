using Services;
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
builder.Services.AddSingleton<SubmissionHub>();

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
