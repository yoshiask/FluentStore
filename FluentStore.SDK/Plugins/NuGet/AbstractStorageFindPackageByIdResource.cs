// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentStore.SDK.Downloads;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using OwlCore.ComponentModel;
using OwlCore.Storage;

namespace FluentStore.SDK.Plugins.NuGet;

/// <summary>
/// A <see cref="FindPackageByIdResource" /> for all file systems that can be abstracted
/// via OwlCore.Storage.
/// </summary>
public class AbstractStorageFindPackageByIdResource : FindPackageByIdResource
{
    private readonly IFile _file;
    private readonly ConcurrentDictionary<string, AsyncLazy<SortedDictionary<NuGetVersion, PackageInfo>>> _packageInfoCache =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly IReadOnlyList<Uri> _baseUris;

    private const string ResourceTypeName = nameof(FindPackageByIdResource);
    private const string ThisTypeName = nameof(AbstractStorageFindPackageByIdResource);

    /// <summary>
    /// Initializes a new <see cref="AbstractStorageFindPackageByIdResource" /> class.
    /// </summary>
    /// <param name="baseUris">Base URI's.</param>
    /// <param name="file">An HTTP source.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseUris" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="baseUris" /> is empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file" /> is <see langword="null" />.</exception>
    public AbstractStorageFindPackageByIdResource(IReadOnlyList<Uri> baseUris, IFile file)
    {
        if (baseUris == null)
            throw new ArgumentNullException(nameof(baseUris));

        if (baseUris.Count < 1)
            throw new ArgumentException("One or more URIs must be specified.", nameof(baseUris));

        if (file is null)
            throw new ArgumentNullException(nameof(file));

        _baseUris = baseUris
            .Select(uri => uri.OriginalString.EndsWith("/", StringComparison.Ordinal) ? uri : new Uri(uri.OriginalString + "/"))
            .ToList();

        _file = file;
    }

    private static async Task<LazySeekStream> OpenSeekableStreamAsync(PackageInfo packageInfo, CancellationToken token = default)
    {
        var packageFile = AbstractStorageHelper.GetFileFromUrl(packageInfo.ContentUri);
        return new(await packageFile.SafeOpenStreamAsync(token: token));
    }

