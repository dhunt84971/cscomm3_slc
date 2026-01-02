// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CSComm3.SLC.Exceptions;

namespace CSComm3.SLC.Internal
{
    /// <summary>
    /// TCP socket transport implementation for EtherNet/IP communication.
    /// </summary>
    internal sealed class SocketTransport : ITransport
    {
        private Socket? _socket;
        private bool _disposed;
        private int _sendTimeout = 5000;
        private int _receiveTimeout = 5000;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketTransport"/> class.
        /// </summary>
        public SocketTransport()
        {
        }

        /// <inheritdoc/>
        public bool IsConnected => _socket?.Connected ?? false;

        /// <inheritdoc/>
        public int SendTimeout
        {
            get => _sendTimeout;
            set
            {
                _sendTimeout = value;
                if (_socket != null)
                {
                    _socket.SendTimeout = value;
                }
            }
        }

        /// <inheritdoc/>
        public int ReceiveTimeout
        {
            get => _receiveTimeout;
            set
            {
                _receiveTimeout = value;
                if (_socket != null)
                {
                    _socket.ReceiveTimeout = value;
                }
            }
        }

        /// <inheritdoc/>
        public void Connect(string host, int port)
        {
            ThrowIfDisposed();

            try
            {
                _socket?.Dispose();
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = _sendTimeout,
                    ReceiveTimeout = _receiveTimeout,
                    NoDelay = true
                };

                _socket.Connect(host, port);
            }
            catch (SocketException ex)
            {
                throw new CommException($"Failed to connect to {host}:{port}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            try
            {
                _socket?.Dispose();
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = _sendTimeout,
                    ReceiveTimeout = _receiveTimeout,
                    NoDelay = true
                };

#if NETSTANDARD2_0
                await Task.Factory.FromAsync(
                    _socket.BeginConnect(host, port, null, null),
                    _socket.EndConnect).ConfigureAwait(false);
#else
                await _socket.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
#endif
            }
            catch (SocketException ex)
            {
                throw new CommException($"Failed to connect to {host}:{port}", ex);
            }
        }

        /// <inheritdoc/>
        public int Send(byte[] data)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            try
            {
                return _socket!.Send(data);
            }
            catch (SocketException ex)
            {
                throw new CommException("Failed to send data", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<int> SendAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            try
            {
#if NETSTANDARD2_0
                var segment = new ArraySegment<byte>(data);
                return await Task.Factory.FromAsync(
                    (callback, state) => _socket!.BeginSend(data, 0, data.Length, SocketFlags.None, callback, state),
                    _socket!.EndSend,
                    null).ConfigureAwait(false);
#else
                return await _socket!.SendAsync(data, SocketFlags.None, cancellationToken).ConfigureAwait(false);
#endif
            }
            catch (SocketException ex)
            {
                throw new CommException("Failed to send data", ex);
            }
        }

        /// <inheritdoc/>
        public byte[] Receive(int size)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            try
            {
                var buffer = new byte[size];
                int totalReceived = 0;

                while (totalReceived < size)
                {
                    int received = _socket!.Receive(buffer, totalReceived, size - totalReceived, SocketFlags.None);
                    if (received == 0)
                    {
                        break; // Connection closed
                    }
                    totalReceived += received;
                }

                if (totalReceived < size)
                {
                    // Return only what was received
                    var result = new byte[totalReceived];
                    Array.Copy(buffer, result, totalReceived);
                    return result;
                }

                return buffer;
            }
            catch (SocketException ex)
            {
                throw new CommException("Failed to receive data", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReceiveAsync(int size, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            try
            {
                var buffer = new byte[size];
                int totalReceived = 0;

                while (totalReceived < size)
                {
#if NETSTANDARD2_0
                    int received = await Task.Factory.FromAsync(
                        (callback, state) => _socket!.BeginReceive(buffer, totalReceived, size - totalReceived, SocketFlags.None, callback, state),
                        _socket!.EndReceive,
                        null).ConfigureAwait(false);
#else
                    int received = await _socket!.ReceiveAsync(
                        new Memory<byte>(buffer, totalReceived, size - totalReceived),
                        SocketFlags.None,
                        cancellationToken).ConfigureAwait(false);
#endif
                    if (received == 0)
                    {
                        break; // Connection closed
                    }
                    totalReceived += received;
                }

                if (totalReceived < size)
                {
                    // Return only what was received
                    var result = new byte[totalReceived];
                    Array.Copy(buffer, result, totalReceived);
                    return result;
                }

                return buffer;
            }
            catch (SocketException ex)
            {
                throw new CommException("Failed to receive data", ex);
            }
        }

        /// <inheritdoc/>
        public void Close()
        {
            if (_socket != null)
            {
                try
                {
                    if (_socket.Connected)
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (SocketException)
                {
                    // Ignore errors during shutdown
                }
                finally
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                Close();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SocketTransport));
            }
        }

        private void ThrowIfNotConnected()
        {
            if (!IsConnected)
            {
                throw new CommException("Not connected");
            }
        }
    }
}
