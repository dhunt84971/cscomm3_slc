// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using CSComm3.SLC.Exceptions;

namespace CSComm3.SLC.Packets
{
    /// <summary>
    /// Builds SendUnitData packets for connected messaging.
    /// </summary>
    /// <remarks>
    /// SendUnitData is used for connected messaging over EtherNet/IP.
    /// It requires an established CIP connection with a connection ID.
    /// The data structure follows CPF (Common Packet Format):
    /// - Interface Handle (4 bytes): 0x00000000 for CIP
    /// - Timeout (2 bytes): typically 0x0000 (not used for connected)
    /// - Item Count (2 bytes): number of CPF items (typically 2)
    /// - CPF Items: Connected Address Item + Connected Data Item
    /// </remarks>
    public static class SendUnitDataPacket
    {
        /// <summary>
        /// Builds a SendUnitData request packet for connected messaging.
        /// </summary>
        /// <param name="sessionHandle">The session handle.</param>
        /// <param name="connectionId">The O->T connection ID from ForwardOpen.</param>
        /// <param name="sequenceNumber">The sequence counter for this connection.</param>
        /// <param name="cipData">The CIP service request data.</param>
        /// <returns>The complete packet bytes.</returns>
        public static byte[] BuildRequest(uint sessionHandle, uint connectionId, ushort sequenceNumber, byte[] cipData)
        {
            var packet = new RequestPacket
            {
                Command = EncapsulationCommands.SendUnitData,
                SessionHandle = sessionHandle
            };

            // Interface Handle (4 bytes): 0x00000000 for CIP
            packet.AddUInt32(0);

            // Timeout (2 bytes): 0 for connected messaging
            packet.AddUInt16(0);

            // Item Count (2 bytes): 2 items (Connected Address + Connected Data)
            packet.AddUInt16(2);

            // Item 1: Connected Address Item (0x00A1)
            packet.AddUInt16(SendRRDataPacket.CpfConnectedAddress); // Type ID
            packet.AddUInt16(4);                                     // Length (4 bytes for connection ID)
            packet.AddUInt32(connectionId);                          // Connection ID

            // Item 2: Connected Data Item (0x00B1)
            // Length includes sequence number (2 bytes) + CIP data
            packet.AddUInt16(SendRRDataPacket.CpfConnectedData);           // Type ID
            packet.AddUInt16((ushort)(2 + cipData.Length));                // Length
            packet.AddUInt16(sequenceNumber);                              // Sequence Number
            packet.Add(cipData);                                           // CIP Data

            return packet.Build();
        }

        /// <summary>
        /// Parses a SendUnitData response and extracts the CIP reply data.
        /// </summary>
        /// <param name="responseData">The raw response data.</param>
        /// <returns>The CIP reply data (after sequence number).</returns>
        public static SendUnitDataResponse ParseResponse(byte[] responseData)
        {
            var response = new ResponsePacket(responseData);
            response.ThrowIfError("SendUnitData failed");

            var result = new SendUnitDataResponse();

            // Interface Handle (4 bytes)
            response.ReadUInt32();

            // Timeout (2 bytes)
            response.ReadUInt16();

            // Item Count (2 bytes)
            var itemCount = response.ReadUInt16();

            if (itemCount < 2)
            {
                throw new ResponseException("SendUnitData response missing expected items");
            }

            // Item 1: Connected Address Item
            var item1Type = response.ReadUInt16();
            var item1Length = response.ReadUInt16();

            if (item1Type == SendRRDataPacket.CpfConnectedAddress && item1Length >= 4)
            {
                result.ConnectionId = response.ReadUInt32();
                if (item1Length > 4)
                {
                    response.Skip(item1Length - 4);
                }
            }
            else if (item1Length > 0)
            {
                response.Skip(item1Length);
            }

            // Item 2: Connected Data Item
            var item2Type = response.ReadUInt16();
            var item2Length = response.ReadUInt16();

            if (item2Type != SendRRDataPacket.CpfConnectedData)
            {
                throw new ResponseException($"Unexpected CPF item type: 0x{item2Type:X4}, expected 0x{SendRRDataPacket.CpfConnectedData:X4}");
            }

            if (item2Length < 2)
            {
                throw new ResponseException("Connected data item too short");
            }

            // Sequence Number (2 bytes)
            result.SequenceNumber = response.ReadUInt16();

            // CIP Data
            result.CipData = response.ReadBytes(item2Length - 2);

            return result;
        }

        /// <summary>
        /// Parses a SendUnitData response and validates the CIP service reply.
        /// </summary>
        /// <param name="responseData">The raw response data.</param>
        /// <param name="expectedService">The expected service code (without reply bit).</param>
        /// <returns>The parsed CIP reply.</returns>
        public static CipReply ParseCipReply(byte[] responseData, byte expectedService)
        {
            var unitDataResponse = ParseResponse(responseData);
            var cipData = unitDataResponse.CipData;

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
    /// Represents a parsed SendUnitData response.
    /// </summary>
    public class SendUnitDataResponse
    {
        /// <summary>
        /// Gets or sets the connection ID from the response.
        /// </summary>
        public uint ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the sequence number from the response.
        /// </summary>
        public ushort SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the CIP data from the response.
        /// </summary>
        public byte[] CipData { get; set; } = Array.Empty<byte>();
    }
}
