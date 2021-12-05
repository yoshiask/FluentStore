using System;

namespace StoreLib.Models
{
    public class PackageInstance : IComparable<PackageInstance>
    {
        public string PackageMoniker;
        private string[] monikerSplit;
        public Uri PackageUri;
        public PackageType PackageType;
        public string Digest;

        public PackageInstance(string packageMoniker, Uri packageUri, PackageType packageType, string digest)
        {
            this.PackageMoniker = packageMoniker;
            monikerSplit = packageMoniker.Split('_');
            this.PackageUri = packageUri;
            this.PackageType = packageType;
            this.Digest = digest;
        }

        public string PackageFamily
        {
            get
            {
                try
                {
                    return monikerSplit[0];
                }
                catch { return null; }
            }
        }

        public Version Version
        {
            get
            {
                try
                {
                    return new Version(monikerSplit[1]);
                }
                catch { return null; }
            }
        }

        public string Architecture
        {
            get
            {
                try
                {
                    return monikerSplit[2];
                }
                catch { return null; }
            }
        }

        public string PublisherId
        {
            get
            {
                try
                {
                    return monikerSplit[monikerSplit.Length - 1];
                }
                catch { return null; }
            }
        }

        int IComparable<PackageInstance>.CompareTo(PackageInstance other)
        {
            return Version.CompareTo(other.Version);
        }
    }
}
