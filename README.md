![Fluent Store](.community/LogoHero_Banner.png)
![Fluent Store](.community/Hero.png?raw=true)

## What is Fluent Store?
Fluent Store is a unifying frontend for Windows app stores and package managers. You can search all repositories at once, pick out an app, and install it without ever opening a browser or command prompt. With a Fluent Store Social account, you can create collections of apps to install or share in one click.

Other features include:
- Graphical interface for [WinGet](https://github.com/microsoft/winget-cli)
- Download Microsoft Store apps without installing them
- Find Microsoft Store listings for installed apps (including some system apps)

## What package sources are available?
Several sources are currently supported, with several more planned for the future. Listed below are the available sources, along with the features they currently support. Sources that implement at least one of the three features in the table can be added to Collections.

**Legend**
| Symbol   | Description
---:       | :---
| ✅      | Available in latest release
| ☑       | Available in next release
| 🔷      | Currently in development
| ❌      | Not available at this time
|          | Not applicable

| Source Name       | Search | Download | Install | Collections | *Tracking issue*
:---                 | :---:  | :---:    | :---:   | :---:       |  :---
| Microsoft Store   | ✅     | ✅      | ✅      | ✅         |
| WinGet            | ✅     | ✅      | ✅      | ✅         |
| GitHub Releases   | ❌     | ✅      | ✅      |            |
| Chocolatey        | 🔷     | 🔷      | 🔷      |            | [#30](https://github.com/yoshiask/FluentStore/issues/30)
| UWP Community     | ❌     | ✅¹     | ✅¹     | ✅         |
| Fluent Store      | ❌     | ✅²     | ✅²     | ✅         |
| Scoop             | ❌     | ❌      | ❌      |            | [#40](https://github.com/yoshiask/FluentStore/issues/40)
| WebCatalog        | ❌     | ❌      | ❌      |            | [#85](https://github.com/yoshiask/FluentStore/issues/85)

1.  UWP Community projects have 'download' links, but they do not go directly to an installer.
    Fluent Store will attempt to follow the link and download/install it if recognized. If not, the link is opened in the default browser.

2.  Fluent Store Social only supports collections, or lists of apps from other sources. It doesn't host any packages.
    Collections can be installed by ensuring that the appropriate package source is enabled.

## Where to download?
- [GitHub Releases](https://github.com/yoshiask/FluentStore/releases)
- [IPFS Gateway](https://ipfs.askharoun.com/FluentStore/BetaInstaller/FluentStoreBeta.appinstaller)

## Frequently Asked Questions
### What are the system requirements?
Fluent Store only requires Windows 10 version 1809 (build 17763) or any version of Windows 11. Stripped down or de-bloated Windows installs are not supported.

### I've just installed Fluent Store and my Home page is blank!
This can happen when plugins are not available, possibly due to a propgram or internet error during first-time setup. The setup can be re-attempted by clicking on `Settings > Plugins > Default Plugins > Reinstall...`.
