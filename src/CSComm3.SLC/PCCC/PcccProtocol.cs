// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Collections.Generic;
using System.IO;

namespace CSComm3.SLC.PCCC
{
    /// <summary>
    /// Builds and parses PCCC (Programmable Controller Communication Commands) packets.
    /// </summary>
    public static class PcccProtocol
    {
        private static ushort _transactionId;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the next transaction ID.
        /// </summary>
        /// <returns>The next transaction ID.</returns>
        public static ushort GetNextTransactionId()
        {
            lock (_lock)
            {
                return _transactionId++;
            }
        }

        /// <summary>
        /// Builds an Execute PCCC CIP request.
        /// </summary>
        /// <param name="pcccData">The PCCC command data.</param>
        /// <returns>The complete CIP request bytes.</returns>
        public static byte[] BuildExecutePcccRequest(byte[] pcccData)
        {
            var path = CIP.CipPath.BuildPcccPath();

            using var ms = new MemoryStream();

            // CIP Service: Execute PCCC (0x4B)
            ms.WriteByte(CipServices.ExecutePCCC);

            // Path size in words
            ms.WriteByte((byte)(path.Length / 2));

            // Path
            ms.Write(path, 0, path.Length);

            // PCCC header
            // Requestor ID Length (7 bytes)
            ms.WriteByte(0x07);

            // Requestor ID Vendor (2 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            // Requestor ID Serial (4 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            // PCCC data
            ms.Write(pcccData, 0, pcccData.Length);

            return ms.ToArray();
        }

        /// <summary>
        /// Builds a PCCC Protected Typed Logical Read request.
        /// </summary>
        /// <param name="fileType">The file type code.</param>
        /// <param name="fileNumber">The file number.</param>
        /// <param name="elementNumber">The element number.</param>
        /// <param name="subElement">The sub-element number.</param>
        /// <param name="readLength">The number of bytes to read.</param>
        /// <returns>The PCCC command bytes.</returns>
        public static byte[] BuildTypedRead(byte fileType, byte fileNumber, byte elementNumber, byte subElement, byte readLength)
        {
            var transactionId = GetNextTransactionId();

            using var ms = new MemoryStream();

            // PCCC Command (0x0F)
            ms.WriteByte(PcccCommands.Command);

            // STS (status, 0 for request)
            ms.WriteByte(0x00);

            // Transaction ID (2 bytes, little-endian)
            ms.WriteByte((byte)(transactionId & 0xFF));
            ms.WriteByte((byte)((transactionId >> 8) & 0xFF));

            // Function Code: Protected Typed Logical Read with 3 Address Fields
            ms.WriteByte(PcccCommands.ProtectedTypedLogicalRead3Address);

            // Read Length (bytes to read)
            ms.WriteByte(readLength);

            // File Number
            ms.WriteByte(fileNumber);

            // File Type
            ms.WriteByte(fileType);

            // Element Number
            ms.WriteByte(elementNumber);

            // Sub-Element Number
            ms.WriteByte(subElement);

            return ms.ToArray();
        }

        /// <summary>
        /// Builds a PCCC Protected Typed Logical Write request.
        /// </summary>
        /// <param name="fileType">The file type code.</param>
        /// <param name="fileNumber">The file number.</param>
        /// <param name="elementNumber">The element number.</param>
        /// <param name="subElement">The sub-element number.</param>
        /// <param name="data">The data to write.</param>
        /// <returns>The PCCC command bytes.</returns>
        public static byte[] BuildTypedWrite(byte fileType, byte fileNumber, byte elementNumber, byte subElement, byte[] data)
        {
            var transactionId = GetNextTransactionId();

            using var ms = new MemoryStream();

            // PCCC Command (0x0F)
            ms.WriteByte(PcccCommands.Command);

            // STS (status, 0 for request)
            ms.WriteByte(0x00);

            // Transaction ID (2 bytes, little-endian)
            ms.WriteByte((byte)(transactionId & 0xFF));
            ms.WriteByte((byte)((transactionId >> 8) & 0xFF));

            // Function Code: Protected Typed Logical Write with 3 Address Fields
            ms.WriteByte(PcccCommands.ProtectedTypedLogicalWrite3Address);

            // Write Length (bytes to write)
            ms.WriteByte((byte)data.Length);

            // File Number
            ms.WriteByte(fileNumber);

            // File Type
            ms.WriteByte(fileType);

            // Element Number
            ms.WriteByte(elementNumber);

            // Sub-Element Number
            ms.WriteByte(subElement);

            // Data to write
            ms.Write(data, 0, data.Length);

            return ms.ToArray();
        }

