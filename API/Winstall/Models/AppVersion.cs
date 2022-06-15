using Flurl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Winstall.Models;

public class AppVersion
{
    /// <summary>
    /// The app version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Download links to available installers.
    /// </summary>
    public List<Url> Installers { get; set; }

    /// <summary>
    /// The type of installer.
    /// </summary>
    public string InstallerType { get; set; }
}