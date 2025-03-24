using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scoop.Converters;

namespace Scoop.Responses;

// https://github.com/ScoopInstaller/Scoop/wiki/App-Manifests

public class Manifest
{
    /// <summary>
    /// The version of the app that this manifest installs.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// A one line string containing a short description of the program.
    /// Don’t include the name of the program, if it’s the same
    /// as the app’s filename.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// The home page for the program.
    /// </summary>
    [JsonPropertyName("homepage")]
    public string Homepage { get; set; }

    /// <summary>
    /// A string or hash of the software license for the program.
    /// </summary>
    /// <remarks>
    /// For well-known licenses, please use the identifier found
    /// at <see href="https://spdx.org/licenses"/> For other licenses, use the
    /// URL of the license, if available. Otherwise, use
    /// "Freeware", "Proprietary", "Public Domain", "Shareware",
    /// or "Unknown", as appropriate. If different files have
    /// different licenses, separate licenses with a comma (,).
    /// If the entire application is dual licensed, separate
    /// licenses with a pipe symbol (|).
    /// </remarks>
    // TODO: Deserialize non-standard licenses e.g. extras/sourcetree
    [JsonPropertyName("license")]
    public LicenseInfo? License { get; set; }

    /// <summary>
    /// If the app has 32- and 64-bit versions, architecture can
    /// be used to wrap the differences.
    /// </summary>
    [JsonPropertyName("architecture")]
    public Architecture? Architecture { get; set; }

    /// <summary>
    /// Definition of how the manifest can be updated automatically.
    /// </summary>
    /// <remarks>
    /// <seealso href="https://github.com/ScoopInstaller/Scoop/wiki/App-Manifest-Autoupdate#adding-autoupdate-to-a-manifest"/>
    /// </remarks>
    [JsonPropertyName("autoupdate")]
    public Architecture Autoupdate { get; set; }

    /// <summary>
    /// A string or array of strings of programs (executables or scripts)
    /// to make available on the user's path. 
    /// </summary>
    // TODO: Deserialize complex bin
    public JsonElement? Bin { get; set; }

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
    // TODO: Deserialize complex checkver
    [JsonPropertyName("checkver")]
    public JsonElement? CheckVer { get; set; }

    /// <summary>
    /// Runtime dependencies for the app which will be installed automatically.
    /// </summary>
    /// <remarks>
    /// See also <see cref="Suggest"/> for an alternative to <see cref="Depends"/>.
    /// </remarks>
    [JsonPropertyName("depends")]
    public List<string>? Depends { get; set; }

    /// <summary>
    /// Add this directory to the user's path (or system path if --global is used).
    /// The directory is relative to the install directory and must be inside the
    /// install directory.
    /// </summary>
    [JsonPropertyName("env_add_path")]
    public string? EnvAddPath { get; set; }

    /// <summary>
    /// Sets one or more environment variables for the user
    /// (or system if --global is used).
    /// </summary>
    [JsonPropertyName("env_set")]
    public Dictionary<string, string>? EnvSet { get; set; }

    /// <summary>
    /// If <see cref="Url"/> points to a compressed file (.zip, .7z, .tar, .gz,
    /// .lzma, and .lzh are supported), Scoop will extract just the directory
    /// specified from it.
    /// </summary>
    [JsonPropertyName("extract_dir")]
    public string? ExtractDir { get; set; }

    /// <summary>
    /// If <see cref="Url"/> points to a compressed file (.zip, .7z, .tar, .gz,
    /// .lzma, and .lzh are supported), Scoop will extract all content to the
    /// directory specified.
    /// </summary>
    [JsonPropertyName("extract_to")]
    public string? ExtractTo { get; set; }

    /// <summary>
    /// A string or array of strings with a file hash for each URL in url.
    /// Hashes are SHA256 by default, but you can use SHA512, SHA1 or MD5 by
    /// prefixing the hash string with 'sha512:', 'sha1:' or 'md5:'.
    /// </summary>
    public StringOrArray Hash { get; set; }

    /// <summary>
    /// Set to <see langword="true"/> if the installer is InnoSetup based.
    /// </summary>
    [JsonPropertyName("innosetup")]
    public bool IsInnoSetup { get; set; }

    /// <summary>
    /// Instructions for running a non-MSI installer. 
    /// </summary>
    public Installer? Installer { get; set; }

