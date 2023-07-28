using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;
using System;
using System.Xml.Linq;

using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FluentStore.SDK.Plugins;

internal class FluentStoreProjectContext : INuGetProjectContext
{
    private readonly ILogger _log;

    public PackageExtractionContext PackageExtractionContext { get; set; }

    public ISourceControlManagerProvider SourceControlManagerProvider { get; }

    public ExecutionContext ExecutionContext { get; }

    public XDocument OriginalPackagesConfig { get; set; }
    public NuGetActionType ActionType { get; set; }
    public Guid OperationId { get; set; }

    public FluentStoreProjectContext(string pluginDirectory, ILogger log)
    {
        OriginalPackagesConfig = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><packages></packages>");
        PackageExtractionContext = new(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.None, ClientPolicyContext.GetClientPolicy(NullSettings.Instance, NullLogger.Instance), NullLogger.Instance);
        SourceControlManagerProvider = new FluentStoreSourceControlManagerProvider(pluginDirectory);
        ExecutionContext = new NullExecutionContext();
        _log = log;
    }

    public void Log(MessageLevel level, string message, params object[] args)
    {
        var logLevel = level switch
        {
            MessageLevel.Info => LogLevel.Information,
            MessageLevel.Warning => LogLevel.Warning,
            MessageLevel.Debug => LogLevel.Debug,
            MessageLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };

        _log.Log(logLevel, message, args);
    }

    public void Log(ILogMessage message) => _log.Log(ToLogLevel(message.Level), $"[{message.Time}] {message.FormatWithCode()}");

    public void ReportError(string message) => _log.LogError(message);

    public void ReportError(ILogMessage message) => _log.LogError(message.FormatWithCode());

    public FileConflictAction ResolveFileConflict(string message)
    {
        throw new NotImplementedException();
    }

    private LogLevel ToLogLevel(NuGet.Common.LogLevel level)
    {
        return level switch
        {
            NuGet.Common.LogLevel.Debug => LogLevel.Trace,
            NuGet.Common.LogLevel.Verbose => LogLevel.Debug,
            NuGet.Common.LogLevel.Information or
            NuGet.Common.LogLevel.Minimal => LogLevel.Information,
            NuGet.Common.LogLevel.Warning => LogLevel.Warning,
            NuGet.Common.LogLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }
}
