using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Attempts to find Unity.exe paths from the registry, followed by passed arguments, followed by the current directory.
        /// </summary>
        /// <returns>A collection of paths to Unity.exe.</returns>
        public static IEnumerable<string> GetUnityPaths()
        {
            var argPaths = GetUnityPathsFromArgs();
            var regPaths = GetUnityPathsFromRegistry();
            var localPath = GetUnityPathFromLocalDir();
            var result = argPaths.Concat(regPaths).ToList();

            if (localPath != null)
            {
                result.Add(localPath);
            }

            return result.Distinct();
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
        /// Attempts to find Unity.exe paths from registry.
        /// </summary>
        /// <returns>A collection of paths to Unity.exe.</returns>
        public static IEnumerable<string> GetUnityPathsFromRegistry()
        {
            var uninstallRegKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var paths = new List<string>();

            using (var regKey = Registry.LocalMachine.OpenSubKey(uninstallRegKey))
            {
                foreach (var subKeyName in regKey.GetSubKeyNames())
                    using (var subkey = regKey.OpenSubKey(subKeyName))
                    {
                        if (subkey.GetValue("DisplayName")?.ToString() == "Unity")
                        {
                            paths.Add(subkey.GetValue("DisplayIcon").ToString());
                            break;
                        }
                    }
            }

            return paths.Where(p => p.EndsWith("Unity.exe"));
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
        /// Attempts to find Unity.exe paths from passed args.
        /// </summary>
        /// <returns>A collection of paths to Unity.exe.</returns>
        public static IEnumerable<string> GetUnityPathsFromArgs()
        {
            var args = Environment.GetCommandLineArgs().ToList();
            return args.Select(a =>
            {
                if (string.IsNullOrWhiteSpace(a) || File.Exists(a)) { return null; }
                var fileInfo = new FileInfo(args[1]);
                return fileInfo.FullName;
            }).Where(v => v != null && v.EndsWith("Unity.exe")).ToList();
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
