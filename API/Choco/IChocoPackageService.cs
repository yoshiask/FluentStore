using System;
using System.Threading.Tasks;
using Chocolatey.Models;
using NuGet.Versioning;

namespace Chocolatey;

public interface IChocoPackageService
{
    Task<bool> InstallAsync(string id, NuGetVersion? version = null, IProgress<PackageProgress>? progress = null);
    
    Task<bool> UninstallAsync(string id, NuGetVersion? version = null, IProgress<PackageProgress>? progress = null);
    
    Task<bool> UpgradeAsync(string id, IProgress<PackageProgress>? progress = null);
    
    Task<bool> UpgradeAllAsync(IProgress<PackageProgress>? progress = null);
}