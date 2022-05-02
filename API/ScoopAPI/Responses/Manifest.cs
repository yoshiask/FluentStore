using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScoopAPI.Responses;

public class Manifest
{
    /// <summary>
    /// The version of the app that this manifest installs.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// A one line string containing a short description of the program.
    /// Don’t include the name of the program, if it’s the same
    /// as the app’s filename.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// The home page for the program.
    /// </summary>
    [JsonProperty("homepage")]
    public string Homepage { get; set; }

    /// <summary>
    /// A string or hash of the software license for the program.
    /// </summary>
    /// <remarks>
    /// For well-known licenses, please use the identifier found
    /// at <see href="https://spdx.org/licenses"/> For other licenses, use the
    /// URL of the license, if available. Otherwise, use
    /// “Freeware”, “Proprietary”, “Public Domain”, “Shareware”,
    /// or “Unknown”, as appropriate. If different files have
    /// different licenses, separate licenses with a comma (,).
    /// If the entire application is dual licensed, separate
    /// licenses with a pipe symbol (|).
    /// </remarks>
    [JsonProperty("license")]
    public string License { get; set; }

    /// <summary>
    /// If the app has 32- and 64-bit versions, architecture can
    /// be used to wrap the differences.
    /// </summary>
    [JsonProperty("architecture")]
    public Architecture Architecture { get; set; }

    /// <summary>
    /// Definition of how the manifest can be updated automatically.
    /// </summary>
    /// <remarks>
    /// <seealso href="https://github.com/ScoopInstaller/Scoop/wiki/App-Manifest-Autoupdate#adding-autoupdate-to-a-manifest"/>
    /// </remarks>
    [JsonProperty("autoupdate")]
    public Architecture Autoupdate { get; set; }

    /// <summary>
    /// A string or array of strings of programs (executables or scripts)
    /// to make available on the user's path. 
    /// </summary>
    [JsonProperty("bin")]
    public object Bin { get; set; }

    /// <summary>
    /// App maintainers and developers can use the bin/checkver tool
    /// to check for updated versions of apps. The <see cref="CheckVer"/> property
    /// in a manifest is a regular expression that can be used to
    /// match the current stable version of an app from the app's
    /// homepage. For an example, see the go manifest. If the homepage
    /// doesn't have a reliable indication of the current version, you
    /// can also specify a different URL to check—for an example see
    /// the ruby manifest.
    /// </summary>
    [JsonProperty("checkver")]
    public string CheckVer { get; set; }

    /// <summary>
    /// Runtime dependencies for the app which will be installed automatically.
    /// </summary>
    /// <remarks>
    /// See also <see cref="Suggest"/> for an alternative to <see cref="Depends"/>.
    /// </remarks>
    [JsonProperty("depends")]
    public List<string> Depends { get; set; }

    /// <summary>
    /// Add this directory to the user's path (or system path if --global is used).
    /// The directory is relative to the install directory and must be inside the
    /// install directory.
    /// </summary>
    [JsonProperty("env_add_path")]
    public string EnvAddPath { get; set; }

    /// <summary>
    /// Sets one or more environment variables for the user
    /// (or system if --global is used).
    /// </summary>
    [JsonProperty("env_set")]
    public Dictionary<string, string> EnvSet { get; set; }

    /// <summary>
    /// If <see cref="Url"/> points to a compressed file (.zip, .7z, .tar, .gz,
    /// .lzma, and .lzh are supported), Scoop will extract just the directory
    /// specified from it.
    /// </summary>
    [JsonProperty("extract_dir")]
    public string ExtractDir { get; set; }

    /// <summary>
    /// If <see cref="Url"/> points to a compressed file (.zip, .7z, .tar, .gz,
    /// .lzma, and .lzh are supported), Scoop will extract all content to the
    /// directory specified.
    /// </summary>
    [JsonProperty("extract_to")]
    public string ExtractTo { get; set; }

    /// <summary>
    /// A string or array of strings with a file hash for each URL in url.
    /// Hashes are SHA256 by default, but you can use SHA512, SHA1 or MD5 by
    /// prefixing the hash string with 'sha512:', 'sha1:' or 'md5:'.
    /// </summary>
    [JsonProperty("hash")]
    public object Hash { get; set; }

    /// <summary>
    /// Set to <see langword="true"/> if the installer is InnoSetup based.
    /// </summary>
    [JsonProperty("innosetup")]
    public bool IsInnoSetup { get; set; }

    public 
}

public class Architecture
{
    [JsonProperty("64bit")]
    public ManifestDifference X64 { get; set; }

    [JsonProperty("32bit")]
    public ManifestDifference X86 { get; set; }
}

public class ManifestDifference
{

}

public class Installer
{
    /// <summary>
    /// The installer executable file. For <see cref="Manifest.Installer"/>
    /// this defaults to the last URL downloaded. Must be specified for
    /// <see cref="Manifest.Uninstaller"/>.
    /// </summary>
    [JsonProperty("file")]
    public string File { get; set; }

    [JsonProperty("script")]
    public string Script { get; set; }
}

