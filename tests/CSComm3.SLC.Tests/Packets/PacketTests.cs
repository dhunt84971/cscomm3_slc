using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Packets;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.Packets
{
    public class PacketTests
    {
        [Fact]
        public void RequestPacket_Build_CreatesCorrectHeader()
        {
            var packet = new RequestPacket
            {
                Command = new byte[] { 0x65, 0x00 },
                SessionHandle = 0x12345678
            };

            var result = packet.Build();

            // Should be exactly 24 bytes (header only, no data)
            result.Should().HaveCount(Constants.HeaderSize);

            // Command
            result[0].Should().Be(0x65);
            result[1].Should().Be(0x00);

            // Length (0 - no data)
            result[2].Should().Be(0x00);
            result[3].Should().Be(0x00);

            // Session Handle (little-endian)
            result[4].Should().Be(0x78);
            result[5].Should().Be(0x56);
            result[6].Should().Be(0x34);
            result[7].Should().Be(0x12);

            // Status (0)
            result[8].Should().Be(0x00);
            result[9].Should().Be(0x00);
            result[10].Should().Be(0x00);
            result[11].Should().Be(0x00);
        }

        [Fact]
        public void RequestPacket_AddByte_AddsDataCorrectly()
        {
            var packet = new RequestPacket
            {
                Command = new byte[] { 0x65, 0x00 }
            };

            packet.Add(0x01).Add(0x02);

            var result = packet.Build();

            result.Should().HaveCount(Constants.HeaderSize + 2);

            // Length should be 2
            result[2].Should().Be(0x02);
            result[3].Should().Be(0x00);

            // Data
            result[24].Should().Be(0x01);
            result[25].Should().Be(0x02);
        }

        [Fact]
        public void RequestPacket_AddBytes_AddsDataCorrectly()
        {
            var packet = new RequestPacket
            {
                Command = new byte[] { 0x65, 0x00 }
            };

            packet.Add(new byte[] { 0x01, 0x02, 0x03 });

            var result = packet.Build();

            result.Should().HaveCount(Constants.HeaderSize + 3);
            result[24].Should().Be(0x01);
            result[25].Should().Be(0x02);
            result[26].Should().Be(0x03);
        }

        [Fact]
        public void RequestPacket_AddUInt16_AddsLittleEndian()
        {
            var packet = new RequestPacket
            {
                Command = new byte[] { 0x65, 0x00 }
            };

            packet.AddUInt16(0x1234);

            var result = packet.Build();

            result[24].Should().Be(0x34);
            result[25].Should().Be(0x12);
        }

        [Fact]
        public void RequestPacket_AddUInt32_AddsLittleEndian()
        {
            var packet = new RequestPacket
            {
                Command = new byte[] { 0x65, 0x00 }
            };

            packet.AddUInt32(0x12345678);

            var result = packet.Build();

            result[24].Should().Be(0x78);
            result[25].Should().Be(0x56);
            result[26].Should().Be(0x34);
            result[27].Should().Be(0x12);
        }

        [Fact]
        public void RequestPacket_SenderContext_IncludedInHeader()
        {
            var packet = new RequestPacket
            {
                Command = new byte[] { 0x65, 0x00 },
                SenderContext = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }
            };

            var result = packet.Build();

            result[12].Should().Be(0x01);
            result[13].Should().Be(0x02);
            result[14].Should().Be(0x03);
            result[15].Should().Be(0x04);
            result[16].Should().Be(0x05);
            result[17].Should().Be(0x06);
            result[18].Should().Be(0x07);
            result[19].Should().Be(0x08);
        }

        [Fact]
        public void RequestPacket_Clear_ResetsData()
        {
            var packet = new RequestPacket
            {
                Command = new byte[] { 0x65, 0x00 }
            };

            packet.Add(new byte[] { 0x01, 0x02, 0x03 });
            packet.Clear();

            var result = packet.Build();

            result.Should().HaveCount(Constants.HeaderSize);
        }

        [Fact]
        public void RequestPacket_DataLength_ReturnsCorrectValue()
        {
            var packet = new RequestPacket();

            packet.DataLength.Should().Be(0);

            packet.Add(new byte[] { 0x01, 0x02, 0x03 });

            packet.DataLength.Should().Be(3);
        }

        [Fact]
        public void ResponsePacket_ParsesHeaderCorrectly()
        {
            var rawData = new byte[]
            {
                // Command
                0x65, 0x00,
                // Length
                0x04, 0x00,
                // Session Handle
                0x78, 0x56, 0x34, 0x12,
                // Status
                0x00, 0x00, 0x00, 0x00,
                // Sender Context
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                // Options
                0x00, 0x00, 0x00, 0x00,
                // Data
                0xAA, 0xBB, 0xCC, 0xDD
            };

            var response = new ResponsePacket(rawData);

            response.Command.Should().BeEquivalentTo(new byte[] { 0x65, 0x00 });
            response.Length.Should().Be(4);
            response.SessionHandle.Should().Be(0x12345678);
            response.Status.Should().Be(0);
            response.IsSuccess.Should().BeTrue();
            response.Data.Should().BeEquivalentTo(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD });
        }

        [Fact]
        public void ResponsePacket_ReadByte_ReadsCorrectly()
        {
            var rawData = CreateValidResponse(new byte[] { 0xAA, 0xBB, 0xCC });

            var response = new ResponsePacket(rawData);

            response.ReadByte().Should().Be(0xAA);
            response.ReadByte().Should().Be(0xBB);
            response.ReadByte().Should().Be(0xCC);
        }

        [Fact]
        public void ResponsePacket_ReadBytes_ReadsCorrectly()
        {
            var rawData = CreateValidResponse(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD });

            var response = new ResponsePacket(rawData);

            var bytes = response.ReadBytes(4);

            bytes.Should().BeEquivalentTo(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD });
        }

        [Fact]
        public void ResponsePacket_ReadUInt16_ReadsLittleEndian()
        {
            var rawData = CreateValidResponse(new byte[] { 0x34, 0x12 });

            var response = new ResponsePacket(rawData);

            response.ReadUInt16().Should().Be(0x1234);
        }

        [Fact]
        public void ResponsePacket_ReadUInt32_ReadsLittleEndian()
        {
            var rawData = CreateValidResponse(new byte[] { 0x78, 0x56, 0x34, 0x12 });

            var response = new ResponsePacket(rawData);

            response.ReadUInt32().Should().Be(0x12345678);
        }

        [Fact]
        public void ResponsePacket_Skip_AdvancesPosition()
        {
            var rawData = CreateValidResponse(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD });

            var response = new ResponsePacket(rawData);

            response.Skip(2);
            response.ReadByte().Should().Be(0xCC);
        }

        [Fact]
        public void ResponsePacket_ResetPosition_ResetsToStart()
        {
            var rawData = CreateValidResponse(new byte[] { 0xAA, 0xBB, 0xCC });

            var response = new ResponsePacket(rawData);

            response.ReadByte();
            response.ReadByte();
            response.ResetPosition();

            response.ReadByte().Should().Be(0xAA);
        }

        [Fact]
        public void ResponsePacket_RemainingBytes_ReturnsCorrectCount()
        {
            var rawData = CreateValidResponse(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD });

            var response = new ResponsePacket(rawData);

            response.RemainingBytes.Should().Be(4);
            response.ReadByte();
            response.RemainingBytes.Should().Be(3);
        }

        [Fact]
        public void ResponsePacket_TooShort_ThrowsDataException()
        {
            var rawData = new byte[10]; // Less than 24 bytes

            var act = () => new ResponsePacket(rawData);

            act.Should().Throw<DataException>()
                .WithMessage("*too short*");
        }

        [Fact]
        public void ResponsePacket_ReadPastEnd_ThrowsDataException()
        {
            var rawData = CreateValidResponse(new byte[] { 0xAA });

            var response = new ResponsePacket(rawData);

            response.ReadByte();
            var act = () => response.ReadByte();

            act.Should().Throw<DataException>()
                .WithMessage("*Not enough data*");
        }

        [Fact]
        public void ResponsePacket_WithErrorStatus_IsSuccessIsFalse()
        {
            var rawData = new byte[]
            {
                0x65, 0x00, // Command
                0x00, 0x00, // Length
                0x00, 0x00, 0x00, 0x00, // Session Handle
                0x01, 0x00, 0x00, 0x00, // Status (error)
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Sender Context
                0x00, 0x00, 0x00, 0x00  // Options
            };

            var response = new ResponsePacket(rawData);

            response.IsSuccess.Should().BeFalse();
            response.Status.Should().Be(1);
        }

        [Fact]
        public void ResponsePacket_ThrowIfError_ThrowsOnError()
        {
            var rawData = new byte[]
            {
                0x65, 0x00, // Command
                0x00, 0x00, // Length
                0x00, 0x00, 0x00, 0x00, // Session Handle
                0x01, 0x00, 0x00, 0x00, // Status (error)
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Sender Context
                0x00, 0x00, 0x00, 0x00  // Options
            };

            var response = new ResponsePacket(rawData);

            var act = () => response.ThrowIfError("Test error");

            act.Should().Throw<ResponseException>()
                .WithMessage("Test error*")
                .Where(ex => ex.StatusCode == 1);
        }

        [Fact]
        public void ResponsePacket_ThrowIfError_DoesNotThrowOnSuccess()
        {
            var rawData = CreateValidResponse(Array.Empty<byte>());

            var response = new ResponsePacket(rawData);

            var act = () => response.ThrowIfError();

            act.Should().NotThrow();
        }

        private static byte[] CreateValidResponse(byte[] data)
        {
            var result = new byte[Constants.HeaderSize + data.Length];

            // Command
            result[0] = 0x65;
            result[1] = 0x00;

            // Length
            result[2] = (byte)(data.Length & 0xFF);
            result[3] = (byte)((data.Length >> 8) & 0xFF);

            // Session Handle (0)
            // Status (0 - success)
            // Sender Context (0s)
            // Options (0s)

            // Data
            if (data.Length > 0)
            {
                Array.Copy(data, 0, result, Constants.HeaderSize, data.Length);
            }

            return result;
        }
    }
}
