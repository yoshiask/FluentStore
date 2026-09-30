using Microsoft.Build.Framework;
using Spectre.Console;

namespace PublishTool.Support;

internal class AnsiConsoleMsbuildLogger : ILogger
{
    public LoggerVerbosity Verbosity { get; set; }

    public string? Parameters { get; set; }

    public void Initialize(IEventSource eventSource)
    {
        eventSource.AnyEventRaised += EventSource_AnyEventRaised;
    }

    private void EventSource_AnyEventRaised(object sender, BuildEventArgs e)
    {
        if (e.Message is not null)
            AnsiConsole.MarkupLineInterpolated($"{e.Message}");
    }

    public void Shutdown()
    {
    }
}
