using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentStore.SDK.Models;

namespace FluentStore.SDK.Helpers;

public static partial class InstallerSelection
{
    private static readonly Dictionary<Architecture, string[]> architecture_strings = new()
    {
        {
            Architecture.x64,
            new[] { "x64", "amd64", /*"x86_64"*/ }
            // x86_64 has an underscore, and therefore has to be handled differently
        },
        {
            Architecture.x86,
            new[] { /*"x86",*/ "i386", "i686" }
            // "x86" is special, see above comment
        },
        {
            Architecture.Arm,
            new[] { "arm", "arm32", "aarch32" }
        },
        {
            Architecture.Arm64,
            new[] { "arm64", "aarch64" }
        },
    };

    private static readonly char[] separator_chars = [' ', '_', '-', '+', '@', '!', '.'];

    public static IOrderedEnumerable<RankedInstaller<T>> FilterAndRank<T>(this IEnumerable<T> installers, Func<T, string> filenameSelector, string packageTitle, Architecture arch = Architecture.Unknown)
    {
        if (arch is Architecture.Unknown)
            arch = Win32Helper.GetSystemArchitecture();

        return installers
            // Project into filename, title, and item
            .Select(installer => new Item<T>(installer, filenameSelector(installer)))
            
            // Exclude mismatched architectures, but keep assets that don't specify an architecture
            .Where(a =>
            {
                List<string> parts = a.Filename.Split(separator_chars).ToList();
                int x86Idx = parts.IndexOf("x86");
                if (x86Idx != -1)
                {
                    // Contains x86
                    bool isNextPart64 = Amd64Regex().IsMatch(parts[x86Idx + 1]);
                    return (arch == Architecture.x86 && !isNextPart64)
                        || (arch == Architecture.x64 && isNextPart64);
                }

                // Check for direct match with arch
                if (a.Filename.Contains(StringComparison.InvariantCultureIgnoreCase, architecture_strings[arch]))
                    return true;

                // Check if neutral
                return !architecture_strings.Any(e => a.Filename.Contains(StringComparison.InvariantCultureIgnoreCase, e.Value));
            })


            // Rank installers by file type and title
            .Select(a => new RankedInstaller<T>(a.Installer, Rank(a.Filename, packageTitle)))
            .OrderBy(r => r.Rank);
    }

    /// <summary>
    /// Ranks an installer with <paramref name="filename"/> based on the file type and <paramref name="packageTitle"/>.
    /// <para>
    /// Lower is better.
    /// </para>
    /// </summary>
    public static int Rank(string filename, string packageTitle)
    {
        int extIdx = filename.LastIndexOf('.');
        if (extIdx < 0) return int.MaxValue;

        string ext = filename[(extIdx + 1)..];
        int rank = ext.ToUpperInvariant() switch
        {
            "APPINSTALLER" => 0,
            "MSIXBUNDLE" => 1,
            "APPXBUNDLE" => 2,
            "MSIX" => 3,
            "APPX" => 4,
            "MSI" => 5,
            "EXE" => 6,
            "ZIP" => 7,

            // Default to MaxInt - 10, so if the asset contains the name of the repo, we don't overflow
            _ => 0x7FFFFFF5
        };

        if (!filename.Contains(packageTitle, StringComparison.InvariantCultureIgnoreCase))
            rank += 10;
        
        return rank;
    }

    public static IOrderedEnumerable<RankedInstaller<T>> FilterAndRankByArchitecture<T>(IEnumerable<T> installers, Func<T, Architecture> archSelector, Architecture sysArch = Architecture.Unknown)
    {
        if (sysArch is Architecture.Unknown)
            sysArch = Win32Helper.GetSystemArchitecture();

        var sysArchReduced = sysArch.Reduce();
        var sysArchBitness = sysArchReduced.GetBitnessInt();

        return installers
            .Select(installer =>
            {
                var installerArch = archSelector(installer);
                int rank = int.MaxValue;

                // Always accept exact matches
                if (installerArch == sysArch)
                {
                    rank = 0;
                }

                // Allow a 32-bit installer to be chosen for 64-bit systems when necessary
                else if (installerArch.Reduce() == sysArchReduced && installerArch.GetBitnessInt() < sysArchBitness)
                {
                    rank = 1;
                }

                // Fall back to neutral
                else if (installerArch is Architecture.Neutral)
                {
                    rank = 2;
                }

                return new RankedInstaller<T>(installer, rank);
            })
            .OrderBy(r => r.Rank);
    }

    private record Item<T>(T Installer, string Filename);

    [GeneratedRegex("(x|amd)?64")]
    private static partial Regex Amd64Regex();
}

public record RankedInstaller<T>(T Installer, int Rank);
