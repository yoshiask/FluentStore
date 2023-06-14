using System;
using UnifiedUpdatePlatform.Services.WindowsUpdate;

namespace StoreDownloader;

public class ApplicationFile
{
    public string UpdateID;
    public string RevisionNumber;
    public string Token;
    public CTAC CTAC;

    public string Digest;
    public string Filename;

    public DateTime Modified;
    public string[] Targets;

    public UpdateData UpdateData;
    public string Size;
}
