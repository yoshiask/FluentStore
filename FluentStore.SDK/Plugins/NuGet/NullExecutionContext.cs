using NuGet.ProjectManagement;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.NuGet;

internal class NullExecutionContext : ExecutionContext
{
    public override Task OpenFile(string fullPath) => Task.CompletedTask;
}
