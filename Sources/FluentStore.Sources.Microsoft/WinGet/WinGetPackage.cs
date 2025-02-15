﻿using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using FluentStore.SDK;
using FluentStore.SDK.Models;

using SDKInstallerType = FluentStore.SDK.Models.InstallerType;

namespace FluentStore.Sources.Microsoft.WinGet
{
    public class WinGetPackage : PackageBase
    {
        private readonly IWinGetImplementation _winget;

        public WinGetPackage(PackageHandlerBase packageHandler, object model)
            : this(WinGetProxyHandler.Implementation, packageHandler, model)
        {
        }

        internal WinGetPackage(IWinGetImplementation winget, PackageHandlerBase packageHandler, object model) : base(packageHandler)
        {
            _winget = winget;
            Model = model;
        }

        public override Task<bool> CanDownloadAsync() => _winget.CanDownloadAsync(this);

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            if (!Status.IsAtLeast(PackageStatus.BasicDetails))
                return null;

            folder ??= StorageHelper.GetTempDirectory().CreateSubdirectory(StorageHelper.PrepUrnForFile(Urn));

            DownloadItem = await _winget.DownloadAsync(this, folder);
            if (DownloadItem is not null)
                IsDownloaded = true;

            return DownloadItem;
        }

        public override Task<ImageBase> CacheAppIcon() => Task.FromResult<ImageBase>(TextImage.CreateFromName(Title));

        public override async Task<ImageBase> CacheHeroImage()
        {
            return null;
        }

        public override async Task<List<ImageBase>> CacheScreenshots()
        {
            return new List<ImageBase>();
        }

        public override async Task<bool> InstallAsync()
        {
            bool isSuccess = await _winget.InstallAsync(this);

            if (isSuccess)
                IsInstalled = true;

            return isSuccess;
        }

        public override async Task<bool> CanLaunchAsync()
        {
            if (HasPackageFamilyName)
                return await PackagedInstallerHelper.IsInstalled(PackageFamilyName);

            return false;
        }

        public override async Task LaunchAsync()
        {
            switch (Type.Reduce())
            {
                case SDKInstallerType.Msix:
                    Guard.IsTrue(HasPackageFamilyName, nameof(HasPackageFamilyName));
                    await PackagedInstallerHelper.Launch(PackageFamilyName);
                    break;
            }
        }

        private string _WinGetId;
        public string WinGetId
        {
            get => _WinGetId;
            set => SetProperty(ref _WinGetId, value);
        }

        private string _PackageFamilyName;
        public string PackageFamilyName
        {
            get => _PackageFamilyName;
            set => SetProperty(ref _PackageFamilyName, value);
        }
        public bool HasPackageFamilyName => PackageFamilyName != null;

        private Link _SupportUrl;
        public Link SupportUrl
        {
            get => _SupportUrl;
            set => SetProperty(ref _SupportUrl, value);
        }
    }
}
