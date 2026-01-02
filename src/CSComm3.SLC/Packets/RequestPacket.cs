// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Collections.Generic;
using System.IO;

namespace CSComm3.SLC.Packets
{
    /// <summary>
    /// Base class for building EtherNet/IP request packets.
    /// </summary>
    /// <remarks>
    /// EtherNet/IP Encapsulation Header (24 bytes):
    /// - Command (2 bytes)
    /// - Length (2 bytes) - length of data following header
    /// - Session Handle (4 bytes)
    /// - Status (4 bytes)
    /// - Sender Context (8 bytes)
    /// - Options (4 bytes)
    /// - Data (variable)
    /// </remarks>
    public class RequestPacket
    {
        private readonly MemoryStream _data;
        private byte[] _command = new byte[2];

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestPacket"/> class.
        /// </summary>
        public RequestPacket()
        {
            _data = new MemoryStream();
        }

        /// <summary>
        /// Gets or sets the encapsulation command.
        /// </summary>
        public byte[] Command
        {
            get => _command;
            set => _command = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the session handle.
        /// </summary>
        public uint SessionHandle { get; set; }

        /// <summary>
        /// Gets or sets the sender context.
        /// </summary>
        public byte[] SenderContext { get; set; } = new byte[8];

        /// <summary>
        /// Gets the current data length.
        /// </summary>
        public int DataLength => (int)_data.Length;

        /// <summary>
        /// Adds a byte to the packet data.
        /// </summary>
        /// <param name="value">The byte to add.</param>
        /// <returns>The current packet for method chaining.</returns>
        public RequestPacket Add(byte value)
        {
            _data.WriteByte(value);
            return this;
        }

        /// <summary>
        /// Adds bytes to the packet data.
        /// </summary>
        /// <param name="data">The bytes to add.</param>
        /// <returns>The current packet for method chaining.</returns>
        public RequestPacket Add(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _data.Write(data, 0, data.Length);
            return this;
        }

        /// <summary>
        /// Adds a 16-bit unsigned integer to the packet data (little-endian).
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The current packet for method chaining.</returns>
        public RequestPacket AddUInt16(ushort value)
        {
            _data.WriteByte((byte)(value & 0xFF));
            _data.WriteByte((byte)((value >> 8) & 0xFF));
            return this;
        }

        /// <summary>
        /// Adds a 32-bit unsigned integer to the packet data (little-endian).
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The current packet for method chaining.</returns>
        public RequestPacket AddUInt32(uint value)
        {
            _data.WriteByte((byte)(value & 0xFF));
            _data.WriteByte((byte)((value >> 8) & 0xFF));
            _data.WriteByte((byte)((value >> 16) & 0xFF));
            _data.WriteByte((byte)((value >> 24) & 0xFF));
            return this;
        }

        /// <summary>
        /// Builds the complete packet with the EtherNet/IP encapsulation header.
        /// </summary>
        /// <returns>The complete packet bytes.</returns>
        public byte[] Build()
        {
            var dataBytes = _data.ToArray();
            var packet = new byte[Constants.HeaderSize + dataBytes.Length];

            // Command (2 bytes)
            packet[0] = _command[0];
            packet[1] = _command[1];

            // Length (2 bytes) - length of data following header
            var length = (ushort)dataBytes.Length;
            packet[2] = (byte)(length & 0xFF);
            packet[3] = (byte)((length >> 8) & 0xFF);

            // Session Handle (4 bytes)
            packet[4] = (byte)(SessionHandle & 0xFF);
            packet[5] = (byte)((SessionHandle >> 8) & 0xFF);
            packet[6] = (byte)((SessionHandle >> 16) & 0xFF);
            packet[7] = (byte)((SessionHandle >> 24) & 0xFF);

            // Status (4 bytes) - always 0 for requests
            packet[8] = 0;
            packet[9] = 0;
            packet[10] = 0;
            packet[11] = 0;

            // Sender Context (8 bytes)
            Array.Copy(SenderContext, 0, packet, 12, Math.Min(SenderContext.Length, 8));

            // Options (4 bytes) - always 0
            packet[20] = 0;
            packet[21] = 0;
            packet[22] = 0;
            packet[23] = 0;

            // Data
            if (dataBytes.Length > 0)
            {
                Array.Copy(dataBytes, 0, packet, Constants.HeaderSize, dataBytes.Length);
            }

            return packet;
        }

        /// <summary>
        /// Clears the packet data.
        /// </summary>
        public void Clear()
        {
            _data.SetLength(0);
        }
    }
}
