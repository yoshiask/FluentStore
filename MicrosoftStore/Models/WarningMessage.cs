using System;

namespace MicrosoftStore.Models
{
    public class WarningMessage
    {
        public string Header { get; set; }
        public string Text { get; set; }
        public string Target { get; set; }

        public Uri TargetUri
        {
            get
            {
                try
                {
                    return new Uri(Target);
                }
                catch { return null; }
            }
        }
    }
}
