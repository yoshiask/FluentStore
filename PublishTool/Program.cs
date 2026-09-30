using PublishTool.Commands;
using Spectre.Console;

// List architectures
string[] archs = ["x64", "x86"];

var argParser = Meziantou.Framework.CommandLineParser.Current;

if (args.Length > 0 && !args[0].StartsWith('-'))
{
    // Command
    var command = args[0].Trim().ToUpperInvariant();
    
    switch (command)
    {
        case "PLUGINBUILD":
            await Plugin.BuildPlugins(argParser);
            break;

        case "RELEASE":
            await Release.PublishAsync(argParser);
            break;

        default:
            AnsiConsole.WriteLine($"Unrecognized command '{command}'.");
            break;
    };
}
