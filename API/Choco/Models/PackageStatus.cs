namespace Chocolatey.Models
{
    public enum PackageStatus
    {
        Unknown = -1,
        Pending,
        Ready,
        Waiting,
        Responded,
        Updated,
        Exempted,
        Approved
    }
}