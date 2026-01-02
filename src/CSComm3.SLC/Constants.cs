// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
#endif

namespace CSComm3.SLC
{
    /// <summary>
    /// Constants used throughout the CSComm3.SLC library.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Size of the EtherNet/IP encapsulation header in bytes.
        /// </summary>
        public const int HeaderSize = 24;

        /// <summary>
        /// Success status code.
        /// </summary>
        public const int Success = 0x00;

        /// <summary>
        /// Insufficient packets status code (used for multi-packet services).
        /// </summary>
        public const int InsufficientPackets = 0x06;

        /// <summary>
        /// Default EtherNet/IP TCP port.
        /// </summary>
        public const int DefaultPort = 44818;

        /// <summary>
        /// Default socket timeout in seconds.
        /// </summary>
        public const double DefaultTimeout = 5.0;

        /// <summary>
        /// Standard CIP connection size.
        /// </summary>
        public const int StandardConnectionSize = 500;

        /// <summary>
        /// Extended CIP connection size.
        /// </summary>
        public const int ExtendedConnectionSize = 4000;

        /// <summary>
        /// Protocol version for session registration.
        /// </summary>
        public static readonly byte[] ProtocolVersion = { 0x01, 0x00 };
    }

    /// <summary>
    /// EtherNet/IP encapsulation command codes.
    /// </summary>
    public static class EncapsulationCommands
    {
        /// <summary>
        /// NOP command.
        /// </summary>
        public static readonly byte[] Nop = { 0x00, 0x00 };

        /// <summary>
        /// List Services command.
        /// </summary>
        public static readonly byte[] ListServices = { 0x04, 0x00 };

        /// <summary>
        /// List Identity command.
        /// </summary>
        public static readonly byte[] ListIdentity = { 0x63, 0x00 };

        /// <summary>
        /// List Interfaces command.
        /// </summary>
        public static readonly byte[] ListInterfaces = { 0x64, 0x00 };

        /// <summary>
        /// Register Session command.
        /// </summary>
        public static readonly byte[] RegisterSession = { 0x65, 0x00 };

        /// <summary>
        /// Unregister Session command.
        /// </summary>
        public static readonly byte[] UnregisterSession = { 0x66, 0x00 };

        /// <summary>
        /// Send RR Data command (unconnected messaging).
        /// </summary>
        public static readonly byte[] SendRRData = { 0x6F, 0x00 };

        /// <summary>
        /// Send Unit Data command (connected messaging).
        /// </summary>
        public static readonly byte[] SendUnitData = { 0x70, 0x00 };
    }

    /// <summary>
    /// CIP service codes.
    /// </summary>
    public static class CipServices
    {
        /// <summary>
        /// Get Attributes All service.
        /// </summary>
        public const byte GetAttributesAll = 0x01;

        /// <summary>
        /// Get Attribute Single service.
        /// </summary>
        public const byte GetAttributeSingle = 0x0E;

        /// <summary>
        /// Set Attribute Single service.
        /// </summary>
        public const byte SetAttributeSingle = 0x10;

        /// <summary>
        /// Forward Open service.
        /// </summary>
        public const byte ForwardOpen = 0x54;

        /// <summary>
        /// Large Forward Open service (extended connection).
        /// </summary>
        public const byte LargeForwardOpen = 0x5B;

        /// <summary>
        /// Forward Close service.
        /// </summary>
        public const byte ForwardClose = 0x4E;

        /// <summary>
        /// Unconnected Send service.
        /// </summary>
        public const byte UnconnectedSend = 0x52;

        /// <summary>
        /// Execute PCCC service.
        /// </summary>
        public const byte ExecutePCCC = 0x4B;

        /// <summary>
        /// Read Tag service.
        /// </summary>
        public const byte ReadTag = 0x4C;

        /// <summary>
        /// Write Tag service.
        /// </summary>
        public const byte WriteTag = 0x4D;

        /// <summary>
        /// Multiple Service Packet service.
        /// </summary>
        public const byte MultipleServicePacket = 0x0A;
    }

    /// <summary>
    /// PCCC command and function codes.
    /// </summary>
    public static class PcccCommands
    {
        /// <summary>
        /// PCCC command code.
        /// </summary>
        public const byte Command = 0x0F;

        /// <summary>
        /// Protected Typed Logical Read with Three Address Fields.
        /// </summary>
        public const byte ProtectedTypedLogicalRead3Address = 0xA2;

        /// <summary>
        /// Protected Typed Logical Write with Three Address Fields.
        /// </summary>
        public const byte ProtectedTypedLogicalWrite3Address = 0xAA;

        /// <summary>
        /// Protected Typed Logical Write with Mask.
        /// </summary>
        public const byte ProtectedTypedLogicalMaskedWrite = 0xAB;

        /// <summary>
        /// Word Range Read.
        /// </summary>
        public const byte WordRangeRead = 0x01;

        /// <summary>
        /// Word Range Write.
        /// </summary>
        public const byte WordRangeWrite = 0x00;

        /// <summary>
        /// Read SLC File Info.
        /// </summary>
        public const byte ReadFileInfo = 0x06;
    }

    /// <summary>
    /// PCCC/SLC file type codes.
    /// </summary>
    public static class FileTypeCodes
    {
        /// <summary>
        /// Output file (O).
        /// </summary>
        public const byte Output = 0x82;

        /// <summary>
        /// Input file (I).
        /// </summary>
        public const byte Input = 0x83;

        /// <summary>
        /// Status file (S).
        /// </summary>
        public const byte Status = 0x84;

        /// <summary>
        /// Bit file (B).
        /// </summary>
        public const byte Bit = 0x85;

        /// <summary>
        /// Timer file (T).
        /// </summary>
        public const byte Timer = 0x86;

        /// <summary>
        /// Counter file (C).
        /// </summary>
        public const byte Counter = 0x87;

        /// <summary>
        /// Control file (R).
        /// </summary>
        public const byte Control = 0x88;

        /// <summary>
        /// Integer file (N).
        /// </summary>
        public const byte Integer = 0x89;

        /// <summary>
        /// Float file (F).
        /// </summary>
        public const byte Float = 0x8A;

        /// <summary>
        /// String file (ST).
        /// </summary>
        public const byte String = 0x8D;

        /// <summary>
        /// ASCII file (A).
        /// </summary>
        public const byte Ascii = 0x8E;

        /// <summary>
        /// Long Integer file (L).
        /// </summary>
        public const byte Long = 0x91;
    }

    /// <summary>
    /// Timer/Counter sub-element indices.
    /// </summary>
    public static class TimerCounterElements
    {
        /// <summary>
        /// Control word (word 0).
        /// </summary>
        public const int Control = 0;

        /// <summary>
        /// Preset value (word 1).
        /// </summary>
        public const int Preset = 1;

        /// <summary>
        /// Accumulated value (word 2).
        /// </summary>
        public const int Accumulated = 2;

        /// <summary>
        /// Enable bit position.
        /// </summary>
        public const int EnableBit = 15;

        /// <summary>
        /// Timer Timing bit position.
        /// </summary>
        public const int TimerTimingBit = 14;

        /// <summary>
        /// Done bit position.
        /// </summary>
        public const int DoneBit = 13;

        /// <summary>
        /// Count Up enable bit position.
        /// </summary>
        public const int CountUpBit = 15;

        /// <summary>
        /// Count Down enable bit position.
        /// </summary>
        public const int CountDownBit = 14;

        /// <summary>
        /// Overflow bit position.
        /// </summary>
        public const int OverflowBit = 12;

        /// <summary>
        /// Underflow bit position.
        /// </summary>
        public const int UnderflowBit = 11;
    }

    /// <summary>
    /// CIP path segment types.
    /// </summary>
    public static class PathSegments
    {
        /// <summary>
        /// 8-bit Class segment.
        /// </summary>
        public const byte Class8Bit = 0x20;

        /// <summary>
        /// 16-bit Class segment.
        /// </summary>
        public const byte Class16Bit = 0x21;

        /// <summary>
        /// 8-bit Instance segment.
        /// </summary>
        public const byte Instance8Bit = 0x24;

        /// <summary>
        /// 16-bit Instance segment.
        /// </summary>
        public const byte Instance16Bit = 0x25;

        /// <summary>
        /// PCCC Class ID.
        /// </summary>
        public const byte PcccClass = 0x67;
    }
}
