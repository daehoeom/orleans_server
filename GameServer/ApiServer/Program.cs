using Database.Db;
using Database.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<RedisService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

var databaseService = app.Services.GetRequiredService<DatabaseService>();
var redisService = app.Services.GetRequiredService<RedisService>();
await databaseService.CheckConnectionAsync();
await redisService.ConnectAsync();

app.Run();