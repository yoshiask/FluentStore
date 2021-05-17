using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FluentStore.Services
{
    public class LoggerService
    {
        /// <summary>
        /// Simple logging to debug console for debug builds.
        /// </summary>
        [Conditional("DEBUG")]
        public void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.WriteLine($"{Path.GetFileName(filePath)}_{memberName}() [{lineNumber:D4}] : {message}");
        }
    }
}
