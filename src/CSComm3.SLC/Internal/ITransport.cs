// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

#if NETSTANDARD2_0
using System;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace CSComm3.SLC.Internal
{
    /// <summary>
    /// Interface for transport layer abstraction, enabling mockable socket communications.
    /// </summary>
    public interface ITransport : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the transport is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets or sets the send timeout in milliseconds.
        /// </summary>
        int SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the receive timeout in milliseconds.
        /// </summary>
        int ReceiveTimeout { get; set; }

        /// <summary>
        /// Connects to the specified host and port.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        void Connect(string host, int port);

        /// <summary>
        /// Connects to the specified host and port asynchronously.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends data over the transport.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>The number of bytes sent.</returns>
        int Send(byte[] data);

        /// <summary>
        /// Sends data over the transport asynchronously.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the number of bytes sent.</returns>
        Task<int> SendAsync(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives data from the transport.
        /// </summary>
        /// <param name="size">The maximum number of bytes to receive.</param>
        /// <returns>The received data.</returns>
        byte[] Receive(int size);

        /// <summary>
        /// Receives data from the transport asynchronously.
        /// </summary>
        /// <param name="size">The maximum number of bytes to receive.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the received data.</returns>
        Task<byte[]> ReceiveAsync(int size, CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes the transport connection.
        /// </summary>
        void Close();
    }
}
