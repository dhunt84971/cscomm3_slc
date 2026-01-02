// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Threading;
using System.Threading.Tasks;
using CSComm3.SLC.Exceptions;
using CSComm3.SLC.Internal;
using CSComm3.SLC.Packets;

namespace CSComm3.SLC.CIP
{
    /// <summary>
    /// Base class for CIP (Common Industrial Protocol) communication.
    /// Handles session management and basic CIP messaging.
    /// </summary>
    public class CipDriver : IDisposable
    {
        private readonly ITransport _transport;
        private readonly bool _ownsTransport;
        private uint _sessionHandle;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CipDriver"/> class.
        /// </summary>
        /// <param name="path">The path to the device (e.g., "192.168.1.100").</param>
        public CipDriver(string path)
            : this(path, new SocketTransport(), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipDriver"/> class with a custom transport.
        /// </summary>
        /// <param name="path">The path to the device.</param>
        /// <param name="transport">The transport to use for communication.</param>
        /// <param name="ownsTransport">Whether this instance owns the transport and should dispose it.</param>
        protected internal CipDriver(string path, ITransport transport, bool ownsTransport)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            Path = path;
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _ownsTransport = ownsTransport;

            var (host, port, routePath) = CipPath.ParsePath(path);
            Host = host;
            Port = port;
            RoutePath = routePath;
        }

        /// <summary>
        /// Gets the path to the device.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the host address.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets the port number.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the route path for the device.
        /// </summary>
        public byte[] RoutePath { get; }

        /// <summary>
        /// Gets the session handle.
        /// </summary>
        public uint SessionHandle => _sessionHandle;

        /// <summary>
        /// Gets a value indicating whether a session is established.
        /// </summary>
        public bool Connected => _transport.IsConnected && _sessionHandle != 0;

        /// <summary>
        /// Gets or sets the timeout in milliseconds.
        /// </summary>
        public int Timeout
        {
            get => _transport.ReceiveTimeout;
            set
            {
                _transport.SendTimeout = value;
                _transport.ReceiveTimeout = value;
            }
        }

        /// <summary>
        /// Opens a connection to the device.
        /// </summary>
        /// <returns>True if the connection was established successfully.</returns>
        public bool Open()
        {
            ThrowIfDisposed();

            if (Connected)
                return true;

            try
            {
                _transport.Connect(Host, Port);
                _sessionHandle = RegisterSession();
                return true;
            }
            catch
            {
                _transport.Close();
                _sessionHandle = 0;
                throw;
            }
        }

        /// <summary>
        /// Opens a connection to the device asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the connection was established successfully.</returns>
        public async Task<bool> OpenAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (Connected)
                return true;

            try
            {
                await _transport.ConnectAsync(Host, Port, cancellationToken).ConfigureAwait(false);
                _sessionHandle = await RegisterSessionAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch
            {
                _transport.Close();
                _sessionHandle = 0;
                throw;
            }
        }

        /// <summary>
        /// Closes the connection to the device.
        /// </summary>
        public void Close()
        {
            if (_sessionHandle != 0 && _transport.IsConnected)
            {
                try
                {
                    var request = UnregisterSessionPacket.BuildRequest(_sessionHandle);
                    _transport.Send(request);
                }
                catch
                {
                    // Ignore errors during unregister
                }
            }

            _sessionHandle = 0;
            _transport.Close();
        }

        /// <summary>
        /// Gets the identity of the connected device.
        /// </summary>
        /// <returns>The device identity.</returns>
        public DeviceIdentity GetIdentity()
        {
            ThrowIfDisposed();
            EnsureConnected();

            var request = ListIdentityPacket.BuildRequest();
            var response = SendReceive(request);
            return ListIdentityPacket.ParseResponse(response);
        }

