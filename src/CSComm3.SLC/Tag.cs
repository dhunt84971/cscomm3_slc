// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSComm3.SLC.Exceptions;

namespace CSComm3.SLC
{
    /// <summary>
    /// Represents a parsed SLC/MicroLogix tag address.
    /// </summary>
    /// <remarks>
    /// SLC addressing format: FileType:FileNumber[/BitNumber]
    /// Examples:
    /// - N7:0     - Integer file 7, element 0
    /// - N7:5     - Integer file 7, element 5
    /// - F8:0     - Float file 8, element 0
    /// - B3:0/0   - Bit file 3, element 0, bit 0
    /// - T4:0.ACC - Timer file 4, element 0, accumulated value
    /// - T4:0.PRE - Timer file 4, element 0, preset value
    /// - C5:0.ACC - Counter file 5, element 0, accumulated value
    /// - ST9:0    - String file 9, element 0
    /// </remarks>
    public class Tag
    {
        /// <summary>
        /// Gets the original tag address string.
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Gets the file type letter (N, F, B, T, C, S, ST, A, R, O, I, L).
        /// </summary>
        public string FileType { get; }

        /// <summary>
        /// Gets the file type code for PCCC.
        /// </summary>
        public byte FileTypeCode { get; }

        /// <summary>
        /// Gets the file number.
        /// </summary>
        public byte FileNumber { get; }

        /// <summary>
        /// Gets the element number.
        /// </summary>
        public byte ElementNumber { get; }

        /// <summary>
        /// Gets the sub-element number (for Timer/Counter structures).
        /// </summary>
        public byte SubElement { get; }

        /// <summary>
        /// Gets the bit number (for bit-level access).
        /// </summary>
        public int? BitNumber { get; }

        /// <summary>
        /// Gets the size of one element in bytes.
        /// </summary>
        public int ElementSize { get; }

        /// <summary>
        /// Gets a value indicating whether this is a bit-level tag.
        /// </summary>
        public bool IsBit => BitNumber.HasValue;

        /// <summary>
        /// Gets the value associated with this tag.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="address">The tag address string.</param>
        public Tag(string address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));

