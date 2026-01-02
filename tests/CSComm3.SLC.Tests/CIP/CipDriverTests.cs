using CSComm3.SLC.CIP;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Tests.Internal;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.CIP
{
    public class CipDriverTests
    {
        [Fact]
        public void Constructor_SetsPathAndHost()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            driver.Path.Should().Be("192.168.1.100");
            driver.Host.Should().Be("192.168.1.100");
            driver.Port.Should().Be(CSComm3.SLC.Constants.DefaultPort);
        }

        [Fact]
        public void Constructor_WithBackplanePath_ParsesRoutePath()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100/1", transport);

            driver.Host.Should().Be("192.168.1.100");
            driver.RoutePath.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_NullOrEmptyPath_ThrowsArgumentNullException(string? path)
        {
            using var transport = new MockTransport();

            var act = () => new TestableCipDriver(path!, transport);

            act.Should().Throw<System.ArgumentNullException>();
        }

        [Fact]
        public void Connected_BeforeOpen_ReturnsFalse()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            driver.Connected.Should().BeFalse();
        }

        [Fact]
        public void Open_RegistersSession()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            // Enqueue RegisterSession response
            transport.EnqueueReceiveData(CreateRegisterSessionResponse(0x12345678));

            var result = driver.Open();

            result.Should().BeTrue();
            driver.Connected.Should().BeTrue();
            driver.SessionHandle.Should().Be(0x12345678);
        }

        [Fact]
        public void Open_WhenAlreadyConnected_ReturnsTrue()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            transport.EnqueueReceiveData(CreateRegisterSessionResponse(0x12345678));
            driver.Open();

            // Second open should just return true
            var result = driver.Open();

            result.Should().BeTrue();
        }

        [Fact]
        public void Open_ConnectionFails_ThrowsCommException()
        {
            using var transport = new MockTransport { ThrowOnConnect = true };
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            var act = () => driver.Open();

            act.Should().Throw<CommException>();
            driver.Connected.Should().BeFalse();
        }

        [Fact]
        public void Close_UnregistersSession()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            transport.EnqueueReceiveData(CreateRegisterSessionResponse(0x12345678));
            driver.Open();

            driver.Close();

            driver.Connected.Should().BeFalse();
            driver.SessionHandle.Should().Be(0);
        }

        [Fact]
        public void Timeout_GetSet_Works()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            driver.Timeout = 10000;

            driver.Timeout.Should().Be(10000);
            transport.SendTimeout.Should().Be(10000);
            transport.ReceiveTimeout.Should().Be(10000);
        }

        [Fact]
        public void Dispose_ClosesConnection()
        {
            var transport = new MockTransport();
            transport.EnqueueReceiveData(CreateRegisterSessionResponse(0x12345678));

            var driver = new TestableCipDriver("192.168.1.100", transport);
            driver.Open();

            driver.Dispose();

            driver.Connected.Should().BeFalse();
        }

        [Fact]
        public void Dispose_WhenCalledTwice_DoesNotThrow()
        {
            var transport = new MockTransport();
            var driver = new TestableCipDriver("192.168.1.100", transport);

            driver.Dispose();
            var act = () => driver.Dispose();

            act.Should().NotThrow();
        }

        [Fact]
        public void Open_AfterDispose_ThrowsObjectDisposedException()
        {
            var transport = new MockTransport();
            var driver = new TestableCipDriver("192.168.1.100", transport);
            driver.Dispose();

            var act = () => driver.Open();

            act.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public async Task OpenAsync_RegistersSession()
        {
            using var transport = new MockTransport();
            using var driver = new TestableCipDriver("192.168.1.100", transport);

            transport.EnqueueReceiveData(CreateRegisterSessionResponse(0x12345678));

            var result = await driver.OpenAsync();

            result.Should().BeTrue();
            driver.Connected.Should().BeTrue();
        }

        private static byte[] CreateRegisterSessionResponse(uint sessionHandle)
        {
            var result = new byte[28]; // Header + 4 bytes data

            // Command: RegisterSession (0x0065)
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
            // Data: Protocol Version + Options
            result[24] = 0x01;
            result[25] = 0x00;
            result[26] = 0x00;
            result[27] = 0x00;

            return result;
        }

        /// <summary>
        /// Testable version of CipDriver that allows injecting a mock transport.
        /// </summary>
        private class TestableCipDriver : CipDriver
        {
            public TestableCipDriver(string path, MockTransport transport)
                : base(path, transport, false)
            {
            }
        }
    }
}
