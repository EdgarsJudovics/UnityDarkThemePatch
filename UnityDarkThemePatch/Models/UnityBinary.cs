using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnityDarkThemePatch.Models
{
    public class UnityBinary
    {
        /// <summary>
        /// The path to Unity.exe.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// The Unity version.
        /// </summary>
        public UnityBinaryVersion ExecutableVersion { get; set; }

        /// <summary>
        /// The Unity version string as read from the executable path.
        /// </summary>
        public string ExecutableVersionString { get; set; }

        public UnityBinary(string filePath, string versionString = "")
        {
            if (File.Exists(filePath))
            {
                ExecutablePath = filePath;
                ExecutableVersion = GetUnityBinaryVersion(filePath);

                if (!string.IsNullOrWhiteSpace(versionString))
                {
                    ExecutableVersionString = versionString;
                }
                else if (string.IsNullOrWhiteSpace(ExecutableVersionString))
                {
                    var descriptionAttr = ExecutableVersion.GetType().GetField(ExecutableVersion.ToString()).GetCustomAttribute<DescriptionAttribute>();
                    if (descriptionAttr != null)
                    {
                        ExecutableVersionString = descriptionAttr.Description;
                    } else
                    {
                        ExecutableVersionString = "";
                    }
                }
            }
        }

        private UnityBinaryVersion GetUnityBinaryVersion(string filePath)
        {
            var splitPath = filePath.Split('\\');
            var versionString = splitPath[splitPath.Length - 3];
            ExecutableVersionString = versionString;

            var versionOptions = Enum.GetValues(typeof(UnityBinaryVersion)).Cast<UnityBinaryVersion>();
            versionOptions.DefaultIfEmpty(UnityBinaryVersion.UNKNOWN);

            return versionOptions.LastOrDefault(v =>
            {
                var descriptionAttr = v.GetType().GetField(v.ToString()).GetCustomAttribute<DescriptionAttribute>();

                if (descriptionAttr != null)
                {
                    return filePath.Contains(descriptionAttr.Description);
                }

                return filePath.Contains(v.ToString());
            });
        }
    }
}
