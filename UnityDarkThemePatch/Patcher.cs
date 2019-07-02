using System;
using System.IO;
using Microsoft.Win32;

namespace UnityDarkThemePatch
{
    public class Patcher
    {
        public string UnityExecutablePath { get; private set; }
        public long PatchableByteAddress { get; private set; }
        public byte PatchableByteValue { get; private set; }

        // region to find, 0x00 is any byte.
        public readonly byte[] RegionBytes2017 =
        {
            0x40, 0x53,
            0x48, 0x83, 0xec, 0x20,
            0x48, 0x8b, 0xd9,
            0x00, 0x00, 0x00, 0x00, 0x00, // call command
            0x84, 0xc0,
            0x00, 0x08, // jump command //0x75
            0x33, 0xc0,
            0x48, 0x83, 0xc4, 0x20,
            0x5b,
            0xc3,
            0x8b, 0x03 ,
            0x48, 0x83, 0xc4, 0x20,
            0x5b,
            0xc3,
        };
        public readonly byte[] RegionBytes2018_old =
        {
            0x75,
            0x08,
            0x33,
            0xc0,
            0x48,
            0x83,
            0xc4,
            0x30,
            0x5b,
            0xc3,
            0x8b,
            0x03,
            0x48,
            0x83,
            0xc4,
            0x30
        };
        public readonly byte[] RegionBytes2018 =
        {
            0x04,
            0x33,
            0xc0,
            0xEB,
            0x02,
            0x8B,
            0x03,
            0x48,
            0x8B,
            0x4C,
            0x24,
            0x58,
            0x48,
            0x33,
            0xCC
        };

        // offset from start of region to the actual instruction
        public readonly int JumpInstructionOffset2017 = 16;
        public readonly int JumpInstructionOffset2018 = -1;

        public readonly byte DarkSkin2017Byte = 0x74;
        public readonly byte LightSkin2017Byte = 0x75;

        public readonly byte DarkSkin2018Byte = 0x75;
        public readonly byte LightSkin2018Byte = 0x74;

        public void Init()
        {
            UnityExecutablePath = GetUnityExecutablePath();

            var version = VersionChoice("Choose a version to patch");

            if (version == 2017)
            {
                PatchableByteAddress = FindJumpInstructionAddress(UnityExecutablePath, RegionBytes2017, JumpInstructionOffset2017);
                PatchableByteValue = GetPatchableByteValue();
                if (PatchableByteValue == DarkSkin2017Byte)
                    YesNoChoice("Revert to light skin?", () => PatchExecutable(LightSkin2017Byte));
                else
                    YesNoChoice("Apply dark skin patch?", () => PatchExecutable(DarkSkin2017Byte));
            }
            else if (version == 2018)
            {
                PatchableByteAddress = FindJumpInstructionAddress(UnityExecutablePath, RegionBytes2018, JumpInstructionOffset2018);
                PatchableByteValue = GetPatchableByteValue();
                if (PatchableByteValue == DarkSkin2018Byte)
                    YesNoChoice("Revert to light skin?", () => PatchExecutable(LightSkin2018Byte));
                else
                    YesNoChoice("Apply dark skin patch?", () => PatchExecutable(DarkSkin2018Byte));
            }
            else
            {
                ExitOnInput("Please choose a right version", ConsoleColor.Red);
            }

        }

