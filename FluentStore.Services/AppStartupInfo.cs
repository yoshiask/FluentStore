namespace FluentStore.Services;

public class AppStartupInfo
{
    public ProtocolResult LaunchResult { get; set; }

    public bool IsFirstLaunch { get; set; }

    public bool IsFirstInstance { get; set; }
}
