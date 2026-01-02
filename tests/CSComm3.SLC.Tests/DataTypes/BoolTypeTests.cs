using CSComm3.SLC.DataTypes;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.DataTypes
{
    public class BoolTypeTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BOOL_RoundTrip_PreservesValue(bool value)
        {
            var encoded = BOOL.Instance.Encode(value);
            var decoded = BOOL.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void BOOL_Properties_AreCorrect()
        {
            BOOL.Instance.TypeCode.Should().Be(0xC1);
            BOOL.Instance.Size.Should().Be(1);
        }

        [Fact]
        public void BOOL_Encode_TrueReturns0xFF()
        {
            var encoded = BOOL.Instance.Encode(true);
            encoded.Should().BeEquivalentTo(new byte[] { 0xFF });
        }

        [Fact]
        public void BOOL_Encode_FalseReturns0x00()
        {
            var encoded = BOOL.Instance.Encode(false);
            encoded.Should().BeEquivalentTo(new byte[] { 0x00 });
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0x01, true)]
        [InlineData(0xFF, true)]
        [InlineData(0x80, true)]
        public void BOOL_Decode_ReturnsCorrectValue(byte input, bool expected)
        {
            var decoded = BOOL.Instance.Decode(new[] { input });
            decoded.Should().Be(expected);
        }
    }
}
