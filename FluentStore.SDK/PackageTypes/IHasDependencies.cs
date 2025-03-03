using System.Collections.Generic;
using System.IO;

namespace FluentStore.SDK.PackageTypes;

public interface IHasDependencies
{
    public List<FileSystemInfo> DependencyDownloadItems { get; set; }
}
