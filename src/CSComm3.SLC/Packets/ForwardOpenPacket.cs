// CSComm3.SLC - C# SLC PLC Communication Library

using System;
using System.IO;
using CSComm3.SLC.Exceptions;

namespace CSComm3.SLC.Packets
{
    /// <summary>
    /// Builds and parses CIP Forward Open packets for establishing connections.
    /// </summary>
    public static class ForwardOpenPacket
    {
        private static uint _connectionIdCounter = 0x10000;
        private static readonly object _lock = new object();

        private static uint GetNextConnectionId()
        {
            lock (_lock)
            {
                return ++_connectionIdCounter;
            }
        }

        /// <summary>
        /// Connection parameters for Forward Open.
        /// </summary>
        public class ConnectionParams
        {
            public uint OTConnectionId { get; set; }
            public uint TOConnectionId { get; set; }
            public ushort ConnectionSerialNumber { get; set; }
            public ushort VendorId { get; set; } = 0x1337;
            public uint OriginatorSerialNumber { get; set; } = 0x12345678;
        }

        /// <summary>
        /// Builds a Forward Open request wrapped in SendRRData.
        /// </summary>
        /// <param name="sessionHandle">The session handle.</param>
        /// <param name="routePath">Optional route path for the target.</param>
        /// <returns>The Forward Open request packet and connection parameters.</returns>
        public static (byte[] packet, ConnectionParams parameters) BuildRequest(uint sessionHandle, byte[]? routePath = null)
        {
            var connParams = new ConnectionParams
            {
                OTConnectionId = GetNextConnectionId(),
                TOConnectionId = GetNextConnectionId(),
                ConnectionSerialNumber = (ushort)(GetNextConnectionId() & 0xFFFF)
            };

            // Build the Forward Open service request
            using var cipMs = new MemoryStream();

            // Service: Forward Open (0x54)
            cipMs.WriteByte(CipServices.ForwardOpen);

            // Path to Connection Manager (class 0x06, instance 0x01)
            var cmPath = new byte[] { 0x20, 0x06, 0x24, 0x01 };
            cipMs.WriteByte((byte)(cmPath.Length / 2)); // Path size in words
            cipMs.Write(cmPath, 0, cmPath.Length);

            // Priority/Time_tick: 0x0A (default)
            cipMs.WriteByte(0x0A);

            // Timeout_ticks: 0x0E (default)
            cipMs.WriteByte(0x0E);

            // O->T Network Connection ID (4 bytes, little-endian)
            cipMs.WriteByte((byte)(connParams.OTConnectionId & 0xFF));
            cipMs.WriteByte((byte)((connParams.OTConnectionId >> 8) & 0xFF));
            cipMs.WriteByte((byte)((connParams.OTConnectionId >> 16) & 0xFF));
            cipMs.WriteByte((byte)((connParams.OTConnectionId >> 24) & 0xFF));

            // T->O Network Connection ID (4 bytes, little-endian)
            cipMs.WriteByte((byte)(connParams.TOConnectionId & 0xFF));
            cipMs.WriteByte((byte)((connParams.TOConnectionId >> 8) & 0xFF));
            cipMs.WriteByte((byte)((connParams.TOConnectionId >> 16) & 0xFF));
            cipMs.WriteByte((byte)((connParams.TOConnectionId >> 24) & 0xFF));

            // Connection Serial Number (2 bytes, little-endian)
            cipMs.WriteByte((byte)(connParams.ConnectionSerialNumber & 0xFF));
            cipMs.WriteByte((byte)((connParams.ConnectionSerialNumber >> 8) & 0xFF));

            // Originator Vendor ID (2 bytes, little-endian)
            cipMs.WriteByte((byte)(connParams.VendorId & 0xFF));
            cipMs.WriteByte((byte)((connParams.VendorId >> 8) & 0xFF));

            // Originator Serial Number (4 bytes, little-endian)
            cipMs.WriteByte((byte)(connParams.OriginatorSerialNumber & 0xFF));
            cipMs.WriteByte((byte)((connParams.OriginatorSerialNumber >> 8) & 0xFF));
            cipMs.WriteByte((byte)((connParams.OriginatorSerialNumber >> 16) & 0xFF));
            cipMs.WriteByte((byte)((connParams.OriginatorSerialNumber >> 24) & 0xFF));

            // Connection Timeout Multiplier: 0x00
            cipMs.WriteByte(0x00);

            // Reserved (3 bytes)
            cipMs.WriteByte(0x00);
            cipMs.WriteByte(0x00);
            cipMs.WriteByte(0x00);

            // O->T RPI (Requested Packet Interval): 500ms = 500000 us
            uint otRpi = 500000;
            cipMs.WriteByte((byte)(otRpi & 0xFF));
            cipMs.WriteByte((byte)((otRpi >> 8) & 0xFF));
            cipMs.WriteByte((byte)((otRpi >> 16) & 0xFF));
            cipMs.WriteByte((byte)((otRpi >> 24) & 0xFF));

            // O->T Network Connection Parameters
            // Size: 500 bytes, Fixed, Point-to-Point, Priority Low, Type: NULL (no real-time data)
            ushort otParams = 0x43F4; // Standard connection parameters
            cipMs.WriteByte((byte)(otParams & 0xFF));
            cipMs.WriteByte((byte)((otParams >> 8) & 0xFF));

            // T->O RPI
            uint toRpi = 500000;
            cipMs.WriteByte((byte)(toRpi & 0xFF));
            cipMs.WriteByte((byte)((toRpi >> 8) & 0xFF));
            cipMs.WriteByte((byte)((toRpi >> 16) & 0xFF));
            cipMs.WriteByte((byte)((toRpi >> 24) & 0xFF));

            // T->O Network Connection Parameters
            ushort toParams = 0x43F4;
            cipMs.WriteByte((byte)(toParams & 0xFF));
            cipMs.WriteByte((byte)((toParams >> 8) & 0xFF));

            // Transport Type/Trigger: 0xA3 (Direction: Server, Production Trigger: Application Object, Transport Class: 3)
            cipMs.WriteByte(0xA3);

            // Connection Path Size (in words)
            // Path to PCCC object: Class 0x67, Instance 0x01
            var connPath = routePath ?? new byte[] { 0x20, 0x67, 0x24, 0x01 };
            cipMs.WriteByte((byte)(connPath.Length / 2));

            // Connection Path
            cipMs.Write(connPath, 0, connPath.Length);

            var cipRequest = cipMs.ToArray();

            // Wrap in SendRRData
            var packet = SendRRDataPacket.BuildRequest(sessionHandle, cipRequest);

            return (packet, connParams);
        }

