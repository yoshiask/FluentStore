using Newtonsoft.Json;
using OwlCore.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace FluentStore.SDK.Helpers
{
    public static class Win32Helpers
    {
        public static async Task InvokeWin32ComponentAsync(string applicationPath, string arguments = null, bool runAsAdmin = false, string workingDirectory = null)
        {
            await InvokeWin32ComponentsAsync(applicationPath.IntoList(), arguments, runAsAdmin, workingDirectory);
        }

        public static async Task InvokeWin32ComponentsAsync(IEnumerable<string> applicationPaths, string arguments = null, bool runAsAdmin = false, string workingDirectory = null)
        {
            Debug.WriteLine("Launching EXE in FullTrustProcess");

            var connection = await AppServiceConnectionHelper.Instance;
            if (connection != null)
            {
                var value = new ValueSet()
                {
                    { "Arguments", "LaunchApp" },
                    { "WorkingDirectory", workingDirectory },
                    { "Application", applicationPaths.FirstOrDefault() },
                    { "ApplicationList", JsonConvert.SerializeObject(applicationPaths) },
                };

                if (runAsAdmin)
                {
                    value.Add("Parameters", "runas");
                }
                else
                {
                    value.Add("Parameters", arguments);
                }

                await connection.SendMessageAsync(value);
            }
        }
    }
}