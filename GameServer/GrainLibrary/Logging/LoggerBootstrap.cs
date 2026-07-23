using Serilog;
using Serilog.Events;

namespace GrainLibrary.Logging;

public static class LoggerBootstrap
{
    public static ILogger CreateBootstrapLogger(string appName) =>
        new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Orleans", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", appName)
            .WriteTo.Async(sink => sink.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Application} {Message:lj}{NewLine}{Exception}"))
            .WriteTo.Async(sink => sink.File(
                path: $"Logs/{appName}-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Application} {Message:lj}{NewLine}{Exception}"))
            .CreateLogger();
}