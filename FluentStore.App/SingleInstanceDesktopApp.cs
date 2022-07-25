// https://gist.githubusercontent.com/wbokkers/af326da5391bdb13b58529e720540178/raw/8d4b806a439fd0468bbeb632dd0764bf99125da9/SingleInstanceDesktopApp.cs

using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace FluentStore
{
    public class SingleInstanceLaunchEventArgs : EventArgs
    {
        public SingleInstanceLaunchEventArgs(string arguments, bool isFirstLaunch, bool isFirstInstance)
        {
            Arguments = arguments;
            IsFirstLaunch = isFirstLaunch;
            IsFirstInstance = isFirstInstance;
        }
        public string Arguments { get; private set; } = "";
        public bool IsFirstLaunch { get; private set; }
        public bool IsFirstInstance { get; private set; }
    }

    public sealed class SingleInstanceDesktopApp : IDisposable
    {
        private readonly string _mutexName = "";
        private readonly string _pipeName = "";
        private readonly object _namedPiperServerThreadLock = new();
        private readonly LoggerService _log = Ioc.Default.GetService<LoggerService>();

        private bool _isDisposed = false;
        private bool _isFirstInstance;

        private Mutex? _mutexApplication;
        private NamedPipeServerStream? _namedPipeServerStream;

        public event EventHandler<SingleInstanceLaunchEventArgs>? Launched;

        public SingleInstanceDesktopApp(string appId)
        {
            _mutexName = "MUTEX_" + appId;
            _pipeName = "PIPE_" + appId;
        }

        public void Launch(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                // The arguments from LaunchActivatedEventArgs can be empty, when
                // the user specified arguments (e.g. when using an execution alias). For this reason we
                // alternatively check for arguments using a different API.
                var argList = Environment.GetCommandLineArgs();
                if (argList.Length > 1)
                {
                    arguments = string.Join(' ', argList.Skip(1));
                }
            }

            if (IsFirstApplicationInstance())
            {
                CreateNamedPipeServer();
                Launched?.Invoke(this, new SingleInstanceLaunchEventArgs(arguments, isFirstLaunch: true, isFirstInstance: _isFirstInstance));
            }
            else
            {
                SendArgumentsToRunningInstance(arguments);

                //Process.GetCurrentProcess().Kill();
                App.Current.Exit();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _namedPipeServerStream?.Dispose();
            _mutexApplication?.Dispose();
        }

        private bool IsFirstApplicationInstance()
        {
            // Allow for multiple runs but only try and get the mutex once
            if (_mutexApplication == null)
            {
                _mutexApplication = new Mutex(true, _mutexName, out _isFirstInstance);
            }

            return _isFirstInstance;
        }

        /// <summary>
        /// Starts a new pipe server if one isn't already active.
        /// </summary>
        private void CreateNamedPipeServer()
        {
            _namedPipeServerStream = new NamedPipeServerStream(
                _pipeName, PipeDirection.In,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0);

            _namedPipeServerStream.BeginWaitForConnection(OnNamedPipeServerConnected, _namedPipeServerStream);
        }

        private void SendArgumentsToRunningInstance(string arguments)
        {
            try
            {
                using var namedPipeClientStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
                namedPipeClientStream.Connect(3000); // Maximum wait 3 seconds
                using var sw = new StreamWriter(namedPipeClientStream);
                sw.Write(arguments);
                sw.Flush();
            }
            catch (Exception)
            {
                // Error connecting or sending
            }
        }

        private void OnNamedPipeServerConnected(IAsyncResult asyncResult)
        {
            try
            {
                if (_namedPipeServerStream == null)
                    return;

                _namedPipeServerStream.EndWaitForConnection(asyncResult);

                // Read the arguments from the pipe
                lock (_namedPiperServerThreadLock)
                {
                    using var sr = new StreamReader(_namedPipeServerStream);
                    var args = sr.ReadToEnd();
                    _log?.Log($"RECEIVED arguments from other instance: '{args}'");
                    Launched?.Invoke(this, new SingleInstanceLaunchEventArgs(args, isFirstLaunch: false, isFirstInstance: _isFirstInstance));
                }
            }
            catch (ObjectDisposedException)
            {
                // EndWaitForConnection will throw when the pipe closes before there is a connection.
                // In that case, we don't create more pipes and just return.
                // This will happen when the app is closed and therefore the pipe is closed as well.
                return;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                // Close the original pipe (we will create a new one each time)
                _namedPipeServerStream?.Dispose();
            }

            // Create a new pipe for next connection
            CreateNamedPipeServer();
        }
    }
}
