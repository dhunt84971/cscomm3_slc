// CSComm3.SLC - C# SLC PLC Communication Library
// Test mock for transport abstraction

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSComm3.SLC.Internal;

namespace CSComm3.SLC.Tests.Internal
{
    /// <summary>
    /// Mock transport for unit testing without actual network connections.
    /// </summary>
    public class MockTransport : ITransport
    {
        private bool _disposed;
        private bool _isConnected;
        private readonly Queue<byte[]> _receiveQueue = new Queue<byte[]>();
        private readonly List<byte[]> _sentData = new List<byte[]>();

        /// <summary>
        /// Gets the list of data that was sent.
        /// </summary>
        public IReadOnlyList<byte[]> SentData => _sentData;

        /// <summary>
        /// Gets the host that was connected to.
        /// </summary>
        public string? ConnectedHost { get; private set; }

        /// <summary>
        /// Gets the port that was connected to.
        /// </summary>
        public int ConnectedPort { get; private set; }

        /// <summary>
        /// Gets or sets whether Connect should throw an exception.
        /// </summary>
        public bool ThrowOnConnect { get; set; }

        /// <summary>
        /// Gets or sets whether Send should throw an exception.
        /// </summary>
        public bool ThrowOnSend { get; set; }

        /// <summary>
        /// Gets or sets whether Receive should throw an exception.
        /// </summary>
        public bool ThrowOnReceive { get; set; }

        /// <inheritdoc/>
        public bool IsConnected => _isConnected;

        /// <inheritdoc/>
        public int SendTimeout { get; set; } = 5000;

        /// <inheritdoc/>
        public int ReceiveTimeout { get; set; } = 5000;

        /// <summary>
        /// Enqueues data to be returned by the next Receive call.
        /// </summary>
        /// <param name="data">The data to return.</param>
        public void EnqueueReceiveData(byte[] data)
        {
            _receiveQueue.Enqueue(data);
        }

        /// <inheritdoc/>
        public void Connect(string host, int port)
        {
            ThrowIfDisposed();

            if (ThrowOnConnect)
            {
                throw new CSComm3.SLC.Exceptions.CommException($"Mock connection failed to {host}:{port}");
            }

            ConnectedHost = host;
            ConnectedPort = port;
            _isConnected = true;
        }

        /// <inheritdoc/>
        public Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            Connect(host, port);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public int Send(byte[] data)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            if (ThrowOnSend)
            {
                throw new CSComm3.SLC.Exceptions.CommException("Mock send failed");
            }

            _sentData.Add(data);
            return data.Length;
        }

        /// <inheritdoc/>
        public Task<int> SendAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Send(data));
        }

        /// <inheritdoc/>
        public byte[] Receive(int size)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            if (ThrowOnReceive)
            {
                throw new CSComm3.SLC.Exceptions.CommException("Mock receive failed");
            }

            if (_receiveQueue.Count > 0)
            {
                var data = _receiveQueue.Dequeue();
                if (data.Length <= size)
                {
                    return data;
                }

                // Return only requested size
                var result = new byte[size];
                Array.Copy(data, result, size);

                // Re-queue remaining data
                var remaining = new byte[data.Length - size];
                Array.Copy(data, size, remaining, 0, remaining.Length);
                var temp = new Queue<byte[]>();
                temp.Enqueue(remaining);
                while (_receiveQueue.Count > 0)
                {
                    temp.Enqueue(_receiveQueue.Dequeue());
                }
                while (temp.Count > 0)
                {
                    _receiveQueue.Enqueue(temp.Dequeue());
                }

                return result;
            }

            return Array.Empty<byte>();
        }

        /// <inheritdoc/>
        public Task<byte[]> ReceiveAsync(int size, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Receive(size));
        }

        /// <inheritdoc/>
        public void Close()
        {
            _isConnected = false;
            ConnectedHost = null;
            ConnectedPort = 0;
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

        /// <summary>
        /// Resets the mock state.
        /// </summary>
        public void Reset()
        {
            _sentData.Clear();
            _receiveQueue.Clear();
            ThrowOnConnect = false;
            ThrowOnSend = false;
            ThrowOnReceive = false;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MockTransport));
            }
        }

        private void ThrowIfNotConnected()
        {
            if (!_isConnected)
            {
                throw new CSComm3.SLC.Exceptions.CommException("Not connected");
            }
        }
    }
}
