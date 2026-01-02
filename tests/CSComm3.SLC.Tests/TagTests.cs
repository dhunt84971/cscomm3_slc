using CSComm3.SLC;
using CSComm3.SLC.Exceptions;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests
{
    public class TagTests
    {
        [Fact]
        public void Tag_IntegerFile_ParsesCorrectly()
        {
            var tag = new Tag("N7:0");

            tag.FileType.Should().Be("N");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Integer);
            tag.FileNumber.Should().Be(7);
            tag.ElementNumber.Should().Be(0);
            tag.SubElement.Should().Be(0);
            tag.BitNumber.Should().BeNull();
            tag.ElementSize.Should().Be(2);
            tag.IsBit.Should().BeFalse();
        }

        [Fact]
        public void Tag_FloatFile_ParsesCorrectly()
        {
            var tag = new Tag("F8:5");

            tag.FileType.Should().Be("F");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Float);
            tag.FileNumber.Should().Be(8);
            tag.ElementNumber.Should().Be(5);
            tag.ElementSize.Should().Be(4);
        }

        [Fact]
        public void Tag_BitFile_WithBitNumber_ParsesCorrectly()
        {
            var tag = new Tag("B3:0/5");

            tag.FileType.Should().Be("B");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Bit);
            tag.FileNumber.Should().Be(3);
            tag.ElementNumber.Should().Be(0);
            tag.BitNumber.Should().Be(5);
            tag.IsBit.Should().BeTrue();
        }

        [Fact]
        public void Tag_Timer_WithAccumulator_ParsesCorrectly()
        {
            var tag = new Tag("T4:0.ACC");

            tag.FileType.Should().Be("T");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Timer);
            tag.FileNumber.Should().Be(4);
            tag.ElementNumber.Should().Be(0);
            tag.SubElement.Should().Be(2); // ACC = 2
            tag.ElementSize.Should().Be(6);
        }

        [Fact]
        public void Tag_Timer_WithPreset_ParsesCorrectly()
        {
            var tag = new Tag("T4:1.PRE");

            tag.SubElement.Should().Be(1); // PRE = 1
        }

        [Fact]
        public void Tag_Counter_WithAccumulator_ParsesCorrectly()
        {
            var tag = new Tag("C5:0.ACC");

            tag.FileType.Should().Be("C");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Counter);
            tag.SubElement.Should().Be(2);
        }

        [Fact]
        public void Tag_Control_ParsesCorrectly()
        {
            var tag = new Tag("R6:0.LEN");

            tag.FileType.Should().Be("R");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Control);
            tag.SubElement.Should().Be(1); // LEN = 1
        }

        [Fact]
        public void Tag_String_ParsesCorrectly()
        {
            var tag = new Tag("ST9:0");

            tag.FileType.Should().Be("ST");
            tag.FileTypeCode.Should().Be(FileTypeCodes.String);
            tag.FileNumber.Should().Be(9);
            tag.ElementSize.Should().Be(84);
        }

        [Fact]
        public void Tag_Output_ParsesCorrectly()
        {
            var tag = new Tag("O0:0");

            tag.FileType.Should().Be("O");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Output);
        }

        [Fact]
        public void Tag_Input_ParsesCorrectly()
        {
            var tag = new Tag("I1:0");

            tag.FileType.Should().Be("I");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Input);
        }

        [Fact]
        public void Tag_Status_ParsesCorrectly()
        {
            var tag = new Tag("S2:0");

            tag.FileType.Should().Be("S");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Status);
        }

        [Fact]
        public void Tag_LongInteger_ParsesCorrectly()
        {
            var tag = new Tag("L10:0");

            tag.FileType.Should().Be("L");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Long);
            tag.ElementSize.Should().Be(4);
        }

        [Fact]
        public void Tag_Ascii_ParsesCorrectly()
        {
            var tag = new Tag("A10:0");

            tag.FileType.Should().Be("A");
            tag.FileTypeCode.Should().Be(FileTypeCodes.Ascii);
        }

        [Fact]
        public void Tag_CaseInsensitive_ParsesCorrectly()
        {
            var lower = new Tag("n7:0");
            var upper = new Tag("N7:0");

            lower.FileType.Should().Be("N");
            upper.FileType.Should().Be("N");
            lower.FileTypeCode.Should().Be(upper.FileTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Tag_NullOrEmpty_ThrowsArgumentException(string? address)
        {
            var act = () => new Tag(address!);

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("N7")]
        [InlineData("7:0")]
        [InlineData("X7:0")] // Unknown file type
        [InlineData("N7:0.INVALID")] // Unknown sub-element
        [InlineData("B3:0/16")] // Bit number out of range
        public void Tag_InvalidFormat_ThrowsRequestException(string address)
        {
            var act = () => new Tag(address);

            act.Should().Throw<RequestException>();
        }

        [Fact]
        public void Tag_ToString_ReturnsAddress()
        {
            var tag = new Tag("N7:0");

            tag.ToString().Should().Be("N7:0");
        }

        [Fact]
        public void Tag_Value_CanBeSet()
        {
            var tag = new Tag("N7:0");
            tag.Value = 100;

            tag.Value.Should().Be(100);
        }

        [Fact]
        public void TagParser_TryParse_ValidAddress_ReturnsTrue()
        {
            var result = TagParser.TryParse("N7:0", out var parsed);

            result.Should().BeTrue();
            parsed.Should().NotBeNull();
            parsed!.FileType.Should().Be("N");
        }

        [Fact]
        public void TagParser_TryParse_InvalidAddress_ReturnsFalse()
        {
            var result = TagParser.TryParse("invalid", out var parsed);

            result.Should().BeFalse();
            parsed.Should().BeNull();
        }
    }
}
