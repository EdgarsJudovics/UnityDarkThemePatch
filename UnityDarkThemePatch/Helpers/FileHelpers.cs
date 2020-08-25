using System;
using System.IO;

namespace UnityDarkThemePatch.Helpers
{
    public static class FileHelpers
    {
        /// <summary>
        /// Creates a backup copy of the specified file using the current DateTime value.
        /// </summary>
        /// <param name="filePath"></param>
        public static void CreateFileBackup(string filePath) => File.Copy(filePath, $"{filePath}.bak{DateTime.Now:yyyymmdd_HHmmss}");
    }
}
