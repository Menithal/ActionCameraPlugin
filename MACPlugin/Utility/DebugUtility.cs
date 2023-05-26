/**  Copyright 2020 Matti 'Menithal' Lahtinen

* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* 
**/
using System;
using System.IO;
using UnityEngine;

namespace MACPlugin
{
    class DebugUtility
    {
        String debugPath = "";

        float throttle = 0;
        long count = 0;

        public DebugUtility()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            debugPath = Path.Combine(documentsPath, "LIV/PluginLogs.txt");
        }
        public void Write(String type, String source, String message, bool throttleOverride = false)
        {
            throttle += Time.deltaTime;

            if (throttle > 0.25 || throttleOverride)
            {
                if (!throttleOverride)
                {
                    throttle = 0;
                }
                using (StreamWriter swrite = new StreamWriter(@debugPath, true))
                {
                    if (count > 0 && !throttleOverride)
                    {
                        swrite.WriteLine(DateTime.Now + " - WARN - DebugUtility - Throttled " + count + " Logs");
                    }

                    swrite.WriteLine(DateTime.Now + " - " + type.ToUpper() + " - " + source + ": " + message);
                }
            }
            else
            {
                count++;
            }
        }
    }

    public static class PluginLog
    {
        private static DebugUtility debugger = new DebugUtility();

        public static void Log(String source, String message)
        {
        #if DEBUG
            debugger.Write("log", source, message, true);
        #endif
        }
        public static void Debug(String source, String message)
        {
        #if DEBUG
            debugger.Write("debug", source, message);
        #endif
        }
        public static void Warn(String source, String message)
        {
        #if DEBUG
            debugger.Write("warn", source, message);
        #endif
        }
        public static void Error(String source, String message)
        {
            debugger.Write("warn", source, message);
        }
    }
}
