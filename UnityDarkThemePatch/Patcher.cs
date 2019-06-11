using System;
using System.Collections.Generic;
using System.Linq;
using UnityDarkThemePatch.Helpers;
using UnityDarkThemePatch.Models;

namespace UnityDarkThemePatch
{
    public class Patcher
    {
        public IEnumerable<UnityBinary> UnityExecutables { get; private set; }

        public UnityBinary UnityExecutable { get; private set; }

        public long PatchableByteAddress { get; private set; }

        public byte PatchableByteValue { get; private set; }

        // region to find, 0x00 is any byte.
        //75 08 33 C0 48 83 C4 30  5B C3 8B 03 48 83 C4 30
        private readonly byte[] RegionBytes_Pre2018_3 =
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

        private readonly byte[] RegionBytes_2018_3 = 
        {
            0x08,0x33, 0xc0, 0x48, 0x83, 0xc4, 0x30, 0x5b, 0xc3, 0x8b, 0x03, 0x48, 0x83, 0xc4, 0x30
        };

        private readonly byte[] RegionBytes_2019_0 =
        {
            0x04,0x33, 0xc0, 0xeb, 0x02, 0x8b, 0x07
        };

        private readonly byte[] RegionBytes_2019_2 =
        {
            0x15, 0x33, 0xc0, 0xeb, 0x13, 0x90
        };

        //private byte[] RegionBytes;

        // offset from start of region to the actual instruction
        //private int JumpInstructionOffset = -1;

        private readonly int JumpInstructionOffset_Pre2018_3 = 16;
        private readonly int JumpInstructionOffset_2018_3 = -1;
        private readonly int JumpInstructionOffset_2019_0 = -1;
        private readonly int JumpInstructionOffset_2019_2 = -1;

        private readonly byte DarkSkinByte = 0x74;
        private readonly byte LightSkinByte = 0x75;

        struct UnityVersionContainer
        {
            public byte[] RegionByteArray;
            public int InstructionOffset;
        }

        private UnityVersionContainer FindRegionBytesByVersion(UnityBinaryVersion version) 
        {
            var versionContainer = new UnityVersionContainer();

            if (version < UnityBinaryVersion.UNITY_2018_3_0)
            {
                versionContainer.RegionByteArray = RegionBytes_Pre2018_3;
                versionContainer.InstructionOffset = JumpInstructionOffset_Pre2018_3;
                return versionContainer;
            }

            if (version < UnityBinaryVersion.UNITY_2019_1_0)
            {
                versionContainer.RegionByteArray = RegionBytes_2018_3;
                versionContainer.InstructionOffset = JumpInstructionOffset_2018_3;
                return versionContainer;
            }

            if (version < UnityBinaryVersion.UNITY_2019_2_0)
            {
                versionContainer.RegionByteArray = RegionBytes_2019_0;
                versionContainer.InstructionOffset = JumpInstructionOffset_2019_0;
                return versionContainer;
            }

            if (version < UnityBinaryVersion.UNITY_2019_2_0)
            {
                versionContainer.RegionByteArray = RegionBytes_2019_0;
                versionContainer.InstructionOffset = JumpInstructionOffset_2019_0;
                return versionContainer;
            }

            //must be newer, will break as unity adds new versions, needs to be added to sequentially as the AOB changes
            versionContainer.RegionByteArray = RegionBytes_2019_2;
            versionContainer.InstructionOffset = JumpInstructionOffset_2019_2;
            return versionContainer;
        }

        public void Init()
        {
            while (true)
            {
                UnityExecutables = GetUnityExecutablePath().Select(p => new UnityBinary(p));
                bool quitRequested = false;
                var choices = UnityExecutables.Select(e => new ConsoleChoice { ChoiceDescription = e.ExecutableVersionString, ChoiceAction = () => UnityExecutable = e }).ToList();
                ConsoleHelpers.MultipleChoice(choices, () => quitRequested = true);
                if (quitRequested) { break; }


                //replace with FindRegion here
                UnityVersionContainer currentVersion = FindRegionBytesByVersion(UnityExecutable.ExecutableVersion);

                //RegionBytes = UnityExecutable.ExecutableVersion >= UnityBinaryVersion.UNITY_2018_3_0 ? RegionBytes_2018_3 : RegionBytes_Pre2018_3;
                //JumpInstructionOffset = UnityExecutable.ExecutableVersion >= UnityBinaryVersion.UNITY_2018_3_0 ? JumpInstructionOffsetVersionB : JumpInstructionOffsetVersionA;
                PatchableByteAddress = BinaryHelpers.FindJumpInstructionAddress(UnityExecutable.ExecutablePath, currentVersion.RegionByteArray, currentVersion.InstructionOffset);
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
        }

        #region UI WORK METHODS

        /// <summary>
        /// Uses the UnityHelpers GetUnityPath method to find the path to the Unity executable, prints the output to the console and returns the result.
        /// </summary>
        /// <returns>The path to the Unity binary.</returns>
        private IEnumerable<string> GetUnityExecutablePath()
        {
            var unityExecutablePaths = UnityHelpers.GetUnityPaths();
            ConsoleHelpers.Write("Unity binaries: ");
            ConsoleHelpers.WriteLine();
            if (unityExecutablePaths.Count() <= 0)
            {
                ConsoleHelpers.ExitOnInput("Failed to find Unity editor binary", ConsoleColor.Red);
            }
            unityExecutablePaths.ToList().ForEach(p => ConsoleHelpers.WriteLine($"{p}", ConsoleColor.Green));
            ConsoleHelpers.WriteLine();
            return unityExecutablePaths;
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