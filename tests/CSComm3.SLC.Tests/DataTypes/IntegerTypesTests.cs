using CSComm3.SLC.DataTypes;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.DataTypes
{
    public class IntegerTypesTests
    {
        [Theory]
        [InlineData((sbyte)0)]
        [InlineData((sbyte)127)]
        [InlineData((sbyte)-128)]
        [InlineData((sbyte)-1)]
        public void SINT_RoundTrip_PreservesValue(sbyte value)
        {
            var encoded = SINT.Instance.Encode(value);
            var decoded = SINT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void SINT_Properties_AreCorrect()
        {
            SINT.Instance.TypeCode.Should().Be(0xC2);
            SINT.Instance.Size.Should().Be(1);
        }

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)32767)]
        [InlineData((short)-32768)]
        [InlineData((short)-1)]
        public void INT_RoundTrip_PreservesValue(short value)
        {
            var encoded = INT.Instance.Encode(value);
            var decoded = INT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void INT_Properties_AreCorrect()
        {
            INT.Instance.TypeCode.Should().Be(0xC3);
            INT.Instance.Size.Should().Be(2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(12345678)]
        public void DINT_RoundTrip_PreservesValue(int value)
        {
            var encoded = DINT.Instance.Encode(value);
            var decoded = DINT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void DINT_Properties_AreCorrect()
        {
            DINT.Instance.TypeCode.Should().Be(0xC4);
            DINT.Instance.Size.Should().Be(4);
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        [InlineData(-1L)]
        public void LINT_RoundTrip_PreservesValue(long value)
        {
            var encoded = LINT.Instance.Encode(value);
            var decoded = LINT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void LINT_Properties_AreCorrect()
        {
            LINT.Instance.TypeCode.Should().Be(0xC5);
            LINT.Instance.Size.Should().Be(8);
        }

        [Theory]
        [InlineData((byte)0)]
        [InlineData((byte)255)]
        [InlineData((byte)128)]
        public void USINT_RoundTrip_PreservesValue(byte value)
        {
            var encoded = USINT.Instance.Encode(value);
            var decoded = USINT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void USINT_Properties_AreCorrect()
        {
            USINT.Instance.TypeCode.Should().Be(0xC6);
            USINT.Instance.Size.Should().Be(1);
        }

        [Theory]
        [InlineData((ushort)0)]
        [InlineData((ushort)65535)]
        [InlineData((ushort)32768)]
        public void UINT_RoundTrip_PreservesValue(ushort value)
        {
            var encoded = UINT.Instance.Encode(value);
            var decoded = UINT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void UINT_Properties_AreCorrect()
        {
            UINT.Instance.TypeCode.Should().Be(0xC7);
            UINT.Instance.Size.Should().Be(2);
        }

        [Theory]
        [InlineData(0U)]
        [InlineData(uint.MaxValue)]
        [InlineData(2147483648U)]
        public void UDINT_RoundTrip_PreservesValue(uint value)
        {
            var encoded = UDINT.Instance.Encode(value);
            var decoded = UDINT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void UDINT_Properties_AreCorrect()
        {
            UDINT.Instance.TypeCode.Should().Be(0xC8);
            UDINT.Instance.Size.Should().Be(4);
        }

        [Theory]
        [InlineData(0UL)]
        [InlineData(ulong.MaxValue)]
        [InlineData(9223372036854775808UL)]
        public void ULINT_RoundTrip_PreservesValue(ulong value)
        {
            var encoded = ULINT.Instance.Encode(value);
            var decoded = ULINT.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void ULINT_Properties_AreCorrect()
        {
            ULINT.Instance.TypeCode.Should().Be(0xC9);
            ULINT.Instance.Size.Should().Be(8);
        }

        [Fact]
        public void INT_DecodeWithOffset_Works()
        {
            // Arrange
            var buffer = new byte[] { 0x00, 0x00, 0x34, 0x12, 0x00 }; // 0x1234 at offset 2

            // Act
            var result = INT.Instance.Decode(buffer, 2);

            // Assert
            result.Should().Be(0x1234);
        }
    }
}
