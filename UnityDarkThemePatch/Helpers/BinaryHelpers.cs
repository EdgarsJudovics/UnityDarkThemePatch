using System.IO;

namespace UnityDarkThemePatch.Helpers
{
    public static class BinaryHelpers
    {
        /// <summary>
        /// Finds the JumpInstructionAddress using the specified file, byte array regionToFind and jumpInstructionOffset.
        /// </summary>
        /// <param name="filePath">Path to the file to search</param>
        /// <param name="regionToFind">Byte Array region to find in the file.</param>
        /// <param name="jumpInstructionOffset">Offset to the jump instruction.</param>
        /// <returns>The address of the JumpInstruction.</returns>
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
                    if (regionToFind[index] == currentByte || (regionToFind[index] == 0x00 && index > 0))
                    {
                        if (index == 0)
                        {
                            firstAddr = i;
                        }
                        index += 1;
                    }
                    else
                    {
                        index = 0;
                    }

                    if (index >= regionToFind.LongLength)
                    {
                        return firstAddr + jumpInstructionOffset;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Reads and returns the bytes at the specified address in the specified file.
        /// </summary>
        /// <param name="filePath">The file to read.</param>
        /// <param name="address">The binary address to read.</param>
        /// <returns>A byte array read from the file at the address.</returns>
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
    }
}
