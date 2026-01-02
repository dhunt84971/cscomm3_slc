// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

#if NETSTANDARD2_0
using System;
using System.IO;
#endif

namespace CSComm3.SLC.DataTypes
{
    /// <summary>
    /// Base class for all CIP data types.
    /// Provides static Encode/Decode methods for serializing/deserializing values.
    /// </summary>
    public abstract class DataType
    {
        /// <summary>
        /// Gets the CIP type code for this data type.
        /// </summary>
        public abstract byte TypeCode { get; }

        /// <summary>
        /// Gets the size in bytes of this data type.
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// Gets the name of this data type.
        /// </summary>
        public virtual string Name => GetType().Name;
    }

    /// <summary>
    /// Base class for elementary (primitive) data types.
    /// </summary>
    /// <typeparam name="T">The .NET type that this data type represents.</typeparam>
    public abstract class ElementaryDataType<T> : DataType where T : struct
    {
        /// <summary>
        /// Encodes a value to bytes.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <returns>The encoded bytes.</returns>
        public abstract byte[] Encode(T value);

        /// <summary>
        /// Decodes a value from a byte array.
        /// </summary>
        /// <param name="buffer">The buffer containing the encoded value.</param>
        /// <returns>The decoded value.</returns>
        public abstract T Decode(byte[] buffer);

        /// <summary>
        /// Decodes a value from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The decoded value.</returns>
        public abstract T Decode(Stream stream);

        /// <summary>
        /// Decodes a value from a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The buffer containing the encoded value.</param>
        /// <param name="offset">The offset in the buffer to start reading from.</param>
        /// <returns>The decoded value.</returns>
        public virtual T Decode(byte[] buffer, int offset)
        {
            var bytes = new byte[Size];
            Buffer.BlockCopy(buffer, offset, bytes, 0, Size);
            return Decode(bytes);
        }
    }
}
