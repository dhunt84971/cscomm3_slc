using FluentAssertions;
using Xunit;

namespace CSComm3.SLC.Tests
{
    /// <summary>
    /// Tests for the SLCDriver class.
    /// </summary>
    public class SLCDriverTests
    {
        [Fact]
        public void Constructor_WithValidPath_SetsPath()
        {
            // Arrange & Act
            using var driver = new SLCDriver("192.168.1.10");

            // Assert
            driver.Path.Should().Be("192.168.1.10");
        }

        [Fact]
        public void Constructor_WithNullPath_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new SLCDriver(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("path");
        }

        [Fact]
        public void Connected_BeforeOpen_ReturnsFalse()
        {
            // Arrange
            using var driver = new SLCDriver("192.168.1.10");

            // Act & Assert
            driver.Connected.Should().BeFalse();
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var driver = new SLCDriver("192.168.1.10");

            // Act & Assert (should not throw)
            driver.Dispose();
            driver.Dispose();
        }
    }
}
