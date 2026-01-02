// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

#if NETSTANDARD2_0
using System;
using System.IO;
#endif

namespace CSComm3.SLC.DataTypes
{
    /// <summary>
    /// Boolean type (BOOL).
    /// Encodes true as 0xFF and false as 0x00.
    /// Decodes 0x00 as false, any other value as true.
    /// </summary>
    public sealed class BOOL : ElementaryDataType<bool>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static BOOL Instance { get; } = new BOOL();

        private BOOL() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC1;

        /// <inheritdoc/>
        public override int Size => 1;

        /// <inheritdoc/>
        public override byte[] Encode(bool value) => new[] { value ? (byte)0xFF : (byte)0x00 };

        /// <inheritdoc/>
        public override bool Decode(byte[] buffer) => buffer[0] != 0x00;

        /// <inheritdoc/>
        public override bool Decode(Stream stream)
        {
            var b = stream.ReadByte();
            if (b == -1) throw new EndOfStreamException();
            return b != 0x00;
        }
    }
}
