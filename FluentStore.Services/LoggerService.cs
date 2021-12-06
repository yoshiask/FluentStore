using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
#if DEBUG
using Console = System.Diagnostics.Debug;
#else
using Console = System.Console;
#endif

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
            Console.WriteLine($"{Path.GetFileName(filePath)}_{memberName}() [{lineNumber:D4}] : {message}");
        }

        /// <summary>
        /// Log a non-fatal exception to the debug output.
        /// </summary>
        [Conditional("DEBUG")]
        public void Warn(Exception ex, string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Console.WriteLine("=== WARN ===");
            Log(message, filePath, memberName, lineNumber);
            Console.WriteLine(ex.ToString());
            Console.WriteLine("=== ==== ===");
        }

        /// <summary>
        /// Log a fatal exception to the debug output.
        /// </summary>
        [Conditional("DEBUG")]
        public void UnhandledException(Exception ex, string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Console.WriteLine("=== FATAL EXCEPTION ===");
            Log(message, filePath, memberName, lineNumber);
            Console.WriteLine(ex.ToString());
            Console.WriteLine("=== =============== ===");
        }
    }
}
