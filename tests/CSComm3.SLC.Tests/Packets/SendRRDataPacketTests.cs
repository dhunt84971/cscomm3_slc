using CSComm3.SLC;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Packets;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.Packets
{
    public class SendRRDataPacketTests
    {
        [Fact]
        public void BuildRequest_CreatesCorrectPacket()
        {
            var cipData = new byte[] { 0x4B, 0x02, 0x20, 0x67, 0x24, 0x01 }; // Execute PCCC
            var packet = SendRRDataPacket.BuildRequest(0x12345678, cipData);

            // Header (24) + Interface(4) + Timeout(2) + ItemCount(2) +
            // Item1Type(2) + Item1Len(2) + Item2Type(2) + Item2Len(2) + CipData(6)
            // = 24 + 16 + 6 = 46
            packet.Should().HaveCount(46);

            // Command: SendRRData (0x006F)
            packet[0].Should().Be(0x6F);
            packet[1].Should().Be(0x00);

            // Session Handle
            packet[4].Should().Be(0x78);
            packet[5].Should().Be(0x56);
            packet[6].Should().Be(0x34);
            packet[7].Should().Be(0x12);

            // Interface Handle (0x00000000)
            packet[24].Should().Be(0x00);
            packet[25].Should().Be(0x00);
            packet[26].Should().Be(0x00);
            packet[27].Should().Be(0x00);

            // Timeout (0x0000)
            packet[28].Should().Be(0x00);
            packet[29].Should().Be(0x00);

            // Item Count (2)
            packet[30].Should().Be(0x02);
            packet[31].Should().Be(0x00);

            // Item 1: Null Address (0x0000)
            packet[32].Should().Be(0x00);
            packet[33].Should().Be(0x00);
            // Item 1 Length (0)
            packet[34].Should().Be(0x00);
            packet[35].Should().Be(0x00);

            // Item 2: Unconnected Data (0x00B2)
            packet[36].Should().Be(0xB2);
            packet[37].Should().Be(0x00);
            // Item 2 Length (6)
            packet[38].Should().Be(0x06);
            packet[39].Should().Be(0x00);

            // CIP Data
            packet[40].Should().Be(0x4B);
            packet[41].Should().Be(0x02);
            packet[42].Should().Be(0x20);
            packet[43].Should().Be(0x67);
            packet[44].Should().Be(0x24);
            packet[45].Should().Be(0x01);
        }

        [Fact]
        public void ParseResponse_ExtractsCipData()
        {
            var responseData = CreateSendRRDataResponse(new byte[] { 0xCB, 0x00, 0x00, 0x00, 0xAA, 0xBB });

            var cipData = SendRRDataPacket.ParseResponse(responseData);

            cipData.Should().BeEquivalentTo(new byte[] { 0xCB, 0x00, 0x00, 0x00, 0xAA, 0xBB });
        }

        [Fact]
        public void ParseResponse_WithEncapsulationError_ThrowsResponseException()
        {
            var responseData = CreateErrorResponse(0x01);

            var act = () => SendRRDataPacket.ParseResponse(responseData);

            act.Should().Throw<ResponseException>()
                .WithMessage("*SendRRData failed*");
        }

        [Fact]
        public void ParseCipReply_ParsesSuccessfulReply()
        {
            // CIP reply: service(0xCB = 0x4B | 0x80), reserved, status(0), extStatusSize(0), data(AA BB)
            var cipData = new byte[] { 0xCB, 0x00, 0x00, 0x00, 0xAA, 0xBB };
            var responseData = CreateSendRRDataResponse(cipData);

            var reply = SendRRDataPacket.ParseCipReply(responseData, CipServices.ExecutePCCC);

            reply.IsSuccess.Should().BeTrue();
            reply.Service.Should().Be(0xCB);
            reply.Status.Should().Be(0);
            reply.Data.Should().BeEquivalentTo(new byte[] { 0xAA, 0xBB });
        }

        [Fact]
        public void ParseCipReply_WithExtendedStatus_ParsesCorrectly()
        {
            // CIP reply with extended status: service, reserved, status(1), extStatusSize(1), extStatus(0x2001), data
            var cipData = new byte[] { 0xCB, 0x00, 0x01, 0x01, 0x01, 0x20, 0xAA };
            var responseData = CreateSendRRDataResponse(cipData);

            var act = () => SendRRDataPacket.ParseCipReply(responseData, CipServices.ExecutePCCC);

            act.Should().Throw<ResponseException>()
                .Where(ex => ex.StatusCode == 0x01)
                .Where(ex => ex.ExtendedStatus == 0x2001);
        }

        [Fact]
        public void ParseCipReply_WrongService_ThrowsResponseException()
        {
            // Wrong service code
            var cipData = new byte[] { 0xCC, 0x00, 0x00, 0x00 }; // 0xCC instead of 0xCB
            var responseData = CreateSendRRDataResponse(cipData);

            var act = () => SendRRDataPacket.ParseCipReply(responseData, CipServices.ExecutePCCC);

            act.Should().Throw<ResponseException>()
                .WithMessage("*Unexpected service reply*");
        }

        [Fact]
        public void ParseCipReply_CipError_ThrowsResponseException()
        {
            // CIP error status
            var cipData = new byte[] { 0xCB, 0x00, 0x10, 0x00 }; // status 0x10
            var responseData = CreateSendRRDataResponse(cipData);

            var act = () => SendRRDataPacket.ParseCipReply(responseData, CipServices.ExecutePCCC);

            act.Should().Throw<ResponseException>()
                .WithMessage("*CIP error*")
                .Where(ex => ex.StatusCode == 0x10);
        }

        [Fact]
        public void CipReply_IsSuccess_ReturnsTrueForZeroStatus()
        {
            var reply = new CipReply { Status = 0 };
            reply.IsSuccess.Should().BeTrue();

            reply.Status = 1;
            reply.IsSuccess.Should().BeFalse();
        }

        private static byte[] CreateSendRRDataResponse(byte[] cipData)
        {
            // Header(24) + Interface(4) + Timeout(2) + ItemCount(2) +
            // Item1Type(2) + Item1Len(2) + Item2Type(2) + Item2Len(2) + CipData
            var dataLength = 16 + cipData.Length;
            var result = new byte[24 + dataLength];

            // Command: SendRRData (0x006F)
            result[0] = 0x6F;
            result[1] = 0x00;

            // Length
            result[2] = (byte)(dataLength & 0xFF);
            result[3] = (byte)((dataLength >> 8) & 0xFF);

            // Session Handle, Status, etc. (zeros are fine)

            // Interface Handle (4 bytes at offset 24)
            // Timeout (2 bytes at offset 28)
            // Item Count = 2 (at offset 30)
            result[30] = 0x02;
            result[31] = 0x00;

            // Item 1: Null Address (0x0000, length 0)
            // offset 32-35

            // Item 2: Unconnected Data (0x00B2)
            result[36] = 0xB2;
            result[37] = 0x00;
            // Item 2 Length
            result[38] = (byte)(cipData.Length & 0xFF);
            result[39] = (byte)((cipData.Length >> 8) & 0xFF);

            // CIP Data
            Array.Copy(cipData, 0, result, 40, cipData.Length);

            return result;
        }

        private static byte[] CreateErrorResponse(uint status)
        {
            var result = new byte[24];

            // Command: SendRRData (0x006F)
            result[0] = 0x6F;
            result[1] = 0x00;

            // Status (error)
            result[8] = (byte)(status & 0xFF);
            result[9] = (byte)((status >> 8) & 0xFF);
            result[10] = (byte)((status >> 16) & 0xFF);
            result[11] = (byte)((status >> 24) & 0xFF);

            return result;
        }
    }
}
