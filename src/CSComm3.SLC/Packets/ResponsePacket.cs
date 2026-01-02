// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using CSComm3.SLC.Exceptions;

namespace CSComm3.SLC.Packets
{
    /// <summary>
    /// Represents a parsed EtherNet/IP response packet.
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
    public class ResponsePacket
    {
        private readonly byte[] _rawData;
        private int _dataPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponsePacket"/> class.
        /// </summary>
        /// <param name="rawData">The raw packet data including the header.</param>
        public ResponsePacket(byte[] rawData)
        {
            _rawData = rawData ?? throw new ArgumentNullException(nameof(rawData));

            if (rawData.Length < Constants.HeaderSize)
            {
                throw new DataException($"Response packet too short: {rawData.Length} bytes, expected at least {Constants.HeaderSize}");
            }

            ParseHeader();
            _dataPosition = Constants.HeaderSize;
        }

        /// <summary>
        /// Gets the encapsulation command.
        /// </summary>
        public byte[] Command { get; private set; } = new byte[2];

        /// <summary>
        /// Gets the data length from the header.
        /// </summary>
        public ushort Length { get; private set; }

        /// <summary>
        /// Gets the session handle.
        /// </summary>
        public uint SessionHandle { get; private set; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        public uint Status { get; private set; }

        /// <summary>
        /// Gets the sender context.
        /// </summary>
        public byte[] SenderContext { get; private set; } = new byte[8];

        /// <summary>
        /// Gets the options.
        /// </summary>
        public uint Options { get; private set; }

        /// <summary>
        /// Gets the data portion of the packet (after the header).
        /// </summary>
        public byte[] Data { get; private set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets the raw packet data.
        /// </summary>
        public byte[] RawData => _rawData;

        /// <summary>
        /// Gets the number of remaining bytes to read from the data portion.
        /// </summary>
        public int RemainingBytes => Data.Length - (_dataPosition - Constants.HeaderSize);

        /// <summary>
        /// Gets a value indicating whether the response indicates success.
        /// </summary>
        public bool IsSuccess => Status == Constants.Success;

        /// <summary>
        /// Reads a byte from the data portion.
        /// </summary>
        /// <returns>The byte value.</returns>
        public byte ReadByte()
        {
            EnsureDataAvailable(1);
            return _rawData[_dataPosition++];
        }

        /// <summary>
        /// Reads bytes from the data portion.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The bytes.</returns>
        public byte[] ReadBytes(int count)
        {
            EnsureDataAvailable(count);
            var result = new byte[count];
            Array.Copy(_rawData, _dataPosition, result, 0, count);
            _dataPosition += count;
            return result;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the data portion (little-endian).
        /// </summary>
        /// <returns>The value.</returns>
        public ushort ReadUInt16()
        {
            EnsureDataAvailable(2);
            var value = (ushort)(_rawData[_dataPosition] | (_rawData[_dataPosition + 1] << 8));
            _dataPosition += 2;
            return value;
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from the data portion (little-endian).
        /// </summary>
        /// <returns>The value.</returns>
        public uint ReadUInt32()
        {
            EnsureDataAvailable(4);
            var value = (uint)(_rawData[_dataPosition] |
                              (_rawData[_dataPosition + 1] << 8) |
                              (_rawData[_dataPosition + 2] << 16) |
                              (_rawData[_dataPosition + 3] << 24));
            _dataPosition += 4;
            return value;
        }

        /// <summary>
        /// Skips a specified number of bytes in the data portion.
        /// </summary>
        /// <param name="count">The number of bytes to skip.</param>
        public void Skip(int count)
        {
            EnsureDataAvailable(count);
            _dataPosition += count;
        }

        /// <summary>
        /// Resets the read position to the start of the data portion.
        /// </summary>
        public void ResetPosition()
        {
            _dataPosition = Constants.HeaderSize;
        }

        /// <summary>
        /// Throws a <see cref="ResponseException"/> if the status is not success.
        /// </summary>
        /// <param name="message">The error message prefix.</param>
        public void ThrowIfError(string message = "Response error")
        {
            if (!IsSuccess)
            {
                throw new ResponseException($"{message}: Status 0x{Status:X8}", (int)Status);
            }
        }

        private void ParseHeader()
        {
            // Command (2 bytes)
            Command[0] = _rawData[0];
            Command[1] = _rawData[1];

            // Length (2 bytes)
            Length = (ushort)(_rawData[2] | (_rawData[3] << 8));

            // Session Handle (4 bytes)
            SessionHandle = (uint)(_rawData[4] |
                                   (_rawData[5] << 8) |
                                   (_rawData[6] << 16) |
                                   (_rawData[7] << 24));

            // Status (4 bytes)
            Status = (uint)(_rawData[8] |
                           (_rawData[9] << 8) |
                           (_rawData[10] << 16) |
                           (_rawData[11] << 24));

            // Sender Context (8 bytes)
            Array.Copy(_rawData, 12, SenderContext, 0, 8);

            // Options (4 bytes)
            Options = (uint)(_rawData[20] |
                            (_rawData[21] << 8) |
                            (_rawData[22] << 16) |
                            (_rawData[23] << 24));

            // Data
            if (_rawData.Length > Constants.HeaderSize)
            {
                var dataLength = Math.Min(Length, _rawData.Length - Constants.HeaderSize);
                Data = new byte[dataLength];
                Array.Copy(_rawData, Constants.HeaderSize, Data, 0, dataLength);
            }
        }

        private void EnsureDataAvailable(int count)
        {
            if (_dataPosition + count > _rawData.Length)
            {
                throw new DataException($"Not enough data: need {count} bytes, have {_rawData.Length - _dataPosition}");
            }
        }
    }
}
