using Microsoft.Build.Framework;
using Spectre.Console;

namespace PublishTool.Support;

internal class AnsiConsoleMsbuildLogger(IAnsiConsole console) : ILogger
{
    private IEventSource? _eventSource;

    public LoggerVerbosity Verbosity { get; set; }

    public string? Parameters { get; set; }

    public void Initialize(IEventSource eventSource)
    {
        _eventSource = eventSource;
        if (Verbosity is LoggerVerbosity.Diagnostic)
        {
            eventSource.AnyEventRaised += EventSource_AnyEventRaised;
        }
        else
        {
            eventSource.ErrorRaised += EventSource_ErrorRaised;
            eventSource.BuildStarted += EventSource_AnyEventRaised;
            eventSource.BuildFinished += EventSource_AnyEventRaised;
            eventSource.TargetStarted += EventSource_AnyEventRaised;
            eventSource.TargetFinished += EventSource_AnyEventRaised;
        }
    }

    private void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
    {
        if (e.Message is not null)
            console.MarkupLineInterpolated($"[red]{e.Message}[/]");
    }

    private void EventSource_AnyEventRaised(object sender, BuildEventArgs e)
    {
        switch (e)
        {
            case TargetStartedEventArgs targetStartedEvent:
                console.WriteLine($"Building target {targetStartedEvent.TargetName} for '{targetStartedEvent.ProjectFile}'...");
                break;

            case TargetFinishedEventArgs targetFinishedEvent:
                var slug = $"Target {targetFinishedEvent.TargetName} for '{targetFinishedEvent.ProjectFile}'";
                if (targetFinishedEvent.Succeeded)
                    console.MarkupLineInterpolated($"[green]{slug} succeeded[/]");
                else
                    console.MarkupLineInterpolated($"[red]{slug} failed[/]");
                break;

            default:
                if (e.Message is not null)
                    console.MarkupLineInterpolated($"{e.Message}");
                break;
        }
    }

    public void Shutdown()
    {
        if (_eventSource is null)
            return;

        if (Verbosity is LoggerVerbosity.Diagnostic)
        {
            _eventSource.AnyEventRaised -= EventSource_AnyEventRaised;
        }
        else
        {
            _eventSource.ErrorRaised -= EventSource_ErrorRaised;
            _eventSource.BuildStarted -= EventSource_AnyEventRaised;
            _eventSource.BuildFinished -= EventSource_AnyEventRaised;
            _eventSource.TargetStarted -= EventSource_AnyEventRaised;
            _eventSource.TargetFinished -= EventSource_AnyEventRaised;
        }
    }
}
