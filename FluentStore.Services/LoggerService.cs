using OwlCore.Diagnostics;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace FluentStore.Services
{
    public class LoggerService : IDisposable
    {
        private readonly StreamWriter m_logWriter;

        public LogLevel LogLevel { get; set; } = LogLevel.Error;

        public LoggerService(Stream logFile = null) : this(new StreamWriter(logFile))
        {
        }

        public LoggerService(StreamWriter logWriter = null)
        {
            m_logWriter = logWriter;
        }

        /// <summary>
        /// Logs a message to the debug output.
        /// </summary>
        public void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogLevel.CompareTo(LogLevel.Trace) <= 0)
                WriteLine($"{Path.GetFileName(filePath)}_L{lineNumber:D}_{memberName}() : {message}");
        }

        /// <summary>
        /// Log a non-fatal exception to the debug output.
        /// </summary>
        public void Warn(Exception ex, string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogLevel.CompareTo(LogLevel.Warning) > 0)
                return;

            WriteLine("=== WARN ===");
            Log(message, filePath, memberName, lineNumber);
            WriteLine(ex.ToString());
            WriteLine("=== ==== ===");
        }

        /// <summary>
        /// Log a fatal exception to the debug output.
        /// </summary>
        public void UnhandledException(Exception ex, LogLevel errorLevel, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogLevel.CompareTo(errorLevel) > 0)
                return;

            WriteLine($"=== {errorLevel} EXCEPTION ===");
            WriteLine($"{ex.GetType().Name}: {ex.Message}");
            WriteLine(ex.StackTrace);
            WriteLine("=== =============== ===");
        }

        private void WriteLine(string line)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(line);
#endif

            m_logWriter?.WriteLine(line);
            m_logWriter?.Flush();
        }

        public void Dispose()
        {
            m_logWriter?.Flush();
            m_logWriter?.Dispose();
        }
    }
}
