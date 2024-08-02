using NuGet.Configuration;
using NuGet.ProjectManagement;
using System.Collections.Generic;
using System.IO;

namespace FluentStore.SDK.Plugins.NuGet;

internal class FluentStoreSourceControlManagerProvider : ISourceControlManagerProvider
{
    private readonly string _root;

    public FluentStoreSourceControlManagerProvider(string root)
    {
        _root = root;
    }

    public SourceControlManager GetSourceControlManager() => new NullSourceControlManager(_root);
}

internal class NullSourceControlManager : SourceControlManager
{
    public NullSourceControlManager(ISettings settings) : base(settings)
    {
    }

    public NullSourceControlManager(string root) : this(new Settings(root))
    {
    }

    public NullSourceControlManager() : base(NullSettings.Instance)
    {
    }

    public override Stream CreateFile(string fullPath, INuGetProjectContext nuGetProjectContext)
    {
        return File.Create(fullPath);
    }

    public override void PendAddFiles(IEnumerable<string> fullPaths, string root, INuGetProjectContext nuGetProjectContext)
    {
    }

    public override void PendDeleteFiles(IEnumerable<string> fullPaths, string root, INuGetProjectContext nuGetProjectContext)
    {
    }
}
