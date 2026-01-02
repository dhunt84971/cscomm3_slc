using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Tests.Internal;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.Internal
{
    public class TransportTests
    {
        [Fact]
        public void MockTransport_Connect_SetsIsConnected()
        {
            using var transport = new MockTransport();

            transport.IsConnected.Should().BeFalse();
            transport.Connect("192.168.1.1", 44818);
            transport.IsConnected.Should().BeTrue();
        }

        [Fact]
        public void MockTransport_Connect_RecordsHostAndPort()
        {
            using var transport = new MockTransport();

            transport.Connect("192.168.1.1", 44818);

            transport.ConnectedHost.Should().Be("192.168.1.1");
            transport.ConnectedPort.Should().Be(44818);
        }

        [Fact]
        public void MockTransport_ThrowOnConnect_ThrowsCommException()
        {
            using var transport = new MockTransport { ThrowOnConnect = true };

            var act = () => transport.Connect("192.168.1.1", 44818);

            act.Should().Throw<CommException>();
        }

        [Fact]
        public void MockTransport_Send_RecordsData()
        {
            using var transport = new MockTransport();
            transport.Connect("192.168.1.1", 44818);

            var data = new byte[] { 0x01, 0x02, 0x03 };
            var sent = transport.Send(data);

            sent.Should().Be(3);
            transport.SentData.Should().HaveCount(1);
            transport.SentData[0].Should().BeEquivalentTo(data);
        }

        [Fact]
        public void MockTransport_Send_WhenNotConnected_ThrowsCommException()
        {
            using var transport = new MockTransport();

            var act = () => transport.Send(new byte[] { 0x01 });

            act.Should().Throw<CommException>()
                .WithMessage("Not connected");
        }

        [Fact]
        public void MockTransport_ThrowOnSend_ThrowsCommException()
        {
            using var transport = new MockTransport { ThrowOnSend = true };
            transport.Connect("192.168.1.1", 44818);

            var act = () => transport.Send(new byte[] { 0x01 });

            act.Should().Throw<CommException>();
        }

        [Fact]
        public void MockTransport_Receive_ReturnsEnqueuedData()
        {
            using var transport = new MockTransport();
            transport.Connect("192.168.1.1", 44818);

            var expectedData = new byte[] { 0x65, 0x00, 0x04, 0x00 };
            transport.EnqueueReceiveData(expectedData);

            var received = transport.Receive(4);

            received.Should().BeEquivalentTo(expectedData);
        }

        [Fact]
        public void MockTransport_Receive_WhenNotConnected_ThrowsCommException()
        {
            using var transport = new MockTransport();

            var act = () => transport.Receive(4);

            act.Should().Throw<CommException>()
                .WithMessage("Not connected");
        }

        [Fact]
        public void MockTransport_ThrowOnReceive_ThrowsCommException()
        {
            using var transport = new MockTransport { ThrowOnReceive = true };
            transport.Connect("192.168.1.1", 44818);

            var act = () => transport.Receive(4);

            act.Should().Throw<CommException>();
        }

        [Fact]
        public void MockTransport_Close_SetsIsConnectedToFalse()
        {
            using var transport = new MockTransport();
            transport.Connect("192.168.1.1", 44818);

            transport.Close();

            transport.IsConnected.Should().BeFalse();
        }

        [Fact]
        public void MockTransport_Timeout_Properties()
        {
            using var transport = new MockTransport();

            transport.SendTimeout = 10000;
            transport.ReceiveTimeout = 15000;

            transport.SendTimeout.Should().Be(10000);
            transport.ReceiveTimeout.Should().Be(15000);
        }

        [Fact]
        public async Task MockTransport_ConnectAsync_Works()
        {
            using var transport = new MockTransport();

            await transport.ConnectAsync("192.168.1.1", 44818);

            transport.IsConnected.Should().BeTrue();
        }

        [Fact]
        public async Task MockTransport_SendAsync_Works()
        {
            using var transport = new MockTransport();
            await transport.ConnectAsync("192.168.1.1", 44818);

            var data = new byte[] { 0x01, 0x02 };
            var sent = await transport.SendAsync(data);

            sent.Should().Be(2);
        }

        [Fact]
        public async Task MockTransport_ReceiveAsync_Works()
        {
            using var transport = new MockTransport();
            await transport.ConnectAsync("192.168.1.1", 44818);

            var expectedData = new byte[] { 0x01, 0x02, 0x03 };
            transport.EnqueueReceiveData(expectedData);

            var received = await transport.ReceiveAsync(3);

            received.Should().BeEquivalentTo(expectedData);
        }

        [Fact]
        public void MockTransport_Reset_ClearsState()
        {
            using var transport = new MockTransport();
            transport.Connect("192.168.1.1", 44818);
            transport.Send(new byte[] { 0x01 });
            transport.EnqueueReceiveData(new byte[] { 0x02 });
            transport.ThrowOnConnect = true;

            transport.Reset();

            transport.SentData.Should().BeEmpty();
            transport.ThrowOnConnect.Should().BeFalse();
        }

        [Fact]
        public void MockTransport_Dispose_CleansUp()
        {
            var transport = new MockTransport();
            transport.Connect("192.168.1.1", 44818);

            transport.Dispose();

            transport.IsConnected.Should().BeFalse();
        }

        [Fact]
        public void MockTransport_Dispose_ThrowsOnSubsequentUse()
        {
            var transport = new MockTransport();
            transport.Dispose();

            var act = () => transport.Connect("192.168.1.1", 44818);

            act.Should().Throw<ObjectDisposedException>();
        }
    }
}
