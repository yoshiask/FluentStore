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
---        | :---:
| ✅      | Available in latest release
| ☑       | Available in next release
| ❌      | Not available at this time

| Source Name       | Search | Download | Install
---                 | :---:  | :---:    | :---:
| Microsoft Store   | ✅     | ✅      | ✅  |
| WinGet            | ✅     | ✅      | ✅  |
| GitHub Releases   | ❌     | ☑       | ☑   |
| Chocolatey        | ❌     | ❌      | ❌  |
| UWP Community     | ❌     | ✅¹     | ✅¹ |

1.  UWP Community projects do not have direct download links. Fluent Store will attempt to follow the link
    and download/install it if recognized. If not, the link is opened in the default browser.

## Where to download?
- [GitHub Releases](https://github.com/yoshiask/FluentStore/releases)
