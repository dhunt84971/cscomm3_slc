using CSComm3.SLC;
using CSComm3.SLC.CIP;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.CIP
{
    public class CipPathTests
    {
        [Fact]
        public void BuildLogicalPath_Creates8BitPath()
        {
            var path = CipPath.BuildLogicalPath(0x67, 0x01);

            path.Should().BeEquivalentTo(new byte[]
            {
                PathSegments.Class8Bit, 0x67,
                PathSegments.Instance8Bit, 0x01
            });
        }

        [Fact]
        public void BuildLogicalPath16_Creates16BitPath()
        {
            var path = CipPath.BuildLogicalPath16(0x0102, 0x0304);

            path.Should().BeEquivalentTo(new byte[]
            {
                PathSegments.Class16Bit, 0x00, 0x02, 0x01,
                PathSegments.Instance16Bit, 0x00, 0x04, 0x03
            });
        }

        [Fact]
        public void BuildPcccPath_CreatesCorrectPath()
        {
            var path = CipPath.BuildPcccPath();

            path.Should().BeEquivalentTo(new byte[]
            {
                PathSegments.Class8Bit, PathSegments.PcccClass,
                PathSegments.Instance8Bit, 0x01
            });
        }

        [Fact]
        public void ParsePath_SimpleIpAddress_ParsesCorrectly()
        {
            var (host, port, routePath) = CipPath.ParsePath("192.168.1.100");

            host.Should().Be("192.168.1.100");
            port.Should().Be(Constants.DefaultPort);
            routePath.Should().BeEmpty();
        }

        [Fact]
        public void ParsePath_WithBackplane_ParsesCorrectly()
        {
            var (host, port, routePath) = CipPath.ParsePath("192.168.1.100/1");

            host.Should().Be("192.168.1.100");
            port.Should().Be(Constants.DefaultPort);
            routePath.Should().BeEquivalentTo(new byte[] { 0x01, 0x01 });
        }

        [Fact]
        public void ParsePath_WithSlot_ParsesCorrectly()
        {
            var (host, port, routePath) = CipPath.ParsePath("192.168.1.100/1/2");

            host.Should().Be("192.168.1.100");
            port.Should().Be(Constants.DefaultPort);
            routePath.Should().BeEquivalentTo(new byte[] { 0x01, 0x01 });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ParsePath_NullOrEmpty_ThrowsArgumentException(string? path)
        {
            var act = () => CipPath.ParsePath(path!);

            act.Should().Throw<System.ArgumentException>();
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("hostname")]
        [InlineData("192.168.1")]
        public void ParsePath_InvalidFormat_ThrowsArgumentException(string path)
        {
            var act = () => CipPath.ParsePath(path);

            act.Should().Throw<System.ArgumentException>()
                .WithMessage("*Invalid path format*");
        }

        [Fact]
        public void BuildConnectionPath_WithRoutePath_IncludesSize()
        {
            var routePath = new byte[] { 0x01, 0x00 };

            var connectionPath = CipPath.BuildConnectionPath(routePath);

            connectionPath.Should().BeEquivalentTo(new byte[] { 0x01, 0x01, 0x00 });
        }

        [Fact]
        public void BuildConnectionPath_EmptyRoutePath_ReturnsEmpty()
        {
            var connectionPath = CipPath.BuildConnectionPath(Array.Empty<byte>());

            connectionPath.Should().BeEmpty();
        }

        [Fact]
        public void BuildConnectionPath_NullRoutePath_ReturnsEmpty()
        {
            var connectionPath = CipPath.BuildConnectionPath(null!);

            connectionPath.Should().BeEmpty();
        }
    }
}
