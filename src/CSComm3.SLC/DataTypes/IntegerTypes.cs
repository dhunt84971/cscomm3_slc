// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

#if NETSTANDARD2_0
using System;
using System.IO;
#endif

namespace CSComm3.SLC.DataTypes
{
    /// <summary>
    /// Signed 8-bit integer type (SINT).
    /// </summary>
    public sealed class SINT : ElementaryDataType<sbyte>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static SINT Instance { get; } = new SINT();

        private SINT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC2;

        /// <inheritdoc/>
        public override int Size => 1;

        /// <inheritdoc/>
        public override byte[] Encode(sbyte value) => new[] { (byte)value };

        /// <inheritdoc/>
        public override sbyte Decode(byte[] buffer) => (sbyte)buffer[0];

        /// <inheritdoc/>
        public override sbyte Decode(Stream stream)
        {
            var b = stream.ReadByte();
            if (b == -1) throw new EndOfStreamException();
            return (sbyte)b;
        }
    }

    /// <summary>
    /// Signed 16-bit integer type (INT).
    /// </summary>
    public sealed class INT : ElementaryDataType<short>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static INT Instance { get; } = new INT();

        private INT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC3;

        /// <inheritdoc/>
        public override int Size => 2;

        /// <inheritdoc/>
        public override byte[] Encode(short value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override short Decode(byte[] buffer) => BitConverter.ToInt16(buffer, 0);

        /// <inheritdoc/>
        public override short Decode(Stream stream)
        {
            var buffer = new byte[2];
            var read = stream.Read(buffer, 0, 2);
            if (read < 2) throw new EndOfStreamException();
            return BitConverter.ToInt16(buffer, 0);
        }
    }

    /// <summary>
    /// Signed 32-bit integer type (DINT).
    /// </summary>
    public sealed class DINT : ElementaryDataType<int>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DINT Instance { get; } = new DINT();

        private DINT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC4;

        /// <inheritdoc/>
        public override int Size => 4;

        /// <inheritdoc/>
        public override byte[] Encode(int value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override int Decode(byte[] buffer) => BitConverter.ToInt32(buffer, 0);

        /// <inheritdoc/>
        public override int Decode(Stream stream)
        {
            var buffer = new byte[4];
            var read = stream.Read(buffer, 0, 4);
            if (read < 4) throw new EndOfStreamException();
            return BitConverter.ToInt32(buffer, 0);
        }
    }

    /// <summary>
    /// Signed 64-bit integer type (LINT).
    /// </summary>
    public sealed class LINT : ElementaryDataType<long>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static LINT Instance { get; } = new LINT();

        private LINT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC5;

        /// <inheritdoc/>
        public override int Size => 8;

        /// <inheritdoc/>
        public override byte[] Encode(long value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override long Decode(byte[] buffer) => BitConverter.ToInt64(buffer, 0);

        /// <inheritdoc/>
        public override long Decode(Stream stream)
        {
            var buffer = new byte[8];
            var read = stream.Read(buffer, 0, 8);
            if (read < 8) throw new EndOfStreamException();
            return BitConverter.ToInt64(buffer, 0);
        }
    }

    /// <summary>
    /// Unsigned 8-bit integer type (USINT).
    /// </summary>
    public sealed class USINT : ElementaryDataType<byte>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static USINT Instance { get; } = new USINT();

        private USINT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC6;

        /// <inheritdoc/>
        public override int Size => 1;

        /// <inheritdoc/>
        public override byte[] Encode(byte value) => new[] { value };

        /// <inheritdoc/>
        public override byte Decode(byte[] buffer) => buffer[0];

        /// <inheritdoc/>
        public override byte Decode(Stream stream)
        {
            var b = stream.ReadByte();
            if (b == -1) throw new EndOfStreamException();
            return (byte)b;
        }
    }

    /// <summary>
    /// Unsigned 16-bit integer type (UINT).
    /// </summary>
    public sealed class UINT : ElementaryDataType<ushort>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static UINT Instance { get; } = new UINT();

        private UINT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC7;

        /// <inheritdoc/>
        public override int Size => 2;

        /// <inheritdoc/>
        public override byte[] Encode(ushort value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override ushort Decode(byte[] buffer) => BitConverter.ToUInt16(buffer, 0);

        /// <inheritdoc/>
        public override ushort Decode(Stream stream)
        {
            var buffer = new byte[2];
            var read = stream.Read(buffer, 0, 2);
            if (read < 2) throw new EndOfStreamException();
            return BitConverter.ToUInt16(buffer, 0);
        }
    }

    /// <summary>
    /// Unsigned 32-bit integer type (UDINT).
    /// </summary>
    public sealed class UDINT : ElementaryDataType<uint>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static UDINT Instance { get; } = new UDINT();

        private UDINT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC8;

        /// <inheritdoc/>
        public override int Size => 4;

        /// <inheritdoc/>
        public override byte[] Encode(uint value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override uint Decode(byte[] buffer) => BitConverter.ToUInt32(buffer, 0);

        /// <inheritdoc/>
        public override uint Decode(Stream stream)
        {
            var buffer = new byte[4];
            var read = stream.Read(buffer, 0, 4);
            if (read < 4) throw new EndOfStreamException();
            return BitConverter.ToUInt32(buffer, 0);
        }
    }

    /// <summary>
    /// Unsigned 64-bit integer type (ULINT).
    /// </summary>
    public sealed class ULINT : ElementaryDataType<ulong>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static ULINT Instance { get; } = new ULINT();

        private ULINT() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xC9;

        /// <inheritdoc/>
        public override int Size => 8;

        /// <inheritdoc/>
        public override byte[] Encode(ulong value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override ulong Decode(byte[] buffer) => BitConverter.ToUInt64(buffer, 0);

        /// <inheritdoc/>
        public override ulong Decode(Stream stream)
        {
            var buffer = new byte[8];
            var read = stream.Read(buffer, 0, 8);
            if (read < 8) throw new EndOfStreamException();
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
