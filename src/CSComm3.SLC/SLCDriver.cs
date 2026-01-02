// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSComm3.SLC.CIP;
using CSComm3.SLC.DataTypes;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Internal;
using CSComm3.SLC.Packets;
using CSComm3.SLC.PCCC;

namespace CSComm3.SLC
{
    /// <summary>
    /// Driver for communicating with Allen Bradley SLC and MicroLogix PLCs over Ethernet.
    /// </summary>
    /// <remarks>
    /// Supported PLCs:
    /// - SLC 5/05 (1747-L551, 1747-L552, 1747-L553)
    /// - MicroLogix 1100 (1763-L16xxx)
    /// - MicroLogix 1400 (1766-Lxxxxx)
    ///
    /// Addressing format: FileType FileNumber : ElementNumber [.SubElement | /BitNumber]
    /// Examples:
    /// - N7:0     - Integer file 7, element 0
    /// - F8:5     - Float file 8, element 5
    /// - B3:0/5   - Bit file 3, element 0, bit 5
    /// - T4:0.ACC - Timer file 4, element 0, accumulated value
    /// </remarks>
    public class SLCDriver : IDisposable
    {
        private readonly CipDriver _cipDriver;
        private readonly bool _ownsCipDriver;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SLCDriver"/> class.
        /// </summary>
        /// <param name="path">The IP address or hostname of the PLC (e.g., "192.168.1.100").</param>
        public SLCDriver(string path)
            : this(new CipDriver(path), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SLCDriver"/> class with a custom CIP driver.
        /// </summary>
        /// <param name="cipDriver">The CIP driver to use.</param>
        /// <param name="ownsCipDriver">Whether this instance owns the CIP driver and should dispose it.</param>
        internal SLCDriver(CipDriver cipDriver, bool ownsCipDriver)
        {
            _cipDriver = cipDriver ?? throw new ArgumentNullException(nameof(cipDriver));
            _ownsCipDriver = ownsCipDriver;
        }

        /// <summary>
        /// Gets the path (IP address) of the PLC.
        /// </summary>
        public string Path => _cipDriver.Path;

        /// <summary>
        /// Gets a value indicating whether the connection is open.
        /// </summary>
        public bool Connected => _cipDriver.Connected;

        /// <summary>
        /// Gets or sets the timeout in milliseconds.
        /// </summary>
        public int Timeout
        {
            get => _cipDriver.Timeout;
            set => _cipDriver.Timeout = value;
        }

        /// <summary>
        /// Opens a connection to the PLC.
        /// </summary>
        /// <returns>True if the connection was successful.</returns>
        public bool Open()
        {
            ThrowIfDisposed();
            return _cipDriver.Open();
        }

        /// <summary>
        /// Opens a connection to the PLC asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the connection was successful.</returns>
        public Task<bool> OpenAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            return _cipDriver.OpenAsync(cancellationToken);
        }

        /// <summary>
        /// Closes the connection to the PLC.
        /// </summary>
        public void Close()
        {
            _cipDriver.Close();
        }

        /// <summary>
        /// Gets the identity of the connected PLC.
        /// </summary>
        /// <returns>The device identity information.</returns>
        public DeviceIdentity GetIdentity()
        {
            ThrowIfDisposed();
            EnsureConnected();
            return _cipDriver.GetIdentity();
        }

        /// <summary>
        /// Reads a single tag from the PLC.
        /// </summary>
        /// <param name="address">The tag address (e.g., "N7:0", "F8:5", "B3:0/5").</param>
        /// <returns>The tag with its value populated.</returns>
        public Tag Read(string address)
        {
            ThrowIfDisposed();
            EnsureConnected();

            var tag = new Tag(address);
            ReadTagValue(tag);
            return tag;
        }