    /// <summary>
    /// A one-line string, or array of strings, with a message to be displayed after installing the app.
    /// </summary>
    public StringOrArray Notes { get; set; }

    /// <summary>
    /// A string or array of strings of directories and files to persist inside the data directory for the app.
    /// </summary>
    public StringOrArray Persist { get; set; }

    /// <summary>
    /// A one-line string, or array of strings, of the commands to be executed after an application is installed.
    /// These can use variables like $dir, $persist_dir, and $version.
    /// </summary>
    public StringOrArray PostInstall { get; set; }

    /// <summary>
    /// Same options as <see cref="PostInstall"/>, but executed before an application is installed.
    /// </summary>
    public StringOrArray PreInstall { get; set; }

    /// <summary>
    /// Same options as <see cref="PostInstall"/>, but executed before an application is installed.
    /// </summary>
    public StringOrArray PreUninstall { get; set; }

    /// <summary>
    /// Same options as <see cref="PostInstall"/>, but executed before an application is installed.
    /// </summary>
    public StringOrArray PostUninstall { get; set; }

    /// <summary>
    /// Install as a PowerShell module in <c>~/scoop/modules</c>. 
    /// </summary>
    [JsonPropertyName("psmodule")]
    public PowerShellModuleDeclaration? PowerShellModule { get; set; }

    /// <summary>
    /// Specifies the shortcut values to make available in the startmenu.
    /// The array has to contain a executable/label pair. The third and fourth element are optional. 
    /// </summary>
    public List<List<string>>? Shortcuts { get; set; }

    /// <summary>
    /// Display a message suggesting optional apps that provide complementary features.
    /// </summary>
    public Dictionary<string, StringOrArray>? Suggest { get; set; }

    /// <summary>
    /// Same options as <see cref="Installer"/>, but the file/script is run to uninstall the application. 
    /// </summary>
    public Installer? Uninstaller { get; set; }

    /// <summary>
    /// The URL or URLs of files to download. If there's more than one URL, you can use a JSON - array, e.g.
    /// <c>"url": [ "http://example.org/program.zip", "http://example.org/dependencies.zip" ]</c>.
    /// URLs can be HTTP, HTTPS or FTP.
    /// </summary>
    public StringOrArray Url { get; set; }
}

public class Architecture
{
    [JsonPropertyName("64bit")]
    public ManifestDifference X64 { get; set; }

    [JsonPropertyName("32bit")]
    public ManifestDifference X86 { get; set; }

    [JsonPropertyName("arm64")]
    public ManifestDifference Arm64 { get; set; }
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
    [JsonPropertyName("file")]
    public string File { get; set; }

    /// <summary>
    /// A one-line string, or array of strings, of commands to be executed as
    /// an installer/uninstaller instead of <see cref="File"/>.
    /// </summary>
    [JsonPropertyName("script")]
    public StringOrArray Script { get; set; }

    /// <summary>
    /// An array of arguments to pass to the installer. Optional.
    /// </summary>
    public List<string>? Args { get; set; }

    /// <summary>
    /// <see langword="true"/> if the installer should be kept after running (for future uninstallation, as an example).
    /// If omitted or set to any other value, the installer will be deleted after running.
    /// See <see href="https://github.com/ScoopInstaller/Java/blob/master/bucket/oraclejdk.json">java/oraclejdk</see> for an example.
    /// This option will be ignored when used in an <see cref="Manifest.Uninstaller"/> directive.
    /// </summary>
    public bool Keep { get; set; }
}

public class PowerShellModuleDeclaration
{
    /// <summary>
    /// The name of the module, which should match at least one file in the extracted directory for PowerShell to recognize this as a module.
    /// </summary>
    public string Name { get; set; }
}

[JsonConverter(typeof(LicenseJsonConverter))]
public class LicenseInfo
{
    /// <summary>
    /// The SPDX identifier, or "Freeware" (free to use forever), "Proprietary" (must pay to use),
    /// "Public Domain", "Shareware" (free to try, must pay eventually), or "Unknown"
    /// (unable to determine the license), as appropriate.
    /// </summary>
    public List<List<string>> MultiLicenses { get; set; } = [["Unknown"]];

    /// <summary>
    /// For non-SPDX licenses, include a link to the license. It is acceptable to include links
    /// to SPDX licenses, as well.
    /// </summary>
    public Uri? Url { get; set; }
}

