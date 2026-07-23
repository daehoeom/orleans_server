using ApiServer.Middleware;
using Database.Db;
using Database.Redis;
using Database.Redis.RedisSet;
using GrainLibrary.Logging;
using Serilog;

Log.Logger = LoggerBootstrap.CreateBootstrapLogger("ApiServer");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddSingleton(sp => new PlayerAccessTokenSet(sp.GetRequiredService<RedisService>().GetDatabase(0)));

var app = builder.Build();

var databaseService = app.Services.GetRequiredService<DatabaseService>();
var redisService = app.Services.GetRequiredService<RedisService>();
await databaseService.CheckConnectionAsync();
await redisService.ConnectAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseMiddleware<RequestLoggingMiddleware>();
// app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

try
{
    app.Run();
}
finally
{
    await Log.CloseAndFlushAsync();
}