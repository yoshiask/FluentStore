using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FluentStore.Services
{
    public class LoggerService : IDisposable, ILogger
    {
        private readonly StreamWriter m_logWriter;
        private readonly ITargetBlock<string[]> m_block;

        public LogLevel LogLevel { get; set; } = LogLevel.Error;

        public LoggerService(Stream logFile = null) : this(new StreamWriter(logFile))
        {
        }

        public LoggerService(StreamWriter logWriter = null)
        {
            m_logWriter = logWriter;

            m_block = new ActionBlock<string[]>(
                WriteLinesAsync,
                new ExecutionDataflowBlockOptions
                {
                    EnsureOrdered = true,
                    MaxDegreeOfParallelism = 1,
                }
            );
        }

        /// <summary>
        /// Logs a message to the debug output.
        /// </summary>
        public void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine(LogString(message, filePath, memberName, lineNumber));
        }

        /// <summary>
        /// Log a non-fatal exception to the debug output.
        /// </summary>
        public void Warn(Exception ex, string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!IsEnabled(LogLevel.Warning))
                return;

            WriteLines([
                "=== WARN ===",
                LogString(message, filePath, memberName, lineNumber),
                ex.ToString(),
                "=== ==== ==="
            ]);
        }

        /// <summary>
        /// Log a fatal exception to the debug output.
        /// </summary>
        public void UnhandledException(Exception ex, LogLevel errorLevel, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!IsEnabled(errorLevel))
                return;

            StackTrace trace = new(2, true);

            WriteLines([
                $"=== {errorLevel} EXCEPTION ===",
                $"{ex.GetType().Name}: {ex.Message}",
                ToTraceString(trace),
                "=== =============== ==="
            ]);
        }

        private void WriteLine(string line) => m_block.Post([line]);

        private void WriteLines(string[] lines) => m_block.Post(lines);

        public void Dispose()
        {
            m_block.Complete();
            m_block.Completion.Wait();

            m_logWriter?.Dispose();
        }

        private static string ToTraceString(StackTrace trace)
        {
            StringBuilder sb = new(256);
            // Passing a default string for "at" in case SR.UsingResourceKeys() is true
            // as this is a special case and we don't want to have "Word_At" on stack traces.
            string word_At = "at";
            // We also want to pass in a default for inFileLineNumber.
            string inFileLineNum = "in {0}:line {1}";
            string inFileILOffset = "in {0}:token 0x{1:x}+0x{2:x}";
            bool fFirstFrame = true;
            var frames = trace.GetFrames();
            for (int iFrameIndex = 0; iFrameIndex < frames.Length; iFrameIndex++)
            {
                StackFrame? sf = frames[iFrameIndex];
                MethodBase? mb = sf?.GetMethod();
                if (mb != null && (mb.Module.Name.StartsWith("FluentStore") ||
                                   (iFrameIndex == frames.Length - 1))) // Don't filter last frame
                {
                    // We want a newline at the end of every line except for the last
                    if (fFirstFrame)
                        fFirstFrame = false;
                    else
                        sb.AppendLine();

                    sb.Append("   ").Append(word_At).Append(' ');

                    bool isAsync = false;
                    Type? declaringType = mb.DeclaringType;
                    string methodName = mb.Name;
                    bool methodChanged = false;
                    if (declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                    {
                        isAsync = typeof(IAsyncStateMachine).IsAssignableFrom(declaringType);
                        if (isAsync || typeof(System.Collections.IEnumerator).IsAssignableFrom(declaringType))
                        {
                            //methodChanged = TryResolveStateMachineMethod(ref mb, out declaringType);
                        }
                    }

                    // if there is a type (non global method) print it
                    // ResolveStateMachineMethod may have set declaringType to null
                    if (declaringType != null)
                    {
                        // Append t.FullName, replacing '+' with '.'
                        string fullName = declaringType.FullName!;
                        for (int i = 0; i < fullName.Length; i++)
                        {
                            char ch = fullName[i];
                            sb.Append(ch == '+' ? '.' : ch);
                        }
                        sb.Append('.');
                    }
                    sb.Append(mb.Name);

                    // deal with the generic portion of the method
                    if (mb is MethodInfo mi && mi.IsGenericMethod)
                    {
                        Type[] typars = mi.GetGenericArguments();
                        sb.Append('[');
                        int k = 0;
                        bool fFirstTyParam = true;
                        while (k < typars.Length)
                        {
                            if (!fFirstTyParam)
                                sb.Append(',');
                            else
                                fFirstTyParam = false;

                            sb.Append(typars[k].Name);
                            k++;
                        }
                        sb.Append(']');
                    }

                    ParameterInfo[]? pi = null;
                    try
                    {
                        pi = mb.GetParameters();
                    }
                    catch
                    {
                        // The parameter info cannot be loaded, so we don't
                        // append the parameter list.
                    }
                    if (pi != null)
                    {
                        // arguments printing
                        sb.Append('(');
                        bool fFirstParam = true;
                        for (int j = 0; j < pi.Length; j++)
                        {
                            if (!fFirstParam)
                                sb.Append(", ");
                            else
                                fFirstParam = false;

                            string typeName = "<UnknownType>";
                            if (pi[j].ParameterType != null)
                                typeName = pi[j].ParameterType.Name;
                            sb.Append(typeName);
                            string? parameterName = pi[j].Name;
                            if (parameterName != null)
                            {
                                sb.Append(' ');
                                sb.Append(parameterName);
                            }
                        }
                        sb.Append(')');
                    }

                    if (methodChanged)
                    {
                        // Append original method name e.g. +MoveNext()
                        sb.Append('+');
                        sb.Append(methodName);
                        sb.Append('(').Append(')');
                    }

                    // source location printing
                    if (sf!.GetILOffset() != -1)
                    {
                        // If we don't have a PDB or PDB-reading is disabled for the module,
                        // then the file name will be null.
                        string? fileName = sf.GetFileName();

                        if (fileName != null)
                        {
                            // tack on " in c:\tmp\MyFile.cs:line 5"
                            sb.Append(' ');
                            sb.AppendFormat(CultureInfo.InvariantCulture, inFileLineNum, fileName, sf.GetFileLineNumber());
                        }
                        else if (true && mb.ReflectedType != null)
                        {
                            string assemblyName = mb.ReflectedType.Module.ScopeName;
                            try
                            {
                                int token = mb.MetadataToken;
                                sb.Append(' ');
                                sb.AppendFormat(CultureInfo.InvariantCulture, inFileILOffset, assemblyName, token, sf.GetILOffset());
                            }
                            catch (System.InvalidOperationException) { }
                        }
                    }

                    // Skip EDI boundary for async
                    //sf.IsLastFrameFromForeignExceptionStackTrace
                    if (false && !isAsync)
                    {
                        sb.AppendLine();
                        // Passing default for Exception_EndStackTraceFromPreviousThrow in case SR.UsingResourceKeys is set.
                        sb.Append("--- End of stack trace from previous location ---");
                    }
                }
            }

            return sb.ToString();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
                WriteLine(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel) => LogLevel.CompareTo(logLevel) <= 0;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        private async Task WriteLineAsync(string line)
        {
            if (line is null)
                return;

#if DEBUG
            Debug.WriteLine(line);
#endif

            if (m_logWriter is not null)
            {
                await m_logWriter.WriteLineAsync(line);
                await m_logWriter.FlushAsync();
            }
        }

        private async Task WriteLinesAsync(string[] lines)
        {
            foreach (var line in lines)
                await WriteLineAsync(line);
        }

        [Pure]
        private string LogString(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            return IsEnabled(LogLevel.Trace)
                ? $"{Path.GetFileName(filePath)}_L{lineNumber:D}_{memberName}() : {message}"
                : null;
        }
    }
}

/// <summary>
/// An empty scope without any logic
/// </summary>
public sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new NullScope();

    private NullScope()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
