using System;
using System.Collections.Generic;
using System.Threading;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads;

namespace StoreDownloader;

public class DownloadProgress : Progress<GeneralDownloadProgress>
{
    private readonly Dictionary<string, FileStatus> files = new();

    private readonly Mutex mutex = new();

    public DownloadProgress() : base()
    {
    }

    public DownloadProgress(Action<GeneralDownloadProgress> handler)
        : base(handler)
    {
    }

    private static string FormatBytes(double bytes)
    {
        string[] suffix = { "B", "KB", "MB", "GB", "TB" };
        int i;
        double dblSByte = bytes;
        for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            dblSByte = bytes / 1024.0;
        }

        return $"{dblSByte:0.##}{suffix[i]}";
    }

    private static string GetProgressBarString(int perc)
    {
        if (perc < 0)
        {
            perc = 0;
        }

        if (perc > 100)
        {
            perc = 100;
        }

        int eqsLength = (int)((double)perc / 100 * 55);
        string bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
        bases = bases.Insert(28, perc + "%");
        if (perc == 100)
        {
            bases = bases[1..];
        }
        else if (perc < 10)
        {
            bases = bases.Insert(28, " ");
        }

        return "[" + bases + "]";
    }

    protected override void OnReport(GeneralDownloadProgress e)
    {
        mutex.WaitOne();

        foreach (FileDownloadStatus status in e.DownloadedStatus)
        {
            if (status == null)
            {
                continue;
            }

            bool shouldReport = !files.ContainsKey(status.File.FileName) || files[status.File.FileName] != status.FileStatus;

            if (!shouldReport)
            {
                continue;
            }

            files[status.File.FileName] = status.FileStatus;
            base.OnReport(e);

            string msg = "U";

            switch (status.FileStatus)
            {
                case FileStatus.Completed:
                    msg = "C";
                    break;
                case FileStatus.Downloading:
                    msg = "D";
                    break;
                case FileStatus.Expired:
                    msg = "E";
                    break;
                case FileStatus.Failed:
                    msg = "F";
                    break;
                case FileStatus.Verifying:
                    msg = "V";
                    break;
            }

            Console.WriteLine($"{DateTime.Now:'['HH':'mm':'ss']'}[{e.NumFilesDownloadedSuccessfully}/{e.NumFiles}][E:{e.NumFilesDownloadedUnsuccessfully}][{msg}] {status.File.FileName} ({FormatBytes(status.File.FileSize)})");
        }

        mutex.ReleaseMutex();
    }
}