        #region CONSOLE METHODS
        public void Write(object message = null, ConsoleColor foregroundColor = (ConsoleColor)(-1))
        {
            if (foregroundColor == (ConsoleColor)(-1))
            {
                Console.Write(message);
                return;
            }
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(message);
            Console.ForegroundColor = fg;
        }
        public void WriteLine(object message = null, ConsoleColor foregroundColor = (ConsoleColor)(-1))
        {
            Write(message, foregroundColor);
            Console.WriteLine();
        }
        public void ExitOnInput(object message = null, ConsoleColor foregroundColor = (ConsoleColor)(-1))
        {
            if (message != null)
                WriteLine(message, foregroundColor);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
        public void YesNoChoice(string message, Action yes = null, Action no = null)
        {
            while (true)
            {
                Console.Write($"{message} [Yes/No]: ");
                var answer = Console.ReadLine()?.ToLower();
                if (string.IsNullOrWhiteSpace(answer))
                    continue;
                if (answer.StartsWith("y"))
                {
                    yes?.Invoke();
                    break;
                }
                if (answer.StartsWith("n"))
                {
                    no?.Invoke();
                    break;
                }
            }
        }
        public int VersionChoice(string message)
        {
            while (true)
            {
                Console.Write($"{message} [2017/2018]: ");
                var answer = Console.ReadLine()?.ToLower();
                if (string.IsNullOrWhiteSpace(answer))
                    continue;
                if (answer.StartsWith("2017"))
                {
                    return 2017;
                }
                if (answer.StartsWith("2018"))
                {
                    return 2018;
                }
            }
        }
        #endregion

        #region UI WORK METHODS
        private string GetUnityExecutablePath()
        {
            var unityExecutablePath = GetUnityPathFromArgs() ??
                                      GetUnityPathFromLocalDir() ??
                                      GetUnityPathFromRegistry();
            Write("Unity binary: ");
            if (string.IsNullOrWhiteSpace(unityExecutablePath))
                ExitOnInput("Failed to find Unity editor binary", ConsoleColor.Red);
            WriteLine($"{unityExecutablePath}", ConsoleColor.Green);
            return unityExecutablePath;
        }
        private byte GetPatchableByteValue()
        {
            Write("Patch status: ");
            var jumpInstructionByte = ReadByteAtAddress(UnityExecutablePath, PatchableByteAddress);
            if (jumpInstructionByte == 0x75)
                WriteLine($"Light skin (unpatched) [0x{jumpInstructionByte:X} @ 0x{PatchableByteAddress:X}]", ConsoleColor.Blue);
            else if (jumpInstructionByte == 0x74)
                WriteLine($"Dark skin (patched) [0x{jumpInstructionByte:X} @ 0x{PatchableByteAddress:X}]", ConsoleColor.Green);
            else
            {
                WriteLine($"Unknown status [0x{jumpInstructionByte:X} @ 0x{PatchableByteAddress:X}]", ConsoleColor.Red);
                ExitOnInput();
            }
            return jumpInstructionByte;
        }
        private void PatchExecutable(byte b)
        {
            try
            {
                CreateFileBackup(UnityExecutablePath);
                WriteByteToAddress(UnityExecutablePath, b, PatchableByteAddress);
                WriteLine("Patch applied successfully", ConsoleColor.Green);
                // Init();
                Console.Read();
            }
            catch
            {
                ExitOnInput("Failed to write changes to file, make sure you have write permission", ConsoleColor.Red);
            }
        }
        #endregion

        #region STATIC
        public static long FindJumpInstructionAddress(string filePath, byte[] regionToFind, long jumpInstructionOffset)
        {
            using (var b = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                var length = b.BaseStream.Length;
                long index = 0;
                long firstAddr = -1;
                for (long i = 0; i < length; i++)
                {
                    var currentByte = b.ReadByte();
                    if (currentByte == regionToFind[index] || (regionToFind[index] == 0x00 && index > 0))
                    {
                        if (index == 0)
                            firstAddr = i;
                        index++;
                    }
                    else
                        index = 0;

                    if (index >= regionToFind.LongLength)
                        return firstAddr + jumpInstructionOffset;
                }
            }

            return -1;
        }

        public static byte ReadByteAtAddress(string filePath, long address)
        {
            using (var b = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                b.BaseStream.Seek(address, SeekOrigin.Begin);
                return b.ReadByte();
            }
        }

        /// <summary>
        /// Writes a single byte to a specified address in file
        /// </summary>
        /// <param name="filePath">Path to file to write into</param>
        /// <param name="byteToWrite">Byte that will be written</param>
        /// <param name="address">Address in bytes where to write the byte</param>
        public static void WriteByteToAddress(string filePath, byte byteToWrite, long address)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                stream.Position = address;
                stream.WriteByte(byteToWrite);
            }
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
                foreach (var subKeyName in regKey.GetSubKeyNames())
                    using (var subkey = regKey.OpenSubKey(subKeyName))
                    {
                        if (subkey.GetValue("DisplayName")?.ToString() == "Unity")
                        {
                            path = subkey.GetValue("DisplayIcon").ToString();
                            break;
                        }
                    }

            if (path.EndsWith("Unity.exe"))
                return path;

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
                return args[1];
            return null;
        }

        /// <summary>
        /// Attempts to find Unity.exe in same dir as this app
        /// </summary>
        /// <returns>Path to Unity.exe or null if none is found</returns>
        public static string GetUnityPathFromLocalDir()
        {
            if (File.Exists("Unity.exe"))
                return Path.GetFullPath("Unity.exe");
            return null;
        }

        public static void CreateFileBackup(string filePath)
        {
            File.Copy(filePath, $"{filePath}.bak{DateTime.Now:yyyymmdd_HHmmss}");
        }
        #endregion
    }
}