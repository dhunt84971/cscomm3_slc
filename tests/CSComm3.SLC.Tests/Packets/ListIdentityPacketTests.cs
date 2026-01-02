using CSComm3.SLC;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Packets;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.Packets
{
    public class ListIdentityPacketTests
    {
        [Fact]
        public void BuildRequest_CreatesCorrectPacket()
        {
            var packet = ListIdentityPacket.BuildRequest();

            // Should be 24 bytes: header only, no data
            packet.Should().HaveCount(24);

            // Command: ListIdentity (0x0063)
            packet[0].Should().Be(0x63);
            packet[1].Should().Be(0x00);

            // Length should be 0
            packet[2].Should().Be(0x00);
            packet[3].Should().Be(0x00);

            // Session Handle should be 0
            packet[4].Should().Be(0x00);
            packet[5].Should().Be(0x00);
            packet[6].Should().Be(0x00);
            packet[7].Should().Be(0x00);
        }

        [Fact]
        public void ParseResponse_ExtractsDeviceIdentity()
        {
            var responseData = CreateListIdentityResponse(
                ipAddress: new byte[] { 192, 168, 1, 100 },
                port: 44818,
                vendorId: 1,
                deviceType: 14, // Programmable Logic Controller
                productCode: 55,
                revisionMajor: 5,
                revisionMinor: 2,
                serialNumber: 0x12345678,
                productName: "1747-L551");

            var identity = ListIdentityPacket.ParseResponse(responseData);

            identity.IpAddress.Should().Be("192.168.1.100");
            identity.Port.Should().Be(44818);
            identity.VendorId.Should().Be(1);
            identity.VendorName.Should().Be("Rockwell Automation/Allen-Bradley");
            identity.DeviceType.Should().Be(14);
            identity.ProductCode.Should().Be(55);
            identity.RevisionMajor.Should().Be(5);
            identity.RevisionMinor.Should().Be(2);
            identity.Revision.Should().Be("5.2");
            identity.SerialNumber.Should().Be(0x12345678);
            identity.ProductName.Should().Be("1747-L551");
        }

        [Fact]
        public void ParseResponse_WithUnknownVendor_ReturnsUnknownVendorName()
        {
            var responseData = CreateListIdentityResponse(
                ipAddress: new byte[] { 192, 168, 1, 100 },
                port: 44818,
                vendorId: 999,
                deviceType: 14,
                productCode: 55,
                revisionMajor: 1,
                revisionMinor: 0,
                serialNumber: 0x00000001,
                productName: "Test Device");

            var identity = ListIdentityPacket.ParseResponse(responseData);

            identity.VendorId.Should().Be(999);
            identity.VendorName.Should().Contain("Unknown");
        }

        [Fact]
        public void ParseResponse_WithNoData_ThrowsResponseException()
        {
            var responseData = new byte[24]; // Header only, no data
            responseData[0] = 0x63; // ListIdentity command
            responseData[1] = 0x00;

            var act = () => ListIdentityPacket.ParseResponse(responseData);

            act.Should().Throw<ResponseException>()
                .WithMessage("*no data*");
        }

        [Fact]
        public void ParseResponse_WithErrorStatus_ThrowsResponseException()
        {
            var responseData = new byte[24];
            responseData[0] = 0x63;
            responseData[1] = 0x00;
            responseData[8] = 0x01; // Error status

            var act = () => ListIdentityPacket.ParseResponse(responseData);

            act.Should().Throw<ResponseException>()
                .WithMessage("*ListIdentity failed*");
        }

        [Fact]
        public void DeviceIdentity_DefaultValues()
        {
            var identity = new DeviceIdentity();

            identity.IpAddress.Should().BeEmpty();
            identity.Port.Should().Be(0);
            identity.ProductName.Should().BeEmpty();
            identity.VendorName.Should().Contain("Unknown");
        }

        private static byte[] CreateListIdentityResponse(
            byte[] ipAddress,
            ushort port,
            ushort vendorId,
            ushort deviceType,
            ushort productCode,
            byte revisionMajor,
            byte revisionMinor,
            uint serialNumber,
            string productName)
        {
            var nameBytes = System.Text.Encoding.ASCII.GetBytes(productName);

            // Identity item length: ProtocolVersion(2) + Socket(16) + VendorId(2) + DeviceType(2) + ProductCode(2) +
            // Revision(2) + Status(2) + Serial(4) + NameLen(1) + Name + State(1)
            var identityLength = 2 + 16 + 2 + 2 + 2 + 2 + 2 + 4 + 1 + nameBytes.Length + 1;

            // Data: ItemCount(2) + ItemType(2) + ItemLength(2) + Identity
            var dataLength = 2 + 2 + 2 + identityLength;

            var result = new byte[24 + dataLength];

            // Command: ListIdentity (0x0063)
            result[0] = 0x63;
            result[1] = 0x00;

            // Length
            result[2] = (byte)(dataLength & 0xFF);
            result[3] = (byte)((dataLength >> 8) & 0xFF);

            // Status: 0 (success)
            // (rest of header is zeros)

            var offset = 24;

            // Item Count = 1
            result[offset++] = 0x01;
            result[offset++] = 0x00;

            // Item Type: Identity (0x000C)
            result[offset++] = 0x0C;
            result[offset++] = 0x00;

            // Item Length
            result[offset++] = (byte)(identityLength & 0xFF);
            result[offset++] = (byte)((identityLength >> 8) & 0xFF);

            // Protocol Version (0x0001)
            result[offset++] = 0x01;
            result[offset++] = 0x00;

            // Socket Address (16 bytes)
            // sin_family (2 bytes - AF_INET = 2, big endian)
            result[offset++] = 0x00;
            result[offset++] = 0x02;
            // sin_port (big endian)
            result[offset++] = (byte)((port >> 8) & 0xFF);
            result[offset++] = (byte)(port & 0xFF);
            // sin_addr (network byte order)
            result[offset++] = ipAddress[0];
            result[offset++] = ipAddress[1];
            result[offset++] = ipAddress[2];
            result[offset++] = ipAddress[3];
            // sin_zero (8 bytes)
            offset += 8;

            // Vendor ID
            result[offset++] = (byte)(vendorId & 0xFF);
            result[offset++] = (byte)((vendorId >> 8) & 0xFF);

            // Device Type
            result[offset++] = (byte)(deviceType & 0xFF);
            result[offset++] = (byte)((deviceType >> 8) & 0xFF);

            // Product Code
            result[offset++] = (byte)(productCode & 0xFF);
            result[offset++] = (byte)((productCode >> 8) & 0xFF);

            // Revision
            result[offset++] = revisionMajor;
            result[offset++] = revisionMinor;

            // Status
            result[offset++] = 0x00;
            result[offset++] = 0x00;

            // Serial Number
            result[offset++] = (byte)(serialNumber & 0xFF);
            result[offset++] = (byte)((serialNumber >> 8) & 0xFF);
            result[offset++] = (byte)((serialNumber >> 16) & 0xFF);
            result[offset++] = (byte)((serialNumber >> 24) & 0xFF);

            // Product Name Length
            result[offset++] = (byte)nameBytes.Length;

            // Product Name
            Array.Copy(nameBytes, 0, result, offset, nameBytes.Length);
            offset += nameBytes.Length;

            // State
            result[offset++] = 0x00;

            return result;
        }
    }
}
