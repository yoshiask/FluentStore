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
using NuGetLogLevel = NuGet.Common.LogLevel;

namespace FluentStore.SDK.Plugins.NuGet;

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

    public void Log(ILogMessage message) => _log.Log(ToLogLevel(message.Level), "[{Timestamp}] {Code}: {Message}", message.Time, message.Code.GetName() ?? "", message.Message);

    public void ReportError(string message) => _log.LogError("{Message}", message);

    public void ReportError(ILogMessage message) => _log.LogError("{Message}", message.FormatWithCode());

    public FileConflictAction ResolveFileConflict(string message)
    {
        throw new NotImplementedException();
    }

    private static LogLevel ToLogLevel(NuGetLogLevel level)
    {
        return level switch
        {
            NuGetLogLevel.Debug => LogLevel.Trace,
            NuGetLogLevel.Verbose => LogLevel.Debug,
            NuGetLogLevel.Information or
            NuGetLogLevel.Minimal => LogLevel.Information,
            NuGetLogLevel.Warning => LogLevel.Warning,
            NuGetLogLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }
}