        /// <summary>
        /// Reads multiple tags from the PLC.
        /// </summary>
        /// <param name="addresses">The tag addresses to read.</param>
        /// <returns>The tags with their values populated.</returns>
        public IList<Tag> Read(params string[] addresses)
        {
            ThrowIfDisposed();
            EnsureConnected();

            var tags = new List<Tag>();
            foreach (var address in addresses)
            {
                var tag = new Tag(address);
                ReadTagValue(tag);
                tags.Add(tag);
            }
            return tags;
        }

        /// <summary>
        /// Writes a value to a tag in the PLC.
        /// </summary>
        /// <param name="address">The tag address (e.g., "N7:0").</param>
        /// <param name="value">The value to write.</param>
        /// <returns>True if the write was successful.</returns>
        public bool Write(string address, object value)
        {
            ThrowIfDisposed();
            EnsureConnected();

            var tag = new Tag(address);
            WriteTagValue(tag, value);
            return true;
        }

        /// <summary>
        /// Writes multiple tag values to the PLC.
        /// </summary>
        /// <param name="tagsAndValues">Pairs of tag addresses and values.</param>
        /// <returns>True if all writes were successful.</returns>
        public bool Write(params (string address, object value)[] tagsAndValues)
        {
            ThrowIfDisposed();
            EnsureConnected();

            foreach (var (address, value) in tagsAndValues)
            {
                var tag = new Tag(address);
                WriteTagValue(tag, value);
            }
            return true;
        }

        /// <summary>
        /// Reads a single tag from the PLC asynchronously.
        /// </summary>
        /// <param name="address">The tag address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The tag with its value populated.</returns>
        public async Task<Tag> ReadAsync(string address, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            EnsureConnected();

            var tag = new Tag(address);
            await ReadTagValueAsync(tag, cancellationToken).ConfigureAwait(false);
            return tag;
        }

        /// <summary>
        /// Reads multiple tags from the PLC asynchronously.
        /// </summary>
        /// <param name="addresses">The tag addresses to read.</param>
        /// <returns>The tags with their values populated.</returns>
        public async Task<IList<Tag>> ReadAsync(params string[] addresses)
        {
            ThrowIfDisposed();
            EnsureConnected();

            var tags = new List<Tag>();
            foreach (var address in addresses)
            {
                var tag = new Tag(address);
                await ReadTagValueAsync(tag, CancellationToken.None).ConfigureAwait(false);
                tags.Add(tag);
            }
            return tags;
        }

        /// <summary>
        /// Writes a value to a tag in the PLC asynchronously.
        /// </summary>
        /// <param name="address">The tag address.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the write was successful.</returns>
        public async Task<bool> WriteAsync(string address, object value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            EnsureConnected();

            var tag = new Tag(address);
            await WriteTagValueAsync(tag, value, cancellationToken).ConfigureAwait(false);
            return true;
        }

        private void ReadTagValue(Tag tag)
        {
            // Build PCCC read request
            byte readLength = (byte)tag.ElementSize;
            if (tag.IsBit)
            {
                readLength = 2; // Read the whole word for bit access
            }

            var pcccRequest = PcccProtocol.BuildTypedRead(
                tag.FileTypeCode,
                tag.FileNumber,
                tag.ElementNumber,
                tag.SubElement,
                readLength);

            // Wrap in Execute PCCC CIP request
            var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);

            // Send via SendRRData and get response
            var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
            var response = SendReceive(request);
            var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

            // Parse PCCC reply
            var pcccReply = PcccProtocol.ParseReply(cipReply.Data);

            pcccReply.ThrowIfError($"Read failed for {tag.Address}");

            // Decode value based on file type
            tag.Value = DecodeValue(tag, pcccReply.Data);
        }

