// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using CSComm3.SLC.Exceptions;

namespace CSComm3.SLC.Packets
{
    /// <summary>
    /// Builds SendRRData (Send Request/Response Data) packets for unconnected messaging.
    /// </summary>
    /// <remarks>
    /// SendRRData is used for unconnected messaging over EtherNet/IP.
    /// The data structure follows CPF (Common Packet Format):
    /// - Interface Handle (4 bytes): 0x00000000 for CIP
    /// - Timeout (2 bytes): typically 0x0000
    /// - Item Count (2 bytes): number of CPF items
    /// - CPF Items (variable)
    /// </remarks>
    public static class SendRRDataPacket
    {
        /// <summary>
        /// CPF Type: Null Address Item.
        /// </summary>
        public const ushort CpfNullAddress = 0x0000;

        /// <summary>
        /// CPF Type: Unconnected Data Item.
        /// </summary>
        public const ushort CpfUnconnectedData = 0x00B2;

        /// <summary>
        /// CPF Type: Connected Address Item.
        /// </summary>
        public const ushort CpfConnectedAddress = 0x00A1;

        /// <summary>
        /// CPF Type: Connected Data Item.
        /// </summary>
        public const ushort CpfConnectedData = 0x00B1;

        /// <summary>
        /// Builds a SendRRData request packet with CPF items.
        /// </summary>
        /// <param name="sessionHandle">The session handle.</param>
        /// <param name="cipData">The CIP service request data.</param>
        /// <param name="timeout">The timeout value (default 0).</param>
        /// <returns>The complete packet bytes.</returns>
        public static byte[] BuildRequest(uint sessionHandle, byte[] cipData, ushort timeout = 0)
        {
            var packet = new RequestPacket
            {
                Command = EncapsulationCommands.SendRRData,
                SessionHandle = sessionHandle
            };

            // Interface Handle (4 bytes): 0x00000000 for CIP
            packet.AddUInt32(0);

            // Timeout (2 bytes)
            packet.AddUInt16(timeout);

            // Item Count (2 bytes): 2 items (Null Address + Unconnected Data)
            packet.AddUInt16(2);

            // Item 1: Null Address Item
            packet.AddUInt16(CpfNullAddress); // Type ID
            packet.AddUInt16(0);              // Length (0 for null address)

            // Item 2: Unconnected Data Item
            packet.AddUInt16(CpfUnconnectedData);       // Type ID
            packet.AddUInt16((ushort)cipData.Length);   // Length
            packet.Add(cipData);                         // Data

            return packet.Build();
        }

        /// <summary>
        /// Parses a SendRRData response and extracts the CIP reply data.
        /// </summary>
        /// <param name="responseData">The raw response data.</param>
        /// <returns>The CIP reply data.</returns>
        public static byte[] ParseResponse(byte[] responseData)
        {
            var response = new ResponsePacket(responseData);
            response.ThrowIfError("SendRRData failed");

            // Interface Handle (4 bytes)
            response.ReadUInt32();

            // Timeout (2 bytes)
            response.ReadUInt16();

            // Item Count (2 bytes)
            var itemCount = response.ReadUInt16();

            if (itemCount < 2)
            {
                throw new ResponseException("SendRRData response missing expected items");
            }

            // Item 1: Should be Null Address Item
            var item1Type = response.ReadUInt16();
            var item1Length = response.ReadUInt16();
            if (item1Length > 0)
            {
                response.Skip(item1Length);
            }

            // Item 2: Should be Unconnected Data Item
            var item2Type = response.ReadUInt16();
            var item2Length = response.ReadUInt16();

            if (item2Type != CpfUnconnectedData)
            {
                throw new ResponseException($"Unexpected CPF item type: 0x{item2Type:X4}, expected 0x{CpfUnconnectedData:X4}");
            }

            return response.ReadBytes(item2Length);
        }

        /// <summary>
        /// Parses a SendRRData response and validates the CIP service reply.
        /// </summary>
        /// <param name="responseData">The raw response data.</param>
        /// <param name="expectedService">The expected service code (with reply bit set).</param>
        /// <returns>The CIP reply data (after service and status bytes).</returns>
        public static CipReply ParseCipReply(byte[] responseData, byte expectedService)
        {
            var cipData = ParseResponse(responseData);

            if (cipData.Length < 4)
            {
                throw new ResponseException("CIP reply too short");
            }

            var reply = new CipReply
            {
                Service = cipData[0],
                Reserved = cipData[1],
                Status = cipData[2],
                ExtendedStatusSize = cipData[3]
            };

            // Read extended status if present
            var extStatusBytes = reply.ExtendedStatusSize * 2;
            var dataOffset = 4 + extStatusBytes;

            if (extStatusBytes > 0 && cipData.Length >= dataOffset)
            {
                reply.ExtendedStatus = new ushort[reply.ExtendedStatusSize];
                for (int i = 0; i < reply.ExtendedStatusSize; i++)
                {
                    reply.ExtendedStatus[i] = (ushort)(cipData[4 + i * 2] | (cipData[5 + i * 2] << 8));
                }
            }

            // Extract data after status
            if (cipData.Length > dataOffset)
            {
                reply.Data = new byte[cipData.Length - dataOffset];
                Array.Copy(cipData, dataOffset, reply.Data, 0, reply.Data.Length);
            }
            else
            {
                reply.Data = Array.Empty<byte>();
            }

            // Verify service (reply bit should be set)
            var expectedReply = (byte)(expectedService | 0x80);
            if (reply.Service != expectedReply)
            {
                throw new ResponseException($"Unexpected service reply: 0x{reply.Service:X2}, expected 0x{expectedReply:X2}");
            }

            // Check for errors
            if (reply.Status != 0)
            {
                var extStatus = reply.ExtendedStatus?.Length > 0 ? reply.ExtendedStatus[0] : (ushort?)null;
                throw new ResponseException(
                    $"CIP error: Status 0x{reply.Status:X2}",
                    reply.Status,
                    extStatus);
            }

            return reply;
        }
    }

    /// <summary>
    /// Represents a parsed CIP reply.
    /// </summary>
    public class CipReply
    {
        /// <summary>
        /// Gets or sets the service code (with reply bit set).
        /// </summary>
        public byte Service { get; set; }

        /// <summary>
        /// Gets or sets the reserved byte.
        /// </summary>
        public byte Reserved { get; set; }

        /// <summary>
        /// Gets or sets the general status code.
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// Gets or sets the size of extended status in words.
        /// </summary>
        public byte ExtendedStatusSize { get; set; }

        /// <summary>
        /// Gets or sets the extended status words.
        /// </summary>
        public ushort[]? ExtendedStatus { get; set; }

        /// <summary>
        /// Gets or sets the reply data.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets a value indicating whether the reply indicates success.
        /// </summary>
        public bool IsSuccess => Status == 0;
    }
}
