// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Text;
using CSComm3.SLC.Exceptions;

namespace CSComm3.SLC.Packets
{
    /// <summary>
    /// Builds ListIdentity request packets for device discovery.
    /// </summary>
    public static class ListIdentityPacket
    {
        /// <summary>
        /// Builds a ListIdentity request packet.
        /// </summary>
        /// <returns>The request packet bytes.</returns>
        public static byte[] BuildRequest()
        {
            var packet = new RequestPacket
            {
                Command = EncapsulationCommands.ListIdentity,
                SessionHandle = 0
            };

            // No data payload for ListIdentity
            return packet.Build();
        }

        /// <summary>
        /// Parses a ListIdentity response and extracts device information.
        /// </summary>
        /// <param name="responseData">The raw response data.</param>
        /// <returns>The device identity information.</returns>
        public static DeviceIdentity ParseResponse(byte[] responseData)
        {
            var response = new ResponsePacket(responseData);
            response.ThrowIfError("ListIdentity failed");

            if (response.Data.Length == 0)
            {
                throw new ResponseException("ListIdentity response contains no data");
            }

            var identity = new DeviceIdentity();

            // Item Count (2 bytes)
            var itemCount = response.ReadUInt16();

            if (itemCount < 1)
            {
                throw new ResponseException("ListIdentity response contains no items");
            }

            // CPF Item: Identity Item (0x000C)
            var itemType = response.ReadUInt16();
            var itemLength = response.ReadUInt16();

            // Protocol Version (2 bytes)
            identity.ProtocolVersion = response.ReadUInt16();

            // Socket Address (16 bytes)
            // sin_family (2 bytes - big endian)
            response.Skip(2);
            // sin_port (2 bytes - big endian)
            var portHi = response.ReadByte();
            var portLo = response.ReadByte();
            identity.Port = (ushort)((portHi << 8) | portLo);
            // sin_addr (4 bytes - network byte order)
            var ip1 = response.ReadByte();
            var ip2 = response.ReadByte();
            var ip3 = response.ReadByte();
            var ip4 = response.ReadByte();
            identity.IpAddress = $"{ip1}.{ip2}.{ip3}.{ip4}";
            // sin_zero (8 bytes)
            response.Skip(8);

            // Vendor ID (2 bytes)
            identity.VendorId = response.ReadUInt16();

            // Device Type (2 bytes)
            identity.DeviceType = response.ReadUInt16();

            // Product Code (2 bytes)
            identity.ProductCode = response.ReadUInt16();

            // Revision (2 bytes: major.minor)
            identity.RevisionMajor = response.ReadByte();
            identity.RevisionMinor = response.ReadByte();

            // Status (2 bytes)
            identity.Status = response.ReadUInt16();

            // Serial Number (4 bytes)
            identity.SerialNumber = response.ReadUInt32();

            // Product Name Length (1 byte)
            var nameLength = response.ReadByte();

            // Product Name (variable)
            if (nameLength > 0 && response.RemainingBytes >= nameLength)
            {
                var nameBytes = response.ReadBytes(nameLength);
                identity.ProductName = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
            }

            // State (1 byte) if remaining
            if (response.RemainingBytes >= 1)
            {
                identity.State = response.ReadByte();
            }

            return identity;
        }
    }

    /// <summary>
    /// Represents identity information for an EtherNet/IP device.
    /// </summary>
    public class DeviceIdentity
    {
        /// <summary>
        /// Gets or sets the protocol version.
        /// </summary>
        public ushort ProtocolVersion { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port number.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Gets or sets the vendor ID.
        /// </summary>
        public ushort VendorId { get; set; }

        /// <summary>
        /// Gets or sets the device type.
        /// </summary>
        public ushort DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the product code.
        /// </summary>
        public ushort ProductCode { get; set; }

        /// <summary>
        /// Gets or sets the major revision number.
        /// </summary>
        public byte RevisionMajor { get; set; }

        /// <summary>
        /// Gets or sets the minor revision number.
        /// </summary>
        public byte RevisionMinor { get; set; }

        /// <summary>
        /// Gets the revision string (major.minor).
        /// </summary>
        public string Revision => $"{RevisionMajor}.{RevisionMinor}";

        /// <summary>
        /// Gets or sets the device status.
        /// </summary>
        public ushort Status { get; set; }

        /// <summary>
        /// Gets or sets the serial number.
        /// </summary>
        public uint SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device state.
        /// </summary>
        public byte State { get; set; }

        /// <summary>
        /// Gets the vendor name based on vendor ID.
        /// </summary>
        public string VendorName => VendorId switch
        {
            1 => "Rockwell Automation/Allen-Bradley",
            _ => $"Unknown (0x{VendorId:X4})"
        };
    }
}
