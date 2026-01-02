// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

#if NETSTANDARD2_0
using System;
using System.IO;
#endif

namespace CSComm3.SLC.DataTypes
{
    /// <summary>
    /// 32-bit floating point type (REAL).
    /// </summary>
    public sealed class REAL : ElementaryDataType<float>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static REAL Instance { get; } = new REAL();

        private REAL() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xCA;

        /// <inheritdoc/>
        public override int Size => 4;

        /// <inheritdoc/>
        public override byte[] Encode(float value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override float Decode(byte[] buffer) => BitConverter.ToSingle(buffer, 0);

        /// <inheritdoc/>
        public override float Decode(Stream stream)
        {
            var buffer = new byte[4];
            var read = stream.Read(buffer, 0, 4);
            if (read < 4) throw new EndOfStreamException();
            return BitConverter.ToSingle(buffer, 0);
        }
    }

    /// <summary>
    /// 64-bit floating point type (LREAL).
    /// </summary>
    public sealed class LREAL : ElementaryDataType<double>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static LREAL Instance { get; } = new LREAL();

        private LREAL() { }

        /// <inheritdoc/>
        public override byte TypeCode => 0xCB;

        /// <inheritdoc/>
        public override int Size => 8;

        /// <inheritdoc/>
        public override byte[] Encode(double value) => BitConverter.GetBytes(value);

        /// <inheritdoc/>
        public override double Decode(byte[] buffer) => BitConverter.ToDouble(buffer, 0);

        /// <inheritdoc/>
        public override double Decode(Stream stream)
        {
            var buffer = new byte[8];
            var read = stream.Read(buffer, 0, 8);
            if (read < 8) throw new EndOfStreamException();
            return BitConverter.ToDouble(buffer, 0);
        }
    }
}
