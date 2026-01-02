using CSComm3.SLC;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.PCCC;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.PCCC
{
    public class PcccProtocolTests
    {
        [Fact]
        public void BuildTypedRead_CreatesCorrectPacket()
        {
            var packet = PcccProtocol.BuildTypedRead(
                fileType: FileTypeCodes.Integer,
                fileNumber: 7,
                elementNumber: 0,
                subElement: 0,
                readLength: 2);

            // Command (0x0F), STS (0), Transaction (2), Function (0xA2),
            // ReadLen, FileNum, FileType, Element, SubElement
            packet.Should().HaveCount(10);
            packet[0].Should().Be(PcccCommands.Command);
            packet[1].Should().Be(0x00); // STS
            // Transaction ID at [2] and [3]
            packet[4].Should().Be(PcccCommands.ProtectedTypedLogicalRead3Address);
            packet[5].Should().Be(2); // Read length
            packet[6].Should().Be(7); // File number
            packet[7].Should().Be(FileTypeCodes.Integer); // File type
            packet[8].Should().Be(0); // Element
            packet[9].Should().Be(0); // Sub-element
        }

        [Fact]
        public void BuildTypedWrite_CreatesCorrectPacket()
        {
            var data = new byte[] { 0x64, 0x00 }; // Value 100

            var packet = PcccProtocol.BuildTypedWrite(
                fileType: FileTypeCodes.Integer,
                fileNumber: 7,
                elementNumber: 5,
                subElement: 0,
                data: data);

            // Command, STS, Transaction (2), Function, WriteLen, FileNum, FileType, Element, SubElement, Data
            packet.Should().HaveCount(12);
            packet[0].Should().Be(PcccCommands.Command);
            packet[4].Should().Be(PcccCommands.ProtectedTypedLogicalWrite3Address);
            packet[5].Should().Be(2); // Write length
            packet[6].Should().Be(7); // File number
            packet[7].Should().Be(FileTypeCodes.Integer);
            packet[8].Should().Be(5); // Element
            packet[9].Should().Be(0); // Sub-element
            packet[10].Should().Be(0x64); // Data LSB
            packet[11].Should().Be(0x00); // Data MSB
        }

        [Fact]
        public void BuildMaskedWrite_CreatesCorrectPacket()
        {
            // Set bit 0, clear bit 1: OR mask = 0x0001, AND mask = 0xFFFD
            var packet = PcccProtocol.BuildMaskedWrite(
                fileType: FileTypeCodes.Bit,
                fileNumber: 3,
                elementNumber: 0,
                subElement: 0,
                orMask: 0x0001,
                andMask: 0xFFFD);

            packet.Should().HaveCount(14);
            packet[0].Should().Be(PcccCommands.Command);
            packet[4].Should().Be(PcccCommands.ProtectedTypedLogicalMaskedWrite);
            packet[5].Should().Be(4); // Write length (OR + AND masks)
            packet[6].Should().Be(3); // File number
            packet[7].Should().Be(FileTypeCodes.Bit);
            // OR mask (little-endian)
            packet[10].Should().Be(0x01);
            packet[11].Should().Be(0x00);
            // AND mask (little-endian)
            packet[12].Should().Be(0xFD);
            packet[13].Should().Be(0xFF);
        }

        [Fact]
        public void BuildExecutePcccRequest_CreatesCipWrapper()
        {
            var pcccData = new byte[] { 0x0F, 0x00, 0x01, 0x00, 0xA2, 0x02, 0x07, 0x89, 0x00, 0x00 };

            var request = PcccProtocol.BuildExecutePcccRequest(pcccData);

            // Service (0x4B), Path size, Path, Requestor ID (7), PCCC data
            request[0].Should().Be(CipServices.ExecutePCCC);
            request[1].Should().Be(2); // Path size in words (4 bytes / 2)
            // Path: Class 8-bit, 0x67, Instance 8-bit, 0x01
            request[2].Should().Be(PathSegments.Class8Bit);
            request[3].Should().Be(PathSegments.PcccClass);
            request[4].Should().Be(PathSegments.Instance8Bit);
            request[5].Should().Be(0x01);
            // Requestor ID length
            request[6].Should().Be(0x07);
        }

        [Fact]
        public void ParseReply_SuccessfulReply_ReturnsData()
        {
            // Reply includes Requestor ID prefix (7 bytes) + PCCC reply
            // Requestor ID: 07-00-00-00-00-00-00
            // PCCC: Command, STS=0, Transaction (2), Data...
            var reply = new byte[] {
                0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // Requestor ID (7 bytes)
                0x4F, 0x00, 0x01, 0x00, 0x64, 0x00         // PCCC reply
            };

            var result = PcccProtocol.ParseReply(reply);

            result.IsSuccess.Should().BeTrue();
            result.Command.Should().Be(0x4F);
            result.Status.Should().Be(0x00);
            result.TransactionId.Should().Be(0x0001);
            result.Data.Should().BeEquivalentTo(new byte[] { 0x64, 0x00 });
        }

        [Fact]
        public void ParseReply_ErrorReply_SetsStatus()
        {
            // Reply includes Requestor ID prefix (7 bytes) + PCCC reply
            // PCCC: Command, STS=0x10 (Illegal command), Transaction (2), ExtSts
            var reply = new byte[] {
                0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // Requestor ID (7 bytes)
                0x4F, 0x10, 0x01, 0x00, 0x00               // PCCC reply with error
            };

            var result = PcccProtocol.ParseReply(reply);

            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(0x10);
            result.ExtendedStatus.Should().Be(0x00);
        }

        [Fact]
        public void ParseReply_TooShort_ThrowsDataException()
        {
            var reply = new byte[] { 0x4F, 0x00, 0x01 }; // Only 3 bytes

            var act = () => PcccProtocol.ParseReply(reply);

            act.Should().Throw<DataException>()
                .WithMessage("*too short*");
        }

        [Fact]
        public void PcccReply_ThrowIfError_ThrowsOnError()
        {
            var reply = new PcccReply { Status = 0x10 };

            var act = () => reply.ThrowIfError("Test error");

            act.Should().Throw<ResponseException>()
                .WithMessage("*Test error*Illegal command*");
        }

        [Fact]
        public void PcccReply_ThrowIfError_DoesNotThrowOnSuccess()
        {
            var reply = new PcccReply { Status = 0x00 };

            var act = () => reply.ThrowIfError();

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData((byte)0x00, "Success")]
        [InlineData((byte)0x10, "Illegal command")]
        [InlineData((byte)0x40, "Host could not complete")]
        [InlineData((byte)0x70, "Program mode")]
        [InlineData((byte)0xFF, "Unknown")]
        public void GetStatusDescription_ReturnsCorrectDescription(byte status, string expectedContains)
        {
            var description = PcccReply.GetStatusDescription(status, null);

            description.Should().Contain(expectedContains);
        }

        [Fact]
        public void GetNextTransactionId_Increments()
        {
            var id1 = PcccProtocol.GetNextTransactionId();
            var id2 = PcccProtocol.GetNextTransactionId();

            (id2 - id1).Should().Be(1);
        }
    }
}
