using CSComm3.SLC;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Packets;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.Packets
{
    public class SendUnitDataPacketTests
    {
        [Fact]
        public void BuildRequest_CreatesCorrectPacket()
        {
            var cipData = new byte[] { 0x4B, 0x02, 0x20, 0x67, 0x24, 0x01 };
            var packet = SendUnitDataPacket.BuildRequest(
                sessionHandle: 0x12345678,
                connectionId: 0xAABBCCDD,
                sequenceNumber: 0x0005,
                cipData: cipData);

            // Header(24) + Interface(4) + Timeout(2) + ItemCount(2) +
            // Item1Type(2) + Item1Len(2) + ConnectionId(4) +
            // Item2Type(2) + Item2Len(2) + SeqNum(2) + CipData(6)
            // = 24 + 22 + 6 = 52
            packet.Should().HaveCount(52);

            // Command: SendUnitData (0x0070)
            packet[0].Should().Be(0x70);
            packet[1].Should().Be(0x00);

            // Session Handle
            packet[4].Should().Be(0x78);
            packet[5].Should().Be(0x56);
            packet[6].Should().Be(0x34);
            packet[7].Should().Be(0x12);

            // Item Count (2)
            packet[30].Should().Be(0x02);
            packet[31].Should().Be(0x00);

            // Item 1: Connected Address (0x00A1)
            packet[32].Should().Be(0xA1);
            packet[33].Should().Be(0x00);
            // Item 1 Length (4)
            packet[34].Should().Be(0x04);
            packet[35].Should().Be(0x00);
            // Connection ID (little-endian)
            packet[36].Should().Be(0xDD);
            packet[37].Should().Be(0xCC);
            packet[38].Should().Be(0xBB);
            packet[39].Should().Be(0xAA);

            // Item 2: Connected Data (0x00B1)
            packet[40].Should().Be(0xB1);
            packet[41].Should().Be(0x00);
            // Item 2 Length (8 = 2 seq + 6 data)
            packet[42].Should().Be(0x08);
            packet[43].Should().Be(0x00);
            // Sequence Number (little-endian)
            packet[44].Should().Be(0x05);
            packet[45].Should().Be(0x00);
            // CIP Data
            packet[46].Should().Be(0x4B);
        }

        [Fact]
        public void ParseResponse_ExtractsCipData()
        {
            var cipData = new byte[] { 0xCB, 0x00, 0x00, 0x00, 0xAA, 0xBB };
            var responseData = CreateSendUnitDataResponse(0xAABBCCDD, 0x0005, cipData);

            var result = SendUnitDataPacket.ParseResponse(responseData);

            result.ConnectionId.Should().Be(0xAABBCCDD);
            result.SequenceNumber.Should().Be(0x0005);
            result.CipData.Should().BeEquivalentTo(cipData);
        }

        [Fact]
        public void ParseResponse_WithEncapsulationError_ThrowsResponseException()
        {
            var responseData = CreateErrorResponse(0x01);

            var act = () => SendUnitDataPacket.ParseResponse(responseData);

            act.Should().Throw<ResponseException>()
                .WithMessage("*SendUnitData failed*");
        }

        [Fact]
        public void ParseCipReply_ParsesSuccessfulReply()
        {
            var cipData = new byte[] { 0xCB, 0x00, 0x00, 0x00, 0xAA, 0xBB };
            var responseData = CreateSendUnitDataResponse(0xAABBCCDD, 0x0005, cipData);

            var reply = SendUnitDataPacket.ParseCipReply(responseData, CipServices.ExecutePCCC);

            reply.IsSuccess.Should().BeTrue();
            reply.Service.Should().Be(0xCB);
            reply.Status.Should().Be(0);
            reply.Data.Should().BeEquivalentTo(new byte[] { 0xAA, 0xBB });
        }

        [Fact]
        public void ParseCipReply_WithCipError_ThrowsResponseException()
        {
            var cipData = new byte[] { 0xCB, 0x00, 0x10, 0x00 }; // status 0x10
            var responseData = CreateSendUnitDataResponse(0xAABBCCDD, 0x0005, cipData);

            var act = () => SendUnitDataPacket.ParseCipReply(responseData, CipServices.ExecutePCCC);

            act.Should().Throw<ResponseException>()
                .WithMessage("*CIP error*")
                .Where(ex => ex.StatusCode == 0x10);
        }

        [Fact]
        public void SendUnitDataResponse_DefaultValues()
        {
            var response = new SendUnitDataResponse();

            response.ConnectionId.Should().Be(0);
            response.SequenceNumber.Should().Be(0);
            response.CipData.Should().BeEmpty();
        }

        private static byte[] CreateSendUnitDataResponse(uint connectionId, ushort sequenceNumber, byte[] cipData)
        {
            // Header(24) + Interface(4) + Timeout(2) + ItemCount(2) +
            // Item1Type(2) + Item1Len(2) + ConnectionId(4) +
            // Item2Type(2) + Item2Len(2) + SeqNum(2) + CipData
            // = 24 + 8 + 8 + 6 + cipData.Length = 46 + cipData.Length
            var dataLength = 8 + 8 + 6 + cipData.Length; // Interface(4)+Timeout(2)+ItemCount(2) + Item1(8) + Item2Header(4)+SeqNum(2) + CipData
            var result = new byte[24 + dataLength];

            // Command: SendUnitData (0x0070)
            result[0] = 0x70;
            result[1] = 0x00;

            // Length
            result[2] = (byte)(dataLength & 0xFF);
            result[3] = (byte)((dataLength >> 8) & 0xFF);

            // Item Count = 2 (at offset 30)
            result[30] = 0x02;
            result[31] = 0x00;

            // Item 1: Connected Address (0x00A1)
            result[32] = 0xA1;
            result[33] = 0x00;
            // Item 1 Length (4)
            result[34] = 0x04;
            result[35] = 0x00;
            // Connection ID
            result[36] = (byte)(connectionId & 0xFF);
            result[37] = (byte)((connectionId >> 8) & 0xFF);
            result[38] = (byte)((connectionId >> 16) & 0xFF);
            result[39] = (byte)((connectionId >> 24) & 0xFF);

            // Item 2: Connected Data (0x00B1)
            result[40] = 0xB1;
            result[41] = 0x00;
            // Item 2 Length (2 + cipData.Length)
            var item2Len = 2 + cipData.Length;
            result[42] = (byte)(item2Len & 0xFF);
            result[43] = (byte)((item2Len >> 8) & 0xFF);
            // Sequence Number
            result[44] = (byte)(sequenceNumber & 0xFF);
            result[45] = (byte)((sequenceNumber >> 8) & 0xFF);
            // CIP Data
            Array.Copy(cipData, 0, result, 46, cipData.Length);

            return result;
        }

        private static byte[] CreateErrorResponse(uint status)
        {
            var result = new byte[24];

            // Command: SendUnitData (0x0070)
            result[0] = 0x70;
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
