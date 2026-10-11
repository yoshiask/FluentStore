using CliWrap;
using Spectre.Console;

namespace PublishTool.Support;

internal static class CliWrapHelpers
{
    public static (PipeTarget outPipe, PipeTarget errPipe) CreateLoggingPipeTargets(Action<FormattableString> write, bool verbose, string? logOutputDirectory, string? logName)
    {
        PipeTarget outPipe = PipeTarget.Null;
        PipeTarget errPipe = PipeTarget.Create(async (s, t) =>
        {
            StreamReader reader = new(s);
            char[] textBuffer = new char[1024];
            while (true)
            {
                var textLength = await reader.ReadAsync(textBuffer, t);
                if (textLength is 0)
                    break;
                write($"[red]{new string(textBuffer[..textLength])}[/]");
            }
        });

        if (verbose)
        {
            outPipe = PipeTarget.Create(async (s, t) =>
            {
                StreamReader reader = new(s);
                char[] textBuffer = new char[1024];
                while (true)
                {
                    var textLength = await reader.ReadAsync(textBuffer, t);
                    if (textLength is 0)
                        break;
                    write($"{new string(textBuffer[..textLength])}");
                }
            });
        }
        if (logOutputDirectory is not null)
        {
            var logOutPath = Path.Combine(logOutputDirectory, $"{logName}_out.txt");
            var logErrPath = Path.Combine(logOutputDirectory, $"{logName}_err.txt");

            outPipe = PipeTarget.Merge(outPipe, PipeTarget.ToFile(logOutPath));
            errPipe = PipeTarget.Merge(errPipe, PipeTarget.ToFile(logErrPath));
        }

        return (outPipe, errPipe);
    }

    public static Command WithStandardLogPipes(this Command cmd, IAnsiConsole console, bool verbose, string? logOutputDirectory, string? logName)
    {
       var (outPipe, errPipe) = CreateLoggingPipeTargets(console.MarkupInterpolated, verbose, logOutputDirectory, logName);
        return cmd
            .WithStandardOutputPipe(outPipe)
            .WithStandardErrorPipe(errPipe);
    }
}
