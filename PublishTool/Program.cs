using PublishTool.Commands;
using Spectre.Console;

var argParser = Meziantou.Framework.CommandLineParser.Current;

if (args.Length > 0 && !args[0].StartsWith('-'))
{
    // Command
    var command = args[0].Trim().ToUpperInvariant();
    
    switch (command)
    {
        case "PLUGINBUILD":
            await new Plugin(argParser, AnsiConsole.Console).BuildPluginsAsync();
            break;

        case "RELEASE":
            await new Release(argParser, AnsiConsole.Console).PublishAsync();
            break;

        default:
            AnsiConsole.WriteLine($"Unrecognized command '{command}'.");
            break;
    };
}
