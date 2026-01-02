using CSComm3.SLC.Exceptions;
using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests.Exceptions
{
    public class ExceptionTests
    {
        [Fact]
        public void CommunicationException_WithMessage_SetsMessage()
        {
            var ex = new CommunicationException("Test message");
            ex.Message.Should().Be("Test message");
        }

        [Fact]
        public void CommunicationException_WithInnerException_SetsInnerException()
        {
            var inner = new InvalidOperationException("Inner");
            var ex = new CommunicationException("Outer", inner);

            ex.Message.Should().Be("Outer");
            ex.InnerException.Should().BeSameAs(inner);
        }

        [Fact]
        public void CommException_InheritsFromCommunicationException()
        {
            var ex = new CommException("Socket error");
            ex.Should().BeAssignableTo<CommunicationException>();
        }

        [Fact]
        public void DataException_InheritsFromCommunicationException()
        {
            var ex = new DataException("Decode error");
            ex.Should().BeAssignableTo<CommunicationException>();
        }

        [Fact]
        public void BufferEmptyException_InheritsFromDataException()
        {
            var ex = new BufferEmptyException();
            ex.Should().BeAssignableTo<DataException>();
            ex.Message.Should().Be("Buffer is empty");
        }

        [Fact]
        public void ResponseException_WithStatusCodes_SetsProperties()
        {
            var ex = new ResponseException("Response error", 0x10, 0x2001);

            ex.Message.Should().Be("Response error");
            ex.StatusCode.Should().Be(0x10);
            ex.ExtendedStatus.Should().Be(0x2001);
        }

        [Fact]
        public void ResponseException_InheritsFromCommunicationException()
        {
            var ex = new ResponseException("Error");
            ex.Should().BeAssignableTo<CommunicationException>();
        }

        [Fact]
        public void RequestException_InheritsFromCommunicationException()
        {
            var ex = new RequestException("Build error");
            ex.Should().BeAssignableTo<CommunicationException>();
        }
    }
}