        private void WriteTagValue(Tag tag, object value)
        {
            if (tag.IsBit)
            {
                // For bit writes, use masks to set/clear specific bit
                var bitValue = Convert.ToBoolean(value);
                var bitMask = (ushort)(1 << tag.BitNumber!.Value);

                ushort orMask = bitValue ? bitMask : (ushort)0;
                ushort andMask = bitValue ? (ushort)0xFFFF : (ushort)(~bitMask);

                var pcccRequest = PcccProtocol.BuildMaskedWrite(
                    tag.FileTypeCode,
                    tag.FileNumber,
                    tag.ElementNumber,
                    tag.SubElement,
                    orMask,
                    andMask);

                var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);
                var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
                var response = SendReceive(request);
                var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

                var pcccReply = PcccProtocol.ParseReply(cipReply.Data);
                pcccReply.ThrowIfError($"Write failed for {tag.Address}");
                return;
            }

            // For non-bit writes, use Protected Typed Logical Write (0xAA)
            var data = EncodeValue(tag, value);

            // For 16-bit values (Integer, Bit word, etc.)
            if (data.Length == 2)
            {
                var pcccRequest = PcccProtocol.BuildTypedWrite(
                    tag.FileTypeCode,
                    tag.FileNumber,
                    tag.ElementNumber,
                    tag.SubElement,
                    data);

                var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);
                var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
                var response = SendReceive(request);
                var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

