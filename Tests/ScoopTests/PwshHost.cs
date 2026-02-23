using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentStore.Sources.Scoop;
using Xunit.Abstractions;

namespace ScoopTests;

public class PwshHost(ITestOutputHelper output)
{
    private readonly ScoopPwsh _scoop = new();

    [Fact]
    public async Task GetBucketsAsync()
    {
        await foreach (var bucket in _scoop.GetBucketsAsync())
        {
            output.WriteLine($"{bucket.Name} {bucket.Updated} {bucket.Manifests} {bucket.Source}");
        }
    }
}