        /// <summary>
        /// Parses a Forward Open response.
        /// </summary>
        /// <param name="response">The response packet.</param>
        /// <param name="originalParams">The original connection parameters.</param>
        /// <returns>The connection ID to use for SendUnitData.</returns>
        public static uint ParseResponse(byte[] response, ConnectionParams originalParams)
        {
            // Parse the SendRRData response first
            var cipReply = SendRRDataPacket.ParseCipReply(response, CipServices.ForwardOpen);

            if (cipReply.Status != 0)
            {
                throw new ResponseException(
                    $"Forward Open failed with status 0x{cipReply.Status:X2}",
                    cipReply.Status,
                    null);
            }

            // Parse the Forward Open reply data
            // Structure:
            // - O->T Connection ID (4 bytes)
            // - T->O Connection ID (4 bytes)
            // - Connection Serial Number (2 bytes)
            // - Originator Vendor ID (2 bytes)
            // - Originator Serial Number (4 bytes)
            // - O->T API (4 bytes)
            // - T->O API (4 bytes)
            // - Application Reply Size (1 byte)
            // - Reserved (1 byte)
            // - Application Reply data

            if (cipReply.Data.Length < 16)
            {
                throw new DataException("Forward Open reply too short");
            }

            // Get the T->O connection ID - this is what we use to send data
            var toConnectionId = (uint)(
                cipReply.Data[4] |
                (cipReply.Data[5] << 8) |
                (cipReply.Data[6] << 16) |
                (cipReply.Data[7] << 24));

            return toConnectionId;
        }

        /// <summary>
        /// Builds a Forward Close request.
        /// </summary>
        /// <param name="sessionHandle">The session handle.</param>
        /// <param name="connParams">The connection parameters from Forward Open.</param>
        /// <param name="routePath">Optional route path.</param>
        /// <returns>The Forward Close request packet.</returns>
        public static byte[] BuildCloseRequest(uint sessionHandle, ConnectionParams connParams, byte[]? routePath = null)
        {
            using var cipMs = new MemoryStream();

            // Service: Forward Close (0x4E)
            cipMs.WriteByte(CipServices.ForwardClose);

            // Path to Connection Manager
            var cmPath = new byte[] { 0x20, 0x06, 0x24, 0x01 };
            cipMs.WriteByte((byte)(cmPath.Length / 2));
            cipMs.Write(cmPath, 0, cmPath.Length);

            // Priority/Time_tick
            cipMs.WriteByte(0x0A);

            // Timeout_ticks
            cipMs.WriteByte(0x0E);

            // Connection Serial Number
            cipMs.WriteByte((byte)(connParams.ConnectionSerialNumber & 0xFF));
            cipMs.WriteByte((byte)((connParams.ConnectionSerialNumber >> 8) & 0xFF));

            // Originator Vendor ID
            cipMs.WriteByte((byte)(connParams.VendorId & 0xFF));
            cipMs.WriteByte((byte)((connParams.VendorId >> 8) & 0xFF));

            // Originator Serial Number
            cipMs.WriteByte((byte)(connParams.OriginatorSerialNumber & 0xFF));
            cipMs.WriteByte((byte)((connParams.OriginatorSerialNumber >> 8) & 0xFF));
            cipMs.WriteByte((byte)((connParams.OriginatorSerialNumber >> 16) & 0xFF));
            cipMs.WriteByte((byte)((connParams.OriginatorSerialNumber >> 24) & 0xFF));

            // Connection Path Size
            var connPath = routePath ?? new byte[] { 0x20, 0x67, 0x24, 0x01 };
            cipMs.WriteByte((byte)(connPath.Length / 2));

            // Reserved
            cipMs.WriteByte(0x00);

            // Connection Path
            cipMs.Write(connPath, 0, connPath.Length);

            var cipRequest = cipMs.ToArray();

            return SendRRDataPacket.BuildRequest(sessionHandle, cipRequest);
        }
    }
}