        /// <summary>
        /// Builds a PCCC Protected Typed Logical Masked Write request.
        /// </summary>
        /// <param name="fileType">The file type code.</param>
        /// <param name="fileNumber">The file number.</param>
        /// <param name="elementNumber">The element number.</param>
        /// <param name="subElement">The sub-element number.</param>
        /// <param name="orMask">The OR mask (bits to set).</param>
        /// <param name="andMask">The AND mask (bits to clear).</param>
        /// <returns>The PCCC command bytes.</returns>
        public static byte[] BuildMaskedWrite(byte fileType, byte fileNumber, byte elementNumber, byte subElement, ushort orMask, ushort andMask)
        {
            var transactionId = GetNextTransactionId();

            using var ms = new MemoryStream();

            // PCCC Command (0x0F)
            ms.WriteByte(PcccCommands.Command);

            // STS (status, 0 for request)
            ms.WriteByte(0x00);

            // Transaction ID (2 bytes, little-endian)
            ms.WriteByte((byte)(transactionId & 0xFF));
            ms.WriteByte((byte)((transactionId >> 8) & 0xFF));

            // Function Code: Protected Typed Logical Masked Write
            ms.WriteByte(PcccCommands.ProtectedTypedLogicalMaskedWrite);

            // Write Length (4 bytes: 2 for OR mask + 2 for AND mask)
            ms.WriteByte(0x04);

            // File Number
            ms.WriteByte(fileNumber);

            // File Type
            ms.WriteByte(fileType);

            // Element Number
            ms.WriteByte(elementNumber);

            // Sub-Element Number
            ms.WriteByte(subElement);

            // OR Mask (little-endian)
            ms.WriteByte((byte)(orMask & 0xFF));
            ms.WriteByte((byte)((orMask >> 8) & 0xFF));

            // AND Mask (little-endian)
            ms.WriteByte((byte)(andMask & 0xFF));
            ms.WriteByte((byte)((andMask >> 8) & 0xFF));

            return ms.ToArray();
        }

        /// <summary>
        /// Parses a PCCC reply and returns the data portion.
        /// </summary>
        /// <param name="reply">The PCCC reply bytes (including echoed Requestor ID).</param>
        /// <returns>The data portion of the reply.</returns>
        public static PcccReply ParseReply(byte[] reply)
        {
            if (reply == null || reply.Length < 4)
            {
                throw new Exceptions.DataException("PCCC reply too short");
            }

            // The reply includes the echoed Requestor ID before the actual PCCC reply
            // Format: [Requestor ID Length (1)] [Requestor ID Data (length-1)] [PCCC Reply...]
            // The Requestor ID Length includes itself, so if it's 0x07, skip 7 bytes total
            int requestorIdLength = reply[0];
            if (requestorIdLength < 1 || requestorIdLength > reply.Length - 4)
            {
                throw new Exceptions.DataException($"Invalid Requestor ID length: {requestorIdLength}");
            }

            int pcccOffset = requestorIdLength;

            if (reply.Length < pcccOffset + 4)
            {
                throw new Exceptions.DataException("PCCC reply too short after Requestor ID");
            }

            var result = new PcccReply
            {
                Command = reply[pcccOffset],
                Status = reply[pcccOffset + 1],
                TransactionId = (ushort)(reply[pcccOffset + 2] | (reply[pcccOffset + 3] << 8))
            };

            int dataOffset = pcccOffset + 4;

            // Check for error status
            if (result.Status != 0)
            {
                // Extended status byte present if STS != 0
                if (reply.Length > dataOffset)
                {
                    result.ExtendedStatus = reply[dataOffset];
                    dataOffset++;
                    if (reply.Length > dataOffset)
                    {
                        result.Data = new byte[reply.Length - dataOffset];
                        Array.Copy(reply, dataOffset, result.Data, 0, result.Data.Length);
                    }
                }
            }
            else
            {
                // Success - data starts after the 4-byte PCCC header
                if (reply.Length > dataOffset)
                {
                    result.Data = new byte[reply.Length - dataOffset];
                    Array.Copy(reply, dataOffset, result.Data, 0, result.Data.Length);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a parsed PCCC reply.
    /// </summary>
    public class PcccReply
    {
        /// <summary>
        /// Gets or sets the command code.
        /// </summary>
        public byte Command { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public ushort TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the extended status code.
        /// </summary>
        public byte? ExtendedStatus { get; set; }

        /// <summary>
        /// Gets or sets the data portion.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets a value indicating whether the reply indicates success.
        /// </summary>
        public bool IsSuccess => Status == 0;

        /// <summary>
        /// Throws an exception if the reply indicates an error.
        /// </summary>
        /// <param name="message">The error message prefix.</param>
        public void ThrowIfError(string message = "PCCC error")
        {
            if (!IsSuccess)
            {
                var statusDesc = GetStatusDescription(Status, ExtendedStatus);
                throw new Exceptions.ResponseException(
                    $"{message}: {statusDesc}",
                    Status,
                    ExtendedStatus);
            }
        }

        /// <summary>
        /// Gets a description for the status code.
        /// </summary>
        /// <param name="status">The status code.</param>
        /// <param name="extStatus">The extended status code.</param>
        /// <returns>A description of the status.</returns>
        public static string GetStatusDescription(byte status, byte? extStatus)
        {
            return status switch
            {
                0x00 => "Success",
                0x01 => "DST node out of buffer space",
                0x02 => "Cannot guarantee delivery - link layer out of buffer",
                0x03 => "Duplicate token holder detected",
                0x04 => "Local port is disconnected",
                0x05 => "Application layer timed out waiting for response",
                0x06 => "Duplicate node detected",
                0x07 => "Station is offline",
                0x08 => "Hardware fault",
                0x10 => "Illegal command or format",
                0x20 => "Host has problem and will not communicate",
                0x30 => "Remote node host is missing",
                0x40 => "Host could not complete function",
                0x50 => "Access denied",
                0x60 => "Function not available",
                0x70 => "Controller in Program mode",
                0x80 => "Compatibility mode file missing",
                0xF0 => "File is open for write by another device",
                _ => $"Unknown status 0x{status:X2}" + (extStatus.HasValue ? $" (ext: 0x{extStatus.Value:X2})" : "")
            };
        }
    }
}