    /// <summary>
    /// Asynchronously gets all package versions for a package ID.
    /// </summary>
    /// <param name="id">A package ID.</param>
    /// <param name="cacheContext">A source cache context.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result (<see cref="Task{TResult}.Result" />) returns an
    /// <see cref="IEnumerable{NuGetVersion}" />.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id" />
    /// is either <see langword="null" /> or an empty string.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheContext" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger" /> <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" />
    /// is cancelled.</exception>
    public override async Task<IEnumerable<NuGetVersion>> GetAllVersionsAsync(
        string id,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException(null, nameof(id));

        if (cacheContext == null)
            throw new ArgumentNullException(nameof(cacheContext));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packageInfos = await EnsurePackagesAsync(id, cacheContext, logger, cancellationToken);

            return packageInfos.Keys;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Asynchronously gets dependency information for a specific package.
    /// </summary>
    /// <param name="id">A package id.</param>
    /// <param name="version">A package version.</param>
    /// <param name="cacheContext">A source cache context.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result (<see cref="Task{TResult}.Result" />) returns an
    /// <see cref="IEnumerable{NuGetVersion}" />.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id" />
    /// is either <see langword="null" /> or an empty string.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="version" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheContext" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger" /> <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" />
    /// is cancelled.</exception>
    public override async Task<FindPackageByIdDependencyInfo> GetDependencyInfoAsync(
        string id,
        NuGetVersion version,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException(null, nameof(id));

        if (version == null)
            throw new ArgumentNullException(nameof(version));

        if (cacheContext == null)
            throw new ArgumentNullException(nameof(cacheContext));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packageInfos = await EnsurePackagesAsync(id, cacheContext, logger, cancellationToken);

            if (packageInfos.TryGetValue(version, out PackageInfo packageInfo))
            {
                var stream = await OpenSeekableStreamAsync(packageInfo, cancellationToken);
                return GetDependencyInfo(new(stream));
            }

            return null;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Asynchronously copies a .nupkg to a stream.
    /// </summary>
    /// <param name="id">A package ID.</param>
    /// <param name="version">A package version.</param>
    /// <param name="destination">A destination stream.</param>
    /// <param name="cacheContext">A source cache context.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result (<see cref="Task{TResult}.Result" />) returns an
    /// <see cref="bool" /> indicating whether or not the .nupkg file was copied.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id" />
    /// is either <see langword="null" /> or an empty string.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="version" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="destination" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheContext" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger" /> <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" />
    /// is cancelled.</exception>
    public override async Task<bool> CopyNupkgToStreamAsync(
        string id,
        NuGetVersion version,
        Stream destination,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException(null, nameof(id));

        if (version == null)
            throw new ArgumentNullException(nameof(version));

        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        if (cacheContext == null)
            throw new ArgumentNullException(nameof(cacheContext));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packageInfos = await EnsurePackagesAsync(id, cacheContext, logger, cancellationToken);

            if (packageInfos.TryGetValue(version, out PackageInfo packageInfo))
            {
                using var source = await OpenSeekableStreamAsync(packageInfo, cancellationToken);
                await source.CopyToAsync(destination, cancellationToken: cancellationToken);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Asynchronously gets a package downloader for a package identity.
    /// </summary>
    /// <param name="packageIdentity">A package identity.</param>
    /// <param name="cacheContext">A source cache context.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result (<see cref="Task{TResult}.Result" />) returns an <see cref="IPackageDownloader" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageIdentity" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheContext" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger" /> <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" />
    /// is cancelled.</exception>
    public override async Task<IPackageDownloader> GetPackageDownloaderAsync(
        PackageIdentity packageIdentity,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (packageIdentity == null)
            throw new ArgumentNullException(nameof(packageIdentity));

        if (cacheContext == null)
            throw new ArgumentNullException(nameof(cacheContext));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        cancellationToken.ThrowIfCancellationRequested();

        var packageInfos = await EnsurePackagesAsync(packageIdentity.Id, cacheContext, logger, cancellationToken);

        if (packageInfos.TryGetValue(packageIdentity.Version, out PackageInfo packageInfo))
        {
            return new RemotePackageArchiveDownloader(null, this, packageInfo.Identity, cacheContext, logger);
        }

        return null;
    }

    /// <summary>
    /// Asynchronously check if exact package (id/version) exists at this source.
    /// </summary>
    /// <param name="id">A package id.</param>
    /// <param name="version">A package version.</param>
    /// <param name="cacheContext">A source cache context.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result (<see cref="Task{TResult}.Result" />) returns an
    /// <see cref="IEnumerable{NuGetVersion}" />.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id" />
    /// is either <see langword="null" /> or an empty string.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="version" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheContext" /> <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger" /> <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" />
    /// is cancelled.</exception>
    public override async Task<bool> DoesPackageExistAsync(
        string id,
        NuGetVersion version,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException(null, nameof(id));

        if (version == null)
            throw new ArgumentNullException(nameof(version));

        if (cacheContext == null)
            throw new ArgumentNullException(nameof(cacheContext));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packageInfos = await EnsurePackagesAsync(id, cacheContext, logger, cancellationToken);

            return packageInfos.TryGetValue(version, out var packageInfo);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private async Task<SortedDictionary<NuGetVersion, PackageInfo>> EnsurePackagesAsync(
        string id,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        AsyncLazy<SortedDictionary<NuGetVersion, PackageInfo>> result = null;

        Func<string, AsyncLazy<SortedDictionary<NuGetVersion, PackageInfo>>> findPackages =
            (keyId) => new AsyncLazy<SortedDictionary<NuGetVersion, PackageInfo>>(
                () => FindPackagesByIdAsync(
                    keyId,
                    cacheContext,
                    logger,
                    cancellationToken));

        if (cacheContext.RefreshMemoryCache)
        {
            // Update the cache
            result = _packageInfoCache.AddOrUpdate(id, findPackages, (k, v) => findPackages(id));
        }
        else
        {
            // Read the cache if it exists
            result = _packageInfoCache.GetOrAdd(id, findPackages);
        }

        return await result;
    }

    private async Task<SortedDictionary<NuGetVersion, PackageInfo>> FindPackagesByIdAsync(
        string id,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Try each base URI _maxRetries times.
        var maxTries = 3 * _baseUris.Count;
        var packageIdLowerCase = id.ToLowerInvariant();

        for (var retry = 1; retry <= maxTries; ++retry)
        {
            var baseUri = _baseUris[retry % _baseUris.Count].OriginalString;
            var uri = baseUri + packageIdLowerCase + "/index.json";

            try
            {
                var file = AbstractStorageHelper.GetFileFromUrl(uri);
                await using var stream = await file.SafeOpenStreamAsync(token: cancellationToken);
                return await ConsumeFlatContainerIndexAsync(stream, id, baseUri, cancellationToken);
            }
            catch (Exception ex) when (retry < 3)
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Retrying FindPackagesById", nameof(FindPackagesByIdAsync), uri)
                    + Environment.NewLine
                    + ExceptionUtilities.DisplayMessage(ex);
                logger.LogMinimal(message);

                if (ex.InnerException != null &&
                    ex.InnerException is IOException &&
                    ex.InnerException.InnerException != null &&
                    ex.InnerException.InnerException is System.Net.Sockets.SocketException)
                {
                    // An IO Exception with inner SocketException indicates server hangup ("Connection reset by peer").
                    // Azure DevOps feeds sporadically do this due to mandatory connection cycling.
                    // Stalling an extra <ExperimentalRetryDelayMilliseconds> gives Azure more of a chance to recover.
                    logger.LogVerbose("Enhanced retry: Encountered SocketException, delaying between tries to allow recovery");
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                }
            }
            catch (Exception ex) when (retry == 3)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    "Failed to retrieve package",
                    id,
                    uri);

                throw new FatalProtocolException(message, ex);
            }
        }

        return null;
    }

    private async Task<SortedDictionary<NuGetVersion, PackageInfo>> ConsumeFlatContainerIndexAsync(Stream stream, string id, string baseUri, CancellationToken token)
    {
        JObject doc;
        using (var reader = new StreamReader(stream))
        await using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
        {
            doc = await JObject.LoadAsync(jsonReader, token);
        }

        var streamResults = new SortedDictionary<NuGetVersion, PackageInfo>();

        var versions = doc["versions"];
        if (versions == null)
        {
            return streamResults;
        }

        foreach (var packageInfo in versions
            .Select(x => BuildModel(baseUri, id, x.ToString()))
            .Where(x => x != null))
        {
            if (!streamResults.ContainsKey(packageInfo.Identity.Version))
            {
                streamResults.Add(packageInfo.Identity.Version, packageInfo);
            }
        }

        return streamResults;
    }

    private static PackageInfo BuildModel(string baseUri, string id, string version)
    {
        var parsedVersion = NuGetVersion.Parse(version);
        var normalizedVersionString = parsedVersion.ToNormalizedString();
        string idInLowerCase = id.ToLowerInvariant();

        StringBuilder builder = new(256);

        builder.Append(baseUri);
        builder.Append(idInLowerCase);
        builder.Append('/');
        builder.Append(normalizedVersionString);
        builder.Append('/');
        builder.Append(idInLowerCase);
        builder.Append('.');
        builder.Append(normalizedVersionString);
        builder.Append(".nupkg");

        string contentUri = builder.ToString();

        return new PackageInfo
        {
            Identity = new PackageIdentity(id, parsedVersion),
            ContentUri = contentUri,
        };
    }

    private class PackageInfo
    {
        public PackageIdentity Identity { get; set; }

        public string Path { get; set; }

        public string ContentUri { get; set; }
    }
}