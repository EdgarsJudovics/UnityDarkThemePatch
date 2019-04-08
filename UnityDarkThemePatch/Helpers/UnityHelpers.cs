using Microsoft.Win32;
using System;
using System.IO;

namespace UnityDarkThemePatch.Helpers
{
    public static class UnityHelpers
    {
        /// <summary>
        /// Attempts to find Unity.exe path from the registry, followed by passed arguments, followed by the current directory.
        /// </summary>
        /// <returns>Path to Unity.exe or null if none is found</returns>
        public static string GetUnityPath()
        {
            return GetUnityPathFromArgs() ?? GetUnityPathFromLocalDir() ?? GetUnityPathFromRegistry();
        }

        /// <summary>
        /// Attempts to find Unity.exe path from registry
        /// </summary>
        /// <returns>Path to Unity.exe or null if none is found</returns>
        public static string GetUnityPathFromRegistry()
        {
            var uninstallRegKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var path = string.Empty;

            using (var regKey = Registry.LocalMachine.OpenSubKey(uninstallRegKey))
            {
                foreach (var subKeyName in regKey.GetSubKeyNames())
                    using (var subkey = regKey.OpenSubKey(subKeyName))
                    {
                        if (subkey.GetValue("DisplayName")?.ToString() == "Unity")
                        {
                            path = subkey.GetValue("DisplayIcon").ToString();
                            break;
                        }
                    }
            }

            if (path.EndsWith("Unity.exe"))
            {
                return path;
            }

            return null;
        }

        /// <summary>
        /// Attempts to find Unity.exe path from passed args
        /// </summary>
        /// <returns>Path to Unity.exe or null if none is found</returns>
        public static string GetUnityPathFromArgs()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) && File.Exists(args[1]))
            {
                var fileInfo = new FileInfo(args[1]);
                return fileInfo.FullName;
            }
            return null;
        }

        /// <summary>
        /// Attempts to find Unity.exe in same dir as this app
        /// </summary>
        /// <returns>Path to Unity.exe or null if none is found</returns>
        public static string GetUnityPathFromLocalDir()
        {
            if (File.Exists("Unity.exe"))
            {
                return Path.GetFullPath("Unity.exe");
            }
            return null;
        }
    }
}
