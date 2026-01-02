using CSComm3.SLC.DataTypes;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.DataTypes
{
    public class FloatTypesTests
    {
        [Theory]
        [InlineData(0f)]
        [InlineData(3.14159f)]
        [InlineData(-273.15f)]
        [InlineData(float.MaxValue)]
        [InlineData(float.MinValue)]
        [InlineData(float.Epsilon)]
        public void REAL_RoundTrip_PreservesValue(float value)
        {
            var encoded = REAL.Instance.Encode(value);
            var decoded = REAL.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void REAL_Properties_AreCorrect()
        {
            REAL.Instance.TypeCode.Should().Be(0xCA);
            REAL.Instance.Size.Should().Be(4);
        }

        [Theory]
        [InlineData(0d)]
        [InlineData(3.141592653589793d)]
        [InlineData(-273.15d)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        public void LREAL_RoundTrip_PreservesValue(double value)
        {
            var encoded = LREAL.Instance.Encode(value);
            var decoded = LREAL.Instance.Decode(encoded);
            decoded.Should().Be(value);
        }

        [Fact]
        public void LREAL_Properties_AreCorrect()
        {
            LREAL.Instance.TypeCode.Should().Be(0xCB);
            LREAL.Instance.Size.Should().Be(8);
        }
    }
}
