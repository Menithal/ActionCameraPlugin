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
        private readonly Dictionary<String, FloatConstraint> floatConfigs;
        private readonly Dictionary<String, BooleanConstraint> booleanConfigs;
        private readonly Dictionary<String, FieldInfo> configFieldInfos;
        private readonly HashSet<String> configValidationSet;

        public ConfigUtility(String filename = "MACPluginDefault.config")
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            configPath = Path.Combine(documentsPath, "LIV/Plugins/" + filename);

            Config = new ActionCameraConfig();
            floatConfigs = new Dictionary<string, FloatConstraint>();
            booleanConfigs = new Dictionary<string, BooleanConstraint>();
            configFieldInfos = new Dictionary<string, FieldInfo>();
            configValidationSet = new HashSet<string>();
            
            BuildConstraintDictionary();

            // See if file exists, else just create new from default else
            ReadConstraintConfig();

            // Write new file
        }

        private void BuildConstraintDictionary()
        {
            Type type = typeof(ActionCameraConfig);

            FieldInfo[] properties = type.GetFields();

            SerializableFloatConfig floatConstraint;
            SerializableBooleanConfig booleanConstraint;

            foreach (FieldInfo property in properties)
            {
                floatConstraint = property.GetCustomAttribute<SerializableFloatConfig>();
                if (floatConstraint != null)
                {
                    configFieldInfos.Add(property.Name, property);
                    floatConfigs.Add(property.Name, floatConstraint.Constraint);
                    configValidationSet.Add(property.Name);
                    continue;
                }

                booleanConstraint = property.GetCustomAttribute<SerializableBooleanConfig>();
                if (booleanConstraint != null)
                {
                    configFieldInfos.Add(property.Name, property);
                    booleanConfigs.Add(property.Name, booleanConstraint.Constraint);
                    configValidationSet.Add(property.Name);
                }
            }
        }


        private void ReadConstraintConfig()
        {

            using (FileStream fs = File.Open(configPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                // If doesnt exist, create it
                // If Exists, get all values from this and start applying it to config

                try
                {

                    StreamReader sr = new StreamReader(fs);
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = Regex.Replace(line, @"\s+", ""); // Remove All whitespaces

                        // Check if contains # at the start, if does, ignore. does this contain an =?
                        String[] commentSplit = line.Split(new char[] { '#' }, 2);

                        if (commentSplit[0].Length == 0) continue; // If there is nothing, just skip the next split

                        String[] configSplit = commentSplit[0].Split(new char[] { '=' }, 2);

                        String key = configSplit[0];
                        String value = configSplit[1];

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
                            PluginLog.Warn( "ConfigUtility", "Unknown Key " + key + ". You can remove this from the config file, its not being used by this plugin.");
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

                    PluginLog.Error("ConfigUtility", "FieldAccessException: This error should not occur and indicates an untested case. Please file ticket " + e.Message);
                }
                catch (TargetException e)
                {

                    PluginLog.Error("ConfigUtility", "TargetException: This error should not occur and indicates an untested case. Please file ticket " + e.Message);
                }
                catch (ArgumentException e)
                {
                    PluginLog.Error("ConfigUtility", "ArgumentException: This error should not occur and indicates an untested case. Please file ticket " + e.Message);
                }

                Config.PrintContents();

                PluginLog.Debug("ConfigUtility", "Settings are now Loaded");

                Config.ready = true;
            }

        }
    }
}
