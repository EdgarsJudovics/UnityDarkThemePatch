using System;
using UnityDarkThemePatch.Helpers;
using UnityDarkThemePatch.Models;

namespace UnityDarkThemePatch
{
    public class Patcher
    {
        public UnityBinary UnityExecutable { get; private set; }
        public long PatchableByteAddress { get; private set; }
        public byte PatchableByteValue { get; private set; }

        // region to find, 0x00 is any byte.
        //75 08 33 C0 48 83 C4 30  5B C3 8B 03 48 83 C4 30
        public readonly byte[] RegionBytesVersionA =
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

        public readonly byte[] RegionBytesVersionB = 
        {
            0x08,0x33, 0xc0, 0x48, 0x83, 0xc4, 0x30, 0x5b, 0xc3, 0x8b, 0x03, 0x48, 0x83, 0xc4, 0x30
        };

        public byte[] RegionBytes;

        // offset from start of region to the actual instruction
        public int JumpInstructionOffset = -1;

        public readonly int JumpInstructionOffsetVersionA = 16;
        public readonly int JumpInstructionOffsetVersionB = -1;

        public readonly byte DarkSkinByte = 0x74;
        public readonly byte LightSkinByte = 0x75;

        public void Init()
        {
            UnityExecutable = new UnityBinary(GetUnityExecutablePath());
            RegionBytes = UnityExecutable.ExecutableVersion >= UnityBinaryVersion.UNITY_2018_3_0 ? RegionBytesVersionB : RegionBytesVersionA;
            JumpInstructionOffset = UnityExecutable.ExecutableVersion >= UnityBinaryVersion.UNITY_2018_3_0 ? JumpInstructionOffsetVersionB : JumpInstructionOffsetVersionA;
            PatchableByteAddress = BinaryHelpers.FindJumpInstructionAddress(UnityExecutable.ExecutablePath, RegionBytes, JumpInstructionOffset);
            PatchableByteValue = GetPatchableByteValue();
            if (PatchableByteValue == DarkSkinByte)
            {
                ConsoleHelpers.YesNoChoice("Revert to light skin?", () => PatchExecutable(LightSkinByte));
            }
            else
            {
                ConsoleHelpers.YesNoChoice("Apply dark skin patch?", () => PatchExecutable(DarkSkinByte));
            }
        }

        #region UI WORK METHODS

        /// <summary>
        /// Uses the UnityHelpers GetUnityPath method to find the path to the Unity executable, prints the output to the console and returns the result.
        /// </summary>
        /// <returns>The path to the Unity binary.</returns>
        private string GetUnityExecutablePath()
        {
            var unityExecutablePath = UnityHelpers.GetUnityPath();
            ConsoleHelpers.Write("Unity binary: ");
            if (string.IsNullOrWhiteSpace(unityExecutablePath))
            {
                ConsoleHelpers.ExitOnInput("Failed to find Unity editor binary", ConsoleColor.Red);
            }
            ConsoleHelpers.WriteLine($"{unityExecutablePath}", ConsoleColor.Green);
            return unityExecutablePath;
        }

        /// <summary>
        /// Gets the value of the patch byte in the Unity executable.
        /// </summary>
        /// <returns>The value of the patch byte.</returns>
        private byte GetPatchableByteValue()
        {
            ConsoleHelpers.Write("Patch status: ");
            var jumpInstructionByte = BinaryHelpers.ReadByteAtAddress(UnityExecutable.ExecutablePath, PatchableByteAddress);
            if (jumpInstructionByte == 0x75)
            {
                ConsoleHelpers.WriteLine($"Light skin (unpatched) [0x{jumpInstructionByte:X} @ 0x{PatchableByteAddress:X}]", ConsoleColor.Blue);
            }
            else if (jumpInstructionByte == 0x74)
            {
                ConsoleHelpers.WriteLine($"Dark skin (patched) [0x{jumpInstructionByte:X} @ 0x{PatchableByteAddress:X}]", ConsoleColor.Green);
            }
            else
            {
                ConsoleHelpers.WriteLine($"Unknown status [0x{jumpInstructionByte:X} @ 0x{PatchableByteAddress:X}]", ConsoleColor.Red);
                ConsoleHelpers.ExitOnInput();
            }
            return jumpInstructionByte;
        }

        /// <summary>
        /// Sets the value of the patch byte in the Unity executable.
        /// </summary>
        private void PatchExecutable(byte b)
        {
            try
            {
                FileHelpers.CreateFileBackup(UnityExecutable.ExecutablePath);
                BinaryHelpers.WriteByteToAddress(UnityExecutable.ExecutablePath, b, PatchableByteAddress);
                ConsoleHelpers.WriteLine("Patch applied successfully", ConsoleColor.Green);
                Init();
            }
            catch
            {
                ConsoleHelpers.ExitOnInput("Failed to write changes to file, make sure you have write permission", ConsoleColor.Red);
            }
        }
        #endregion
    }
}