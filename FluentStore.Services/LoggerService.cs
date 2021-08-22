using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace FluentStore.Services
{
    public class LoggerService
    {
        /// <summary>
        /// Logs a message to the debug output.
        /// </summary>
        [Conditional("DEBUG")]
        public void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.WriteLine($"{Path.GetFileName(filePath)}_{memberName}() [{lineNumber:D4}] : {message}");
        }

        /// <summary>
        /// Log a non-fatal exception to the debug output.
        /// </summary>
        [Conditional("DEBUG")]
        public void Warn(Exception ex, string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.WriteLine("=== WARN ===");
            Log(message, filePath, memberName, lineNumber);
            Debug.WriteLine(ex.ToString());
            Debug.WriteLine("=== ========= ===");
        }

        /// <summary>
        /// Log a fatal exception to the debug output.
        /// </summary>
        [Conditional("DEBUG")]
        public void UnhandledException(Exception ex, string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.WriteLine("=== FATAL EXCEPTION ===");
            Log(message, filePath, memberName, lineNumber);
            Debug.WriteLine(ex.ToString());
            Debug.WriteLine("=== =============== ===");
        }
    }
}
