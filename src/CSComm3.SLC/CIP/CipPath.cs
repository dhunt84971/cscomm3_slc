// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSComm3.SLC.CIP
{
    /// <summary>
    /// Builds CIP path segments for routing messages to target devices.
    /// </summary>
    public static class CipPath
    {
        private static readonly Regex RoutePathRegex = new Regex(
            @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})(?:/(\d+))?(?:/(\d+))?$",
            RegexOptions.Compiled);

        /// <summary>
        /// Builds an EPATH segment for a logical address.
        /// </summary>
        /// <param name="classId">The class ID.</param>
        /// <param name="instanceId">The instance ID.</param>
        /// <returns>The EPATH bytes.</returns>
        public static byte[] BuildLogicalPath(byte classId, byte instanceId)
        {
            return new byte[]
            {
                PathSegments.Class8Bit, classId,
                PathSegments.Instance8Bit, instanceId
            };
        }

        /// <summary>
        /// Builds an EPATH segment for a logical address with 16-bit IDs.
        /// </summary>
        /// <param name="classId">The class ID.</param>
        /// <param name="instanceId">The instance ID.</param>
        /// <returns>The EPATH bytes.</returns>
        public static byte[] BuildLogicalPath16(ushort classId, ushort instanceId)
        {
            return new byte[]
            {
                PathSegments.Class16Bit, 0x00,
                (byte)(classId & 0xFF), (byte)((classId >> 8) & 0xFF),
                PathSegments.Instance16Bit, 0x00,
                (byte)(instanceId & 0xFF), (byte)((instanceId >> 8) & 0xFF)
            };
        }

        /// <summary>
        /// Builds the PCCC object path.
        /// </summary>
        /// <returns>The PCCC object EPATH bytes.</returns>
        public static byte[] BuildPcccPath()
        {
            return BuildLogicalPath(PathSegments.PcccClass, 0x01);
        }

        /// <summary>
        /// Parses a path string and builds route and connection paths.
        /// </summary>
        /// <param name="path">The path string (e.g., "192.168.1.100" or "192.168.1.100/1/0").</param>
        /// <returns>A tuple of (host, port, routePath).</returns>
        public static (string host, int port, byte[] routePath) ParsePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            var match = RoutePathRegex.Match(path);
            if (!match.Success)
                throw new ArgumentException($"Invalid path format: {path}", nameof(path));

            var host = match.Groups[1].Value;
            var port = Constants.DefaultPort;

            // Parse slot/port routing if present
            var routePath = new List<byte>();

            if (match.Groups[2].Success)
            {
                var backplane = int.Parse(match.Groups[2].Value);

                // Port segment (backplane = 1)
                routePath.Add(0x01);
                routePath.Add((byte)backplane);
            }

            if (match.Groups[3].Success)
            {
                var slot = int.Parse(match.Groups[3].Value);

                // Slot routing is already encoded in backplane segment
                // If additional slot specified, use port/link address format
                if (routePath.Count == 0)
                {
                    routePath.Add(0x01);
                    routePath.Add((byte)slot);
                }
            }

            return (host, port, routePath.ToArray());
        }

        /// <summary>
        /// Builds a connection path for connected messaging.
        /// </summary>
        /// <param name="routePath">The route path bytes.</param>
        /// <returns>The connection path including size.</returns>
        public static byte[] BuildConnectionPath(byte[] routePath)
        {
            if (routePath == null || routePath.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var result = new byte[routePath.Length + 1];
            result[0] = (byte)(routePath.Length / 2); // Path size in words
            Array.Copy(routePath, 0, result, 1, routePath.Length);
            return result;
        }
    }
}
