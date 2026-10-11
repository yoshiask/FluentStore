using NuGet.Common;
using Spectre.Console;

namespace PublishTool.Support;

internal class AnsiConsoleNuGetLogger(IAnsiConsole console, bool verbose) : ILogger
{
    private readonly Style _warning = new(foreground: Color.Yellow);
    private readonly Style _error = new(foreground: Color.Red);

    public void Log(LogLevel level, string data)
    {
        if (!IsEnabled(level))
            return;

        Style? style = level switch
        {
            LogLevel.Warning => (Style?)_warning,
            LogLevel.Error => (Style?)_error,
            _ => null
        };
        console.WriteLine(data, style);
    }

    public void Log(ILogMessage message)
    {
        if (!IsEnabled(message.Level))
            return;

        if (message.Level is LogLevel.Warning)
        {
            if (message.WarningLevel is WarningLevel.Important or WarningLevel.Severe)
                console.WriteLine($"[{message.WarningLevel}] {message.Message}", _warning);
            else
                console.WriteLine(message.Message, _warning);
        }
        else if (message.Level is LogLevel.Error)
        {
            console.WriteLine(message.Message, _error);
        }
        else
        {
            console.WriteLine(message.Message);
        }
    }

    public Task LogAsync(LogLevel level, string data)
    {
        Log(level, data);
        return Task.CompletedTask;
    }

    public Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }

    public void LogDebug(string data) => Log(LogLevel.Debug, data);

    public void LogError(string data) => Log(LogLevel.Error, data);

    public void LogInformation(string data) => Log(LogLevel.Information, data);

    public void LogInformationSummary(string data) => Log(LogLevel.Information, data);

    public void LogMinimal(string data) => Log(LogLevel.Minimal, data);

    public void LogVerbose(string data) => Log(LogLevel.Verbose, data);

    public void LogWarning(string data) => Log(LogLevel.Warning, data);

    private bool IsEnabled(LogLevel level) => (verbose && level >= LogLevel.Verbose) || level >= LogLevel.Minimal;
}
