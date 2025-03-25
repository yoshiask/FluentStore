using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.PowerShell;
using Scoop;
using Scoop.Responses;

namespace FluentStore.Sources.Scoop;

public class ScoopPwsh : IScoopAppManager, IScoopBucketManager
{
    private const string CMDLET_SCOOP = "scoop";

    public async Task AddBucketAsync(string name, string repo = null, CancellationToken token = default)
    {
        var ps = PowerShell.Create();
        ps.AddCommand(CMDLET_SCOOP).AddArgument("bucket").AddArgument("add").AddArgument(name);

        if (repo is not null)
            ps.AddCommand(repo);

        await ps.InvokeAsync();
    }

    public Task DownloadAsync(string name, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<object> GetAppInfoAsync(string name, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<Bucket> GetBucketsAsync(CancellationToken token = default)
    {
        using var ps = CreatePwsh();
        ps.AddCommand(CMDLET_SCOOP).AddArgument("bucket").AddArgument("list");

        var psResults = await ps.InvokeAsync();

        foreach (var psResult in psResults)
        {
            Bucket bucket = new()
            {
                Name = GetValue<string>(psResult, "Name"),
                Source = GetValue<string>(psResult, "Source"),
                Updated = GetValue<DateTime>(psResult, "Updated"),
                Manifests = GetValue<int>(psResult, "Manifests"),
            };

            yield return bucket;
        }

        yield break;
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<object> GetInstalledAppsAsync(string name, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<string> GetKnownBucketsAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task InstallAsync(string name, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveBucketAsync(string name, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task UninstallAsync(string name, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(string name, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a PowerShell console with an unrestricted execution policy
    /// </summary>
    /// <returns></returns>
    private PowerShell CreatePwsh()
    {
        // Create a default initial session state and set the execution policy.
        InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;

        // Create a runspace and open it. This example uses C#8 simplified using statements
        Runspace runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        // Create a PowerShell object 
        return PowerShell.Create(runspace);
    }

    private static T GetValue<T>(PSObject psObj, string propertyName)
    {
        var psProp = psObj.Properties[propertyName];
        return (T)((PSObject)psProp.Value).BaseObject;
    }
}
