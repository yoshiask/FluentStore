using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using OwlCore.Storage;

namespace FluentStore.SDK.Plugins;

public class PluginStatusRecord : Dictionary<string, PluginEntry>
{
    public PluginStatusRecord()
    {
    }
    
    public PluginStatusRecord(int capacity) : base(capacity)
    {
    }
    
    public PluginStatusRecord(string[] entryLines) : base(entryLines.Length)
    {
        foreach (var line in entryLines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var entry = PluginEntry.Parse(line);
            this[entry.Id] = entry;
        }
    }

    public static PluginStatusRecord Read(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        return new PluginStatusRecord(lines);
    }

    public static async Task<PluginStatusRecord> ReadAsync(string filePath, CancellationToken token = default)
    {
        var lines = await File.ReadAllLinesAsync(filePath, token);
        return new PluginStatusRecord(lines);
    }

    public static async Task<PluginStatusRecord> ReadAsync(IFile file, CancellationToken token = default)
    {
        using var stream = await file.OpenReadAsync(token);
        using StreamReader reader = new(stream);

        PluginStatusRecord record = [];
        while (true)
        {
            token.ThrowIfCancellationRequested();

            string line = await reader.ReadLineAsync(token);
            if (string.IsNullOrWhiteSpace(line))
                return record;

            var entry = PluginEntry.Parse(line);
            record[entry.Id] = entry;
        }
    }

    public void Write(string filePath)
    {
        var lines = SerializeEntries();
        File.WriteAllLines(filePath, lines);
    }

    public async Task WriteAsync(string filePath, CancellationToken token = default)
    {
        var lines = SerializeEntries();
        await File.WriteAllLinesAsync(filePath, lines, token);
    }

    public async Task WriteAsync(IFile file, CancellationToken token = default)
    {
        using var stream = await file.OpenWriteAsync(token);
        using StreamWriter writer = new(stream);

        var lines = SerializeEntries();
        foreach (var line in lines)
        {
            token.ThrowIfCancellationRequested();

            await writer.WriteLineAsync(line);
        }
    }

    private IEnumerable<string> SerializeEntries() => Values.Select(e => e.ToString());
}
