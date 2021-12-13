using System;
using System.Collections.Generic;
using System.Text;

//Ported from https://github.com/Microsoft/Telemetry-Client-for-Android/blob/master/AndroidCll/src/main/java/com/microsoft/cll/android/CorrelationVector.java

/*
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace StoreLib.Utilities
{
    internal class CorrelationVector
    {

        private string baseVector;
        private int currentVector;

        private readonly string base64CharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private readonly int id0Length = 16;
        bool isInitialized = false;

        internal enum Settings
        {
            SYNCREFRESHINTERVAL,
            QUEUEDRAININTERVAL,
            SNAPSHOTSCHEDULEINTERVAL,
            MAXEVENTSIZEINBYTES,
            MAXEVENTSPERPOST,
            SAMPLERATE,
            MAXFILESSPACE,
            UPLOADENABLED,
            PERSISTENCE,
            LATENCY,
            HTTPTIMEOUTINTERVAL,
            THREADSTOUSEWITHEXECUTOR,
            MAXCORRELATIONVECTORLENGTH,
            MAXCRITICALCANADDATTEMPTS,
            MAXRETRYPERIOD,
            BASERETRYPERIOD,
            CONSTANTFORRETRYPERIOD,
            NORMALEVENTMEMORYQUEUESIZE,
            CLLSETTINGSURL,
            HOSTSETTINGSETAG,
            CLLSETTINGSETAG,
            VORTEXPRODURL
        }

        internal void Init()
        {
            baseVector = SeedCorrelationVector();
            currentVector = 1;
            isInitialized = true;
        }

        protected static int getCllSettingsAsInt(Settings setting)
        {
            return (int)setting;
        }

        private bool CanExtend()
        {
            int vectorSize = (int)Math.Floor(Math.Log10(currentVector) + 1);

            if (baseVector.Length + 1 + vectorSize + 1 + 1 > getCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH))
            {
                return false;
            }

            return true;
        }

        /**
         * Checks to see if we can increment the current vector
         */
        private bool CanIncrement(int newVector)
        {
            if (newVector - 1 == Int32.MaxValue)
            {
                return false;
            }
            int vectorSize = (int)Math.Floor(Math.Log10(newVector) + 1);

            // Get the length of the existing string + length of the new extension + the length of the dot
            if (baseVector.Length + vectorSize + 1 > getCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH))
            {
                return false;
            }

            return true;
        }

        /**
         * Adds another vector
         */
        internal string Extend()
        {
            if (!isInitialized)
            {
                Init();
            }

            if (CanExtend())
            {
                baseVector = GetValue();
                currentVector = 1;
            }

            return GetValue();
        }

        /**
         * Returns the Current Correlation Vector
         */
        internal String GetValue()
        {
            if (!isInitialized)
            {
                return null;
            }

            return baseVector + "." + currentVector;
        }

        /**
         * Increments the current vector
         */
        internal string Increment()
        {
            if (!isInitialized)
            {
                Init();
            }

            int newVector = currentVector + 1;
            // Check if we can increment
            if (CanIncrement(newVector))
            {
                currentVector = newVector;
            }

            return GetValue();
        }

        /**
         * Checks to see if the correlation vector is valid
         */
        bool IsValid(string vector)
        {
            if (vector.Length > getCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH))
            {
                return false;
            }

            string validationPattern = "^[" + base64CharSet + "]{16}(.[0-9]+)+$";
            if (vector != validationPattern)
            {
                return false;
            }

            return true;
        }

        /**
         * Randomly generates a string for the base vector
         */
        private string SeedCorrelationVector()
        {
            string result = "";

            Random r = new Random();
            for (int i = 0; i < id0Length; i++)
            {
                result += base64CharSet[(r.Next(base64CharSet.Length))];
            }

            return result;
        }

        /**
         * Sets the base and current vector values
         */
        internal void SetValue(string vector)
        {
            if (IsValid(vector))
            {
                int lastDot = vector.LastIndexOf(".");
                baseVector = vector.Substring(0, lastDot);
                currentVector = Int32.Parse(vector.Substring(lastDot + 1));
                isInitialized = true;
            }
            else
            {
                throw new Exception("Cannot set invalid correlation vector value");
            }
        }
    }
}
