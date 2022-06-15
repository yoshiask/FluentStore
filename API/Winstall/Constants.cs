using Flurl;

namespace Winstall;

public static class Constants
{
    public const string WINSTALL_HOST = "https://winstall.app";

    public static Url GetWinstallApiHost(string buildId)
        => WINSTALL_HOST.AppendPathSegments("_next", "data", buildId);

    public static Url GetWinstallAppLink(string packageMoniker)
        => WINSTALL_HOST.AppendPathSegments("apps", packageMoniker);

    internal static Url GetImagePng(string img)
    {
        string filename = System.IO.Path.ChangeExtension(img, ".png");
        return Constants.WINSTALL_HOST.AppendPathSegments("assets", "apps", "fallback", filename);
    }

    internal static Url GetImageWebp(string img)
    {
        string filename = System.IO.Path.ChangeExtension(img, ".webp");
        return Constants.WINSTALL_HOST.AppendPathSegments("assets", "apps", filename);
    }
}
