using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnityDarkThemePatch.Models
{
    public class UnityBinary
    {
        public UnityBinary(string filePath)
        {
            if (File.Exists(filePath))
            {
                ExecutablePath = filePath;
                ExecutableVersion = GetUnityBinaryVersion(filePath);
            }
        }

        private UnityBinaryVersion GetUnityBinaryVersion(string filePath)
        {
            var splitPath = filePath.Split('\\');
            var versionString = splitPath[splitPath.Length - 3];

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

        public string ExecutablePath { get; set; }
        public UnityBinaryVersion ExecutableVersion { get; set; }
    }
}