                var pcccReply = PcccProtocol.ParseReply(cipReply.Data);
                pcccReply.ThrowIfError($"Write failed for {tag.Address}");
                return;
            }

            // For 32-bit and larger values (Float, Long, String), write word by word
            if (data.Length >= 4)
            {
                // Write in 2-byte chunks using masked write
                for (int i = 0; i < data.Length; i += 2)
                {
                    byte subElement = (byte)(tag.SubElement + (i / 2));
                    ushort orMask = (ushort)(data[i] | (data[i + 1] << 8));
                    ushort andMask = 0x0000;

                    var pcccRequest = PcccProtocol.BuildMaskedWrite(
                        tag.FileTypeCode,
                        tag.FileNumber,
                        tag.ElementNumber,
                        subElement,
                        orMask,
                        andMask);

                    var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);
                    var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
                    var response = SendReceive(request);
                    var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

                    var pcccReply = PcccProtocol.ParseReply(cipReply.Data);
                    pcccReply.ThrowIfError($"Write failed for {tag.Address} at offset {i}");
                }
                return;
            }

            throw new RequestException($"Unsupported data length {data.Length} for write to {tag.Address}");
        }

        private async Task ReadTagValueAsync(Tag tag, CancellationToken cancellationToken)
        {
            byte readLength = (byte)tag.ElementSize;
            if (tag.IsBit)
            {
                readLength = 2;
            }

            var pcccRequest = PcccProtocol.BuildTypedRead(
                tag.FileTypeCode,
                tag.FileNumber,
                tag.ElementNumber,
                tag.SubElement,
                readLength);

            var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);
            var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
            var response = await SendReceiveAsync(request, cancellationToken).ConfigureAwait(false);
            var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

            var pcccReply = PcccProtocol.ParseReply(cipReply.Data);
            pcccReply.ThrowIfError($"Read failed for {tag.Address}");

            tag.Value = DecodeValue(tag, pcccReply.Data);
        }

        private async Task WriteTagValueAsync(Tag tag, object value, CancellationToken cancellationToken)
        {
            // SLC/MicroLogix uses Protected Typed Logical MASKED Write (0xAB) for all writes

            if (tag.IsBit)
            {
                var bitValue = Convert.ToBoolean(value);
                var bitMask = (ushort)(1 << tag.BitNumber!.Value);

                ushort orMask = bitValue ? bitMask : (ushort)0;
                ushort andMask = bitValue ? (ushort)0xFFFF : (ushort)(~bitMask);

                var pcccRequest = PcccProtocol.BuildMaskedWrite(
                    tag.FileTypeCode,
                    tag.FileNumber,
                    tag.ElementNumber,
                    tag.SubElement,
                    orMask,
                    andMask);

                var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);
                var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
                var response = await SendReceiveAsync(request, cancellationToken).ConfigureAwait(false);
                var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

                var pcccReply = PcccProtocol.ParseReply(cipReply.Data);
                pcccReply.ThrowIfError($"Write failed for {tag.Address}");
                return;
            }

            var data = EncodeValue(tag, value);

            // For 16-bit values
            if (data.Length == 2)
            {
                ushort orMask = (ushort)(data[0] | (data[1] << 8));
                ushort andMask = 0x0000;

                var pcccRequest = PcccProtocol.BuildMaskedWrite(
                    tag.FileTypeCode,
                    tag.FileNumber,
                    tag.ElementNumber,
                    tag.SubElement,
                    orMask,
                    andMask);

                var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);
                var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
                var response = await SendReceiveAsync(request, cancellationToken).ConfigureAwait(false);
                var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

                var pcccReply = PcccProtocol.ParseReply(cipReply.Data);
                pcccReply.ThrowIfError($"Write failed for {tag.Address}");
                return;
            }

            // For 32-bit and larger values, write word by word
            if (data.Length >= 4)
            {
                for (int i = 0; i < data.Length; i += 2)
                {
                    byte subElement = (byte)(tag.SubElement + (i / 2));
                    ushort orMask = (ushort)(data[i] | (data[i + 1] << 8));
                    ushort andMask = 0x0000;

                    var pcccRequest = PcccProtocol.BuildMaskedWrite(
                        tag.FileTypeCode,
                        tag.FileNumber,
                        tag.ElementNumber,
                        subElement,
                        orMask,
                        andMask);

                    var cipRequest = PcccProtocol.BuildExecutePcccRequest(pcccRequest);
                    var request = SendRRDataPacket.BuildRequest(_cipDriver.SessionHandle, cipRequest);
                    var response = await SendReceiveAsync(request, cancellationToken).ConfigureAwait(false);
                    var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ExecutePCCC);

                    var pcccReply = PcccProtocol.ParseReply(cipReply.Data);
                    pcccReply.ThrowIfError($"Write failed for {tag.Address} at offset {i}");
                }
                return;
            }

            throw new RequestException($"Unsupported data length {data.Length} for write to {tag.Address}");
        }

        private object? DecodeValue(Tag tag, byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            // Handle bit extraction
            if (tag.IsBit)
            {
                var wordValue = INT.Instance.Decode(data);
                return (wordValue & (1 << tag.BitNumber!.Value)) != 0;
            }

            return tag.FileType switch
            {
                "N" => INT.Instance.Decode(data),
                "F" => REAL.Instance.Decode(data),
                "B" => INT.Instance.Decode(data), // Return word value for non-bit access
                "L" => DINT.Instance.Decode(data),
                "T" or "C" or "R" => DecodeTimerCounterControl(tag, data),
                "O" or "I" or "S" => INT.Instance.Decode(data),
                "ST" => DecodeString(data),
                "A" => DecodeAscii(data),
                _ => data // Return raw bytes for unknown types
            };
        }

        private object DecodeTimerCounterControl(Tag tag, byte[] data)
        {
            // For Timer/Counter/Control, if sub-element is specified, return that word
            if (tag.SubElement > 0)
            {
                return INT.Instance.Decode(data);
            }

            // Otherwise return the full structure as an array
            if (data.Length >= 6)
            {
                return new short[]
                {
                    INT.Instance.Decode(new[] { data[0], data[1] }),
                    INT.Instance.Decode(new[] { data[2], data[3] }),
                    INT.Instance.Decode(new[] { data[4], data[5] })
                };
            }

            return INT.Instance.Decode(data);
        }

        private string DecodeString(byte[] data)
        {
            if (data.Length < 2)
                return string.Empty;

            // First two bytes are length
            var length = data[0] | (data[1] << 8);
            length = Math.Min(length, data.Length - 2);

            if (length <= 0)
                return string.Empty;

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)data[2 + i];
            }
            return new string(chars);
        }

        private string DecodeAscii(byte[] data)
        {
            if (data.Length < 2)
                return string.Empty;

            var chars = new char[2];
            chars[0] = (char)data[0];
            chars[1] = (char)data[1];
            return new string(chars).TrimEnd('\0');
        }

        private byte[] EncodeValue(Tag tag, object value)
        {
            return tag.FileType switch
            {
                "N" => INT.Instance.Encode(Convert.ToInt16(value)),
                "F" => REAL.Instance.Encode(Convert.ToSingle(value)),
                "B" => INT.Instance.Encode(Convert.ToInt16(value)),
                "L" => DINT.Instance.Encode(Convert.ToInt32(value)),
                "O" or "I" or "S" => INT.Instance.Encode(Convert.ToInt16(value)),
                "ST" => EncodeString(value?.ToString() ?? string.Empty),
                "A" => EncodeAscii(value?.ToString() ?? string.Empty),
                _ => throw new RequestException($"Cannot encode value for file type: {tag.FileType}")
            };
        }

        private byte[] EncodeString(string value)
        {
            var maxLength = 82;
            var length = Math.Min(value.Length, maxLength);
            var result = new byte[2 + length];

            result[0] = (byte)(length & 0xFF);
            result[1] = (byte)((length >> 8) & 0xFF);

            for (int i = 0; i < length; i++)
            {
                result[2 + i] = (byte)value[i];
            }

            return result;
        }

        private byte[] EncodeAscii(string value)
        {
            var result = new byte[2];
            if (value.Length > 0)
                result[0] = (byte)value[0];
            if (value.Length > 1)
                result[1] = (byte)value[1];
            return result;
        }

        private byte[] SendReceive(byte[] packet)
        {
            // Use reflection or internal method to access CipDriver's send/receive
            // For now, we'll implement this directly
            var transport = GetTransport();
            transport.Send(packet);

            var header = transport.Receive(Constants.HeaderSize);
            if (header.Length < Constants.HeaderSize)
            {
                throw new CommException($"Incomplete header received: {header.Length} bytes");
            }

            var dataLength = header[2] | (header[3] << 8);
            if (dataLength == 0)
            {
                return header;
            }

            var data = transport.Receive(dataLength);
            if (data.Length < dataLength)
            {
                throw new CommException($"Incomplete data received: {data.Length} bytes, expected {dataLength}");
            }

            var response = new byte[Constants.HeaderSize + dataLength];
            Array.Copy(header, response, Constants.HeaderSize);
            Array.Copy(data, 0, response, Constants.HeaderSize, dataLength);

            return response;
        }

        private async Task<byte[]> SendReceiveAsync(byte[] packet, CancellationToken cancellationToken)
        {
            var transport = GetTransport();
            await transport.SendAsync(packet, cancellationToken).ConfigureAwait(false);

            var header = await transport.ReceiveAsync(Constants.HeaderSize, cancellationToken).ConfigureAwait(false);
            if (header.Length < Constants.HeaderSize)
            {
                throw new CommException($"Incomplete header received: {header.Length} bytes");
            }

            var dataLength = header[2] | (header[3] << 8);
            if (dataLength == 0)
            {
                return header;
            }

            var data = await transport.ReceiveAsync(dataLength, cancellationToken).ConfigureAwait(false);
            if (data.Length < dataLength)
            {
                throw new CommException($"Incomplete data received: {data.Length} bytes, expected {dataLength}");
            }

            var response = new byte[Constants.HeaderSize + dataLength];
            Array.Copy(header, response, Constants.HeaderSize);
            Array.Copy(data, 0, response, Constants.HeaderSize, dataLength);

            return response;
        }

        private ITransport GetTransport()
        {
            // This is a workaround - in production we'd refactor CipDriver to expose this
            var field = typeof(CipDriver).GetField("_transport",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (ITransport)field!.GetValue(_cipDriver)!;
        }

        private void EnsureConnected()
        {
            if (!Connected)
            {
                throw new CommException("Not connected. Call Open() first.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SLCDriver));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and managed resources.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                    if (_ownsCipDriver)
                    {
                        _cipDriver.Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}