        /// <summary>
        /// Sends a CIP request using unconnected messaging (SendRRData).
        /// </summary>
        /// <param name="cipRequest">The CIP request data.</param>
        /// <returns>The CIP response data.</returns>
        protected byte[] SendRRData(byte[] cipRequest)
        {
            EnsureConnected();

            var request = SendRRDataPacket.BuildRequest(_sessionHandle, cipRequest);
            var response = SendReceive(request);
            return SendRRDataPacket.ParseResponse(response);
        }

        /// <summary>
        /// Sends a CIP request using unconnected messaging and validates the CIP reply.
        /// </summary>
        /// <param name="cipRequest">The CIP request data.</param>
        /// <param name="expectedService">The expected CIP service code.</param>
        /// <returns>The parsed CIP reply.</returns>
        protected CipReply SendRRDataWithReply(byte[] cipRequest, byte expectedService)
        {
            EnsureConnected();

            var request = SendRRDataPacket.BuildRequest(_sessionHandle, cipRequest);
            var response = SendReceive(request);
            return SendRRDataPacket.ParseCipReply(response, expectedService);
        }

        /// <summary>
        /// Sends a packet and receives the response.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <returns>The response data.</returns>
        protected byte[] SendReceive(byte[] packet)
        {
            _transport.Send(packet);

            // Read header first to get length
            var header = _transport.Receive(Constants.HeaderSize);
            if (header.Length < Constants.HeaderSize)
            {
                throw new CommException($"Incomplete header received: {header.Length} bytes");
            }

            // Get data length from header
            var dataLength = header[2] | (header[3] << 8);

            if (dataLength == 0)
            {
                return header;
            }

            // Read data
            var data = _transport.Receive(dataLength);
            if (data.Length < dataLength)
            {
                throw new CommException($"Incomplete data received: {data.Length} bytes, expected {dataLength}");
            }

            // Combine header and data
            var response = new byte[Constants.HeaderSize + dataLength];
            Array.Copy(header, response, Constants.HeaderSize);
            Array.Copy(data, 0, response, Constants.HeaderSize, dataLength);

            return response;
        }

        /// <summary>
        /// Sends a packet and receives the response asynchronously.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response data.</returns>
        protected async Task<byte[]> SendReceiveAsync(byte[] packet, CancellationToken cancellationToken = default)
        {
            await _transport.SendAsync(packet, cancellationToken).ConfigureAwait(false);

            // Read header first to get length
            var header = await _transport.ReceiveAsync(Constants.HeaderSize, cancellationToken).ConfigureAwait(false);
            if (header.Length < Constants.HeaderSize)
            {
                throw new CommException($"Incomplete header received: {header.Length} bytes");
            }

            // Get data length from header
            var dataLength = header[2] | (header[3] << 8);

            if (dataLength == 0)
            {
                return header;
            }

            // Read data
            var data = await _transport.ReceiveAsync(dataLength, cancellationToken).ConfigureAwait(false);
            if (data.Length < dataLength)
            {
                throw new CommException($"Incomplete data received: {data.Length} bytes, expected {dataLength}");
            }

            // Combine header and data
            var response = new byte[Constants.HeaderSize + dataLength];
            Array.Copy(header, response, Constants.HeaderSize);
            Array.Copy(data, 0, response, Constants.HeaderSize, dataLength);

            return response;
        }

        private uint RegisterSession()
        {
            var request = RegisterSessionPacket.BuildRequest();
            var response = SendReceive(request);
            return RegisterSessionPacket.ParseResponse(response);
        }

        private async Task<uint> RegisterSessionAsync(CancellationToken cancellationToken)
        {
            var request = RegisterSessionPacket.BuildRequest();
            var response = await SendReceiveAsync(request, cancellationToken).ConfigureAwait(false);
            return RegisterSessionPacket.ParseResponse(response);
        }

        private void EnsureConnected()
        {
            if (!Connected)
            {
                throw new CommException("Not connected. Call Open() first.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CipDriver));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the CipDriver and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                    if (_ownsTransport)
                    {
                        _transport.Dispose();
                    }
                }

                _disposed = true;
            }
        }
    }
}
