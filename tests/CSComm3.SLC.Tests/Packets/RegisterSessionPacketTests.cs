using CSComm3.SLC;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Packets;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.Packets
{
    public class RegisterSessionPacketTests
    {
        [Fact]
        public void BuildRequest_CreatesCorrectPacket()
        {
            var packet = RegisterSessionPacket.BuildRequest();

            // Should be 28 bytes: 24 header + 4 data (protocol version + options)
            packet.Should().HaveCount(28);

            // Command should be RegisterSession (0x0065)
            packet[0].Should().Be(0x65);
            packet[1].Should().Be(0x00);

            // Length should be 4
            packet[2].Should().Be(0x04);
            packet[3].Should().Be(0x00);

            // Session Handle should be 0 (not yet established)
            packet[4].Should().Be(0x00);
            packet[5].Should().Be(0x00);
            packet[6].Should().Be(0x00);
            packet[7].Should().Be(0x00);

            // Protocol Version (0x0001)
            packet[24].Should().Be(0x01);
            packet[25].Should().Be(0x00);

            // Options (0x0000)
            packet[26].Should().Be(0x00);
            packet[27].Should().Be(0x00);
        }

        [Fact]
        public void ParseResponse_ExtractsSessionHandle()
        {
            var responseData = CreateSuccessResponse(0x12345678);

            var sessionHandle = RegisterSessionPacket.ParseResponse(responseData);

            sessionHandle.Should().Be(0x12345678);
        }

        [Fact]
        public void ParseResponse_WithErrorStatus_ThrowsResponseException()
        {
            var responseData = CreateErrorResponse(0x01);

            var act = () => RegisterSessionPacket.ParseResponse(responseData);

            act.Should().Throw<ResponseException>()
                .WithMessage("*RegisterSession failed*");
        }

        [Fact]
        public void UnregisterSession_BuildRequest_CreatesCorrectPacket()
        {
            var packet = UnregisterSessionPacket.BuildRequest(0x12345678);

            // Should be 24 bytes: header only, no data
            packet.Should().HaveCount(24);

            // Command should be UnregisterSession (0x0066)
            packet[0].Should().Be(0x66);
            packet[1].Should().Be(0x00);

            // Length should be 0
            packet[2].Should().Be(0x00);
            packet[3].Should().Be(0x00);

            // Session Handle should be the provided value
            packet[4].Should().Be(0x78);
            packet[5].Should().Be(0x56);
            packet[6].Should().Be(0x34);
            packet[7].Should().Be(0x12);
        }

        private static byte[] CreateSuccessResponse(uint sessionHandle)
        {
            var result = new byte[28]; // Header + 4 bytes data

            // Command: RegisterSession response (0x0065)
            result[0] = 0x65;
            result[1] = 0x00;

            // Length: 4
            result[2] = 0x04;
            result[3] = 0x00;

            // Session Handle
            result[4] = (byte)(sessionHandle & 0xFF);
            result[5] = (byte)((sessionHandle >> 8) & 0xFF);
            result[6] = (byte)((sessionHandle >> 16) & 0xFF);
            result[7] = (byte)((sessionHandle >> 24) & 0xFF);

            // Status: 0 (success)
            // Sender Context: 0s
            // Options: 0s

            // Data: Protocol Version + Options (echoed back)
            result[24] = 0x01;
            result[25] = 0x00;
            result[26] = 0x00;
            result[27] = 0x00;

            return result;
        }

        private static byte[] CreateErrorResponse(uint status)
        {
            var result = new byte[24]; // Header only

            // Command: RegisterSession response (0x0065)
            result[0] = 0x65;
            result[1] = 0x00;

            // Length: 0
            result[2] = 0x00;
            result[3] = 0x00;

            // Session Handle: 0
            // Status (error)
            result[8] = (byte)(status & 0xFF);
            result[9] = (byte)((status >> 8) & 0xFF);
            result[10] = (byte)((status >> 16) & 0xFF);
            result[11] = (byte)((status >> 24) & 0xFF);

            return result;
        }
    }
}