            var parsed = TagParser.Parse(address);
            FileType = parsed.FileType;
            FileTypeCode = parsed.FileTypeCode;
            FileNumber = parsed.FileNumber;
            ElementNumber = parsed.ElementNumber;
            SubElement = parsed.SubElement;
            BitNumber = parsed.BitNumber;
            ElementSize = parsed.ElementSize;
        }

        /// <inheritdoc/>
        public override string ToString() => Address;
    }

    /// <summary>
    /// Parses SLC/MicroLogix tag address strings.
    /// </summary>
    public static class TagParser
    {
        // Pattern: FileType FileNumber : ElementNumber [.SubElement | /BitNumber]
        // Examples: N7:0, F8:5, B3:0/5, T4:0.ACC, C5:2.PRE
        private static readonly Regex TagPattern = new Regex(
            @"^([NFBTCSROIAL]|ST)(\d+):(\d+)(?:\.(\w+)|/(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Maps file type letters to PCCC file type codes
        private static readonly Dictionary<string, byte> FileTypeCodes = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
        {
            { "O", CSComm3.SLC.FileTypeCodes.Output },
            { "I", CSComm3.SLC.FileTypeCodes.Input },
            { "S", CSComm3.SLC.FileTypeCodes.Status },
            { "B", CSComm3.SLC.FileTypeCodes.Bit },
            { "T", CSComm3.SLC.FileTypeCodes.Timer },
            { "C", CSComm3.SLC.FileTypeCodes.Counter },
            { "R", CSComm3.SLC.FileTypeCodes.Control },
            { "N", CSComm3.SLC.FileTypeCodes.Integer },
            { "F", CSComm3.SLC.FileTypeCodes.Float },
            { "ST", CSComm3.SLC.FileTypeCodes.String },
            { "A", CSComm3.SLC.FileTypeCodes.Ascii },
            { "L", CSComm3.SLC.FileTypeCodes.Long }
        };

        // Maps file types to element sizes in bytes
        private static readonly Dictionary<string, int> ElementSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "O", 2 },  // Output - 16-bit word
            { "I", 2 },  // Input - 16-bit word
            { "S", 2 },  // Status - 16-bit word
            { "B", 2 },  // Bit - 16-bit word
            { "T", 6 },  // Timer - 3 x 16-bit words
            { "C", 6 },  // Counter - 3 x 16-bit words
            { "R", 6 },  // Control - 3 x 16-bit words
            { "N", 2 },  // Integer - 16-bit word
            { "F", 4 },  // Float - 32-bit
            { "ST", 84 }, // String - 82 chars + 2 length bytes
            { "A", 2 },  // ASCII - 2 characters
            { "L", 4 }   // Long - 32-bit
        };

        // Maps sub-element names to offsets for Timer/Counter/Control
        private static readonly Dictionary<string, byte> SubElements = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
        {
            { "CON", 0 },  // Control word
            { "CTL", 0 },  // Control word (alias)
            { "PRE", 1 },  // Preset
            { "ACC", 2 },  // Accumulated
            { "LEN", 1 },  // Length (for Control)
            { "POS", 2 }   // Position (for Control)
        };

        /// <summary>
        /// Parses a tag address string.
        /// </summary>
        /// <param name="address">The tag address string.</param>
        /// <returns>The parsed tag information.</returns>
        public static ParsedTag Parse(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Tag address cannot be null or empty", nameof(address));

            var match = TagPattern.Match(address);
            if (!match.Success)
                throw new RequestException($"Invalid tag address format: {address}");

            var fileType = match.Groups[1].Value.ToUpperInvariant();
            var fileNumber = byte.Parse(match.Groups[2].Value);
            var elementNumber = byte.Parse(match.Groups[3].Value);

            if (!FileTypeCodes.TryGetValue(fileType, out var fileTypeCode))
                throw new RequestException($"Unknown file type: {fileType}");

            if (!ElementSizes.TryGetValue(fileType, out var elementSize))
                elementSize = 2;

            byte subElement = 0;
            int? bitNumber = null;

            // Check for sub-element (e.g., T4:0.ACC)
            if (match.Groups[4].Success)
            {
                var subElementName = match.Groups[4].Value;
                if (!SubElements.TryGetValue(subElementName, out subElement))
                    throw new RequestException($"Unknown sub-element: {subElementName}");
            }

            // Check for bit number (e.g., B3:0/5)
            if (match.Groups[5].Success)
            {
                bitNumber = int.Parse(match.Groups[5].Value);
                if (bitNumber < 0 || bitNumber > 15)
                    throw new RequestException($"Bit number must be 0-15: {bitNumber}");
            }

            return new ParsedTag
            {
                FileType = fileType,
                FileTypeCode = fileTypeCode,
                FileNumber = fileNumber,
                ElementNumber = elementNumber,
                SubElement = subElement,
                BitNumber = bitNumber,
                ElementSize = elementSize
            };
        }

        /// <summary>
        /// Tries to parse a tag address string.
        /// </summary>
        /// <param name="address">The tag address string.</param>
        /// <param name="result">The parsed tag information.</param>
        /// <returns>True if parsing succeeded.</returns>
        public static bool TryParse(string address, out ParsedTag? result)
        {
            try
            {
                result = Parse(address);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }

    /// <summary>
    /// Contains parsed tag information.
    /// </summary>
    public class ParsedTag
    {
        /// <summary>
        /// Gets or sets the file type letter.
        /// </summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file type code.
        /// </summary>
        public byte FileTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the file number.
        /// </summary>
        public byte FileNumber { get; set; }

        /// <summary>
        /// Gets or sets the element number.
        /// </summary>
        public byte ElementNumber { get; set; }

        /// <summary>
        /// Gets or sets the sub-element number.
        /// </summary>
        public byte SubElement { get; set; }

        /// <summary>
        /// Gets or sets the bit number.
        /// </summary>
        public int? BitNumber { get; set; }

        /// <summary>
        /// Gets or sets the element size in bytes.
        /// </summary>
        public int ElementSize { get; set; }
    }
}
