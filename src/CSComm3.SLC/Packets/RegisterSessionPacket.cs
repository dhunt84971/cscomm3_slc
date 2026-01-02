// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;

namespace CSComm3.SLC.Packets
{
    /// <summary>
    /// Builds a RegisterSession request packet.
    /// </summary>
    /// <remarks>
    /// RegisterSession establishes a session with an EtherNet/IP device.
    /// The request data contains:
    /// - Protocol Version (2 bytes): 0x0001
    /// - Options Flags (2 bytes): 0x0000
    /// </remarks>
    public static class RegisterSessionPacket
    {
        /// <summary>
        /// Builds a RegisterSession request packet.
        /// </summary>
        /// <returns>The request packet bytes.</returns>
        public static byte[] BuildRequest()
        {
            var packet = new RequestPacket
            {
                Command = EncapsulationCommands.RegisterSession,
                SessionHandle = 0 // Not yet established
            };

            // Protocol Version (2 bytes): 0x0001
            packet.Add(Constants.ProtocolVersion);

            // Options Flags (2 bytes): 0x0000
            packet.AddUInt16(0);

            return packet.Build();
        }

        /// <summary>
        /// Parses a RegisterSession response and extracts the session handle.
        /// </summary>
        /// <param name="responseData">The raw response data.</param>
        /// <returns>The session handle.</returns>
        public static uint ParseResponse(byte[] responseData)
        {
            var response = new ResponsePacket(responseData);
            response.ThrowIfError("RegisterSession failed");
            return response.SessionHandle;
        }
    }

    /// <summary>
    /// Builds an UnregisterSession request packet.
    /// </summary>
    public static class UnregisterSessionPacket
    {
        /// <summary>
        /// Builds an UnregisterSession request packet.
        /// </summary>
        /// <param name="sessionHandle">The session handle to unregister.</param>
        /// <returns>The request packet bytes.</returns>
        public static byte[] BuildRequest(uint sessionHandle)
        {
            var packet = new RequestPacket
            {
                Command = EncapsulationCommands.UnregisterSession,
                SessionHandle = sessionHandle
            };

            // No data payload for UnregisterSession
            return packet.Build();
        }
    }
}
