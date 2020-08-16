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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MACPlugin.Utility;
namespace MACPlugin.Utility
{
    public class ConfigUtility
    {
        private readonly String configPath;
        public ActionCameraConfig Config { get; private set; }

        /**
         * Utility Class that is used to read and write configurations from a single file.
         */
        public ConfigUtility(String filename, float defaultFov, float gunFov)
        {
            String drive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            String homePath = Environment.GetEnvironmentVariable("HOMEPATH");
            configPath = drive + homePath + "\\Documents\\LIV\\Plugins\\" + filename;

            Config = new ActionCameraConfig
            {
                cameraDefaultFov = defaultFov,
                cameraGunFov = gunFov
            };
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            BuildConfig();

            // See if file exists, else just create new from default else

#if DEBUG
            sw.Stop();
            PluginLog.Log("ConfigUtility", "Configuration Written and Read in " + sw.ElapsedMilliseconds + "ms");
#endif
            // Write new file
        }

        private void BuildConfig()
        {
            // Using Dictionaries for easier look up. Its not very efficient, but its easier to write. 
            // But since we are dealing with less than 1000 lines, I dont think i need to worry about it.
            Dictionary<String, FloatConstraint> floatConfigs = new Dictionary<string, FloatConstraint>();
            Dictionary<String, BooleanConstraint> booleanConfigs = new Dictionary<string, BooleanConstraint>();
            Dictionary<String, FieldInfo> configFieldInfos = new Dictionary<string, FieldInfo>();
            HashSet<String> configValidationSet = new HashSet<string>();

            Type type = typeof(ActionCameraConfig);

            FieldInfo[] properties = type.GetFields();

            SerializableFloatConfig floatConfigField;
            SerializableBooleanConfig booleanConfigField;

            foreach (FieldInfo property in properties)
            {
                floatConfigField = property.GetCustomAttribute<SerializableFloatConfig>();
                if (floatConfigField != null)
                {
                    configFieldInfos.Add(property.Name, property);
                    floatConfigs.Add(property.Name, floatConfigField.Constraint);
                    configValidationSet.Add(property.Name);
                    continue;
                }

                booleanConfigField = property.GetCustomAttribute<SerializableBooleanConfig>();
                if (booleanConfigField != null)
                {
                    configFieldInfos.Add(property.Name, property);
                    booleanConfigs.Add(property.Name, booleanConfigField.Constraint);
                    configValidationSet.Add(property.Name);
                }
            }

            using (FileStream fs = File.Open(configPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                // If doesnt exist, create it
                // If Exists, get all values from this and start applying it to config

                try
                {

                    StreamReader sr = new StreamReader(fs);
                    string line;
                    int lineIndex = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineIndex++;
                        line = Regex.Replace(line, @"\s+", ""); // Remove All whitespaces

                        // Check if contains # at the start, if does, ignore. does this contain an =?
                        String[] commentSplit = line.Split(new char[] { '#' }, 2);

                        if (commentSplit[0].Length == 0) continue; // If there is nothing, just skip the next split

                        String[] configSplit = commentSplit[0].Split(new char[] { '=' }, 2);

                        if (configSplit.Length != 2)
                        {
                            PluginLog.Warn("ConfigUtility", "Skipping line " + lineIndex + ": " + line);

                            continue;
                        }
                        String key = configSplit[0];
                        String value = configSplit[1];

                        try
                        {
                            configValidationSet.Remove(key);
                            if (floatConfigs.ContainsKey(key))
                            {
                                FloatConstraint floatConstraint = floatConfigs[key];

                                configFieldInfos[key].SetValue(Config, floatConstraint.Constrain(float.Parse(value)));

                            }
                            else if (booleanConfigs.ContainsKey(key))
                            {
                                BooleanConstraint booleanConstraint = booleanConfigs[key];

                                configFieldInfos[key].SetValue(Config, booleanConstraint.Constrain(Boolean.Parse(value)));
                            }
                            else
                            {
                                PluginLog.Warn("ConfigUtility", "Config " + key + " is unknown. You can remove this from the config file, its not being used by this plugin.");
                            }
                        }
                        catch (FormatException)
                        {
                            PluginLog.Error("ConfigUtility", "Config " + key + " format is invalid, Double check that input value is correct " + value);
                        }
                        catch (ArgumentException)
                        {
                            PluginLog.Error("ConfigUtility", "Config " + key + " is empty when it shouldnt be.");
                        }
                        catch (OverflowException)
                        {
                            PluginLog.Error("ConfigUtility", "Config " + key + " is big... Too big. Double check the values.");
                        }

                        // Do config binding here.
                    }

                    StreamWriter sw = new StreamWriter(fs);

                    // Write missing to end of file.
                    foreach (String val in configValidationSet.ToArray())
                    {
                        if (floatConfigs.ContainsKey(val))
                        {
                            sw.WriteLine(val + " = " + floatConfigs[val].DefaultValue);
                            PluginLog.Debug("ConfigUtility", "Could not find " + val + " writing to file");
                        }
                        else if (booleanConfigs.ContainsKey(val))
                        {
                            sw.WriteLine(val + " = " + booleanConfigs[val].DefaultValue);
                            PluginLog.Debug("ConfigUtility", "Could not find " + val + " writing to file");
                        }

                        sw.Flush(); // Have to flush after every line written.
                    }


                }
                catch (FieldAccessException e)
                {
                    PluginLog.Error("ConfigUtility", "FieldAccessException: This error should not occur and indicates an untested case. Please file github issue " + e.Message);
                }
                catch (TargetException e)
                {
                    PluginLog.Error("ConfigUtility", "TargetException: This error should not occur and indicates an untested case. Please github issue " + e.Message);
                }
                catch (ArgumentException e)
                {
                    PluginLog.Error("ConfigUtility", "ArgumentException: This error should not occur and indicates an untested case. Please github issue " + e.Message);
                }


                PluginLog.Debug("ConfigUtility", "Settings are now Loaded");


                Config.ready = true;
            }

        }


    }
}
