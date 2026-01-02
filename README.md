# CSComm3.SLC

A C# .NET library for communicating with Allen Bradley SLC and MicroLogix PLCs over Ethernet.

**Status:** Complete - Read and Write operations verified against real hardware

## Overview

CSComm3.SLC is a C# port of the SLC driver functionality from [pycomm3](https://github.com/ottowayi/pycomm3). It provides a simple API for reading and writing data to SLC 5/05 and MicroLogix PLCs using the PCCC protocol over EtherNet/IP.

## Supported Hardware

- Allen Bradley SLC 5/05 (Ethernet-enabled)
- Allen Bradley MicroLogix 1100 (1763-xxx)
- Allen Bradley MicroLogix 1200 (1762-xxx)
- Allen Bradley MicroLogix 1400 (1766-xxx)
- Allen Bradley MicroLogix 1500 (1764-xxx)

## Features

- [x] EtherNet/IP session management
- [x] CIP Forward Open/Close connections
- [x] Read operations for all data types
- [x] Write operations for integers and bits
- [x] Device identity retrieval
- [x] Async/await support
- [x] Multiple tag reads in single call

## Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/cscomm3_slc.git

# Build the library
dotnet build

# Run tests
dotnet test
```

## Quick Start

```csharp
using CSComm3.SLC;

// Connect to PLC
using var plc = new SLCDriver("192.168.1.150");
plc.Open();

// Read a single integer
var tag = plc.Read("N7:0");
Console.WriteLine($"N7:0 = {tag.Value}");

// Read multiple tags
var tags = plc.Read("N7:0", "N7:1", "F8:0");
foreach (var t in tags)
{
    Console.WriteLine($"{t.Address} = {t.Value}");
}

// Write an integer
plc.Write("N7:1", (short)100);

// Write a bit
plc.Write("B3:0/0", true);

// Async read
var result = await plc.ReadAsync("N7:0");
```

## Supported Data Types

| Address Format | Type | Example | C# Type |
|---------------|------|---------|---------|
| N*x*:*y* | Integer | N7:0 | Int16 |
| F*x*:*y* | Float | F8:0 | Single |
| B*x*:*y*/*z* | Bit | B3:0/5 | Boolean |
| S*x*:*y* | Status | S2:37 | Int16 |
| T*x*:*y*.ACC | Timer Accumulated | T4:0.ACC | Int16 |
| T*x*:*y*.PRE | Timer Preset | T4:0.PRE | Int16 |
| C*x*:*y*.ACC | Counter Accumulated | C5:0.ACC | Int16 |
| C*x*:*y*.PRE | Counter Preset | C5:0.PRE | Int16 |
| I:*y* | Input | I:0 | Int16 |
| O:*y* | Output | O:0 | Int16 |

## API Reference

### SLCDriver Class

```csharp
public class SLCDriver : IDisposable
{
    // Constructor
    public SLCDriver(string ipAddress);

    // Properties
    public bool Connected { get; }
    public int Timeout { get; set; }  // milliseconds

    // Connection
    public bool Open();
    public void Close();

    // Read Operations
    public Tag Read(string address);
    public Tag[] Read(params string[] addresses);
    public Task<Tag> ReadAsync(string address);
    public Task<Tag[]> ReadAsync(params string[] addresses);

    // Write Operations
    public bool Write(string address, object value);
    public Task<bool> WriteAsync(string address, object value);

    // Device Info
    public DeviceIdentity GetIdentity();
}
```

### Tag Class

```csharp
public class Tag
{
    public string Address { get; }
    public object? Value { get; }
    public string? Error { get; }
}
```

## Tech Stack

- **Language:** C# 12.0
- **Runtime:** .NET 8.0
- **Testing:** xUnit, FluentAssertions

## Project Structure

```
cscomm3_slc/
├── src/
│   └── CSComm3.SLC/
│       ├── SLCDriver.cs          # Main driver class
│       ├── Tag.cs                # Tag result class
│       ├── CIP/                  # CIP protocol components
│       ├── PCCC/                 # PCCC protocol components
│       ├── Packets/              # EtherNet/IP packets
│       └── Exceptions/           # Custom exceptions
├── tests/
│   └── CSComm3.SLC.Tests/        # Unit tests (205 tests)
├── samples/
│   └── TestApp/                  # Sample application
└── docs/
    └── PRD.md                    # Product requirements
```

## Testing

The library includes 205 unit tests covering:
- Tag address parsing
- PCCC protocol message building
- Data type encoding/decoding
- CIP packet construction

```bash
dotnet test
```

## Verified Against Hardware

Successfully tested against:
- **PLC:** Allen Bradley 1747-L551/C (SLC 5/05)
- **Firmware:** C/13 - DC 3.54

Verified operations:
| Operation | Status |
|-----------|--------|
| Read Integer (N7:x) | Working |
| Read Status (S2:x) | Working |
| Read Float (F8:x) | Working |
| Read Bit (B3:x/y) | Working |
| Read Timer (T4:x.ACC) | Working |
| Read Counter (C5:x.ACC) | Working |
| Write Integer (N7:x) | Working |
| Write Bit (B3:x/y) | Working |

## License

See LICENSE file for details.

## Acknowledgments

- [pycomm3](https://github.com/ottowayi/pycomm3) - Original Python implementation
- Allen Bradley / Rockwell Automation - PLC hardware and protocols
