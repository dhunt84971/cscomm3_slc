// CSComm3.SLC Test Application
// Tests read operations with Allen Bradley SLC/MicroLogix PLC

using CSComm3.SLC;

const string PLC_IP = "192.168.1.150";

Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("CSComm3.SLC Library Test Application");
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine($"Target PLC: {PLC_IP}");
Console.WriteLine();

try
{
    using var plc = new SLCDriver(PLC_IP);
    plc.Timeout = 5000;

    // Test 1: Open Connection
    Console.WriteLine("[TEST 1] Opening connection...");
    var connected = plc.Open();
    Console.WriteLine($"  Result: {(connected ? "SUCCESS" : "FAILED")}");
    Console.WriteLine($"  Connected: {plc.Connected}");
    Console.WriteLine();

    if (!plc.Connected)
    {
        Console.WriteLine("Failed to connect. Exiting.");
        return 1;
    }

    // Test 2: Get Device Identity
    Console.WriteLine("[TEST 2] Getting device identity...");
    try
    {
        var identity = plc.GetIdentity();
        Console.WriteLine($"  Product Name: {identity.ProductName}");
        Console.WriteLine($"  Vendor: {identity.VendorName}");
        Console.WriteLine($"  Revision: {identity.Revision}");
        Console.WriteLine($"  Serial: 0x{identity.SerialNumber:X8}");
        Console.WriteLine("  Result: SUCCESS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 3: Read Integer (N7:0 expected: 4109)
    Console.WriteLine("[TEST 3] Reading Integer N7:0 (expected: 4109)...");
    try
    {
        var tag = plc.Read("N7:0");
        Console.WriteLine($"  {tag.Address} = {tag.Value} ({tag.Value?.GetType().Name})");
        Console.WriteLine($"  Expected: 4109");
        Console.WriteLine($"  Match: {Convert.ToInt16(tag.Value) == 4109}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 4: Read Status (S2:37 expected: 2000)
    Console.WriteLine("[TEST 4] Reading Status S2:37 (expected: 2000)...");
    try
    {
        var tag = plc.Read("S2:37");
        Console.WriteLine($"  {tag.Address} = {tag.Value} ({tag.Value?.GetType().Name})");
        Console.WriteLine($"  Expected: 2000");
        Console.WriteLine($"  Match: {Convert.ToInt16(tag.Value) == 2000}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 5: Read Float (F8:0 expected: 41.09)
    Console.WriteLine("[TEST 5] Reading Float F8:0 (expected: 41.09)...");
    try
    {
        var tag = plc.Read("F8:0");
        Console.WriteLine($"  {tag.Address} = {tag.Value} ({tag.Value?.GetType().Name})");
        Console.WriteLine($"  Expected: 41.09");
        var diff = Math.Abs(Convert.ToSingle(tag.Value) - 41.09f);
        Console.WriteLine($"  Match: {diff < 0.01}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 6: Read Bit
    Console.WriteLine("[TEST 6] Reading Bit B3:0/0...");
    try
    {
        var tag = plc.Read("B3:0/0");
        Console.WriteLine($"  {tag.Address} = {tag.Value} ({tag.Value?.GetType().Name})");
        Console.WriteLine("  Result: SUCCESS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 7: Read Timer
    Console.WriteLine("[TEST 7] Reading Timer T4:0.ACC...");
    try
    {
        var tag = plc.Read("T4:0.ACC");
        Console.WriteLine($"  {tag.Address} = {tag.Value} ({tag.Value?.GetType().Name})");
        Console.WriteLine("  Result: SUCCESS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 8: Read Counter
    Console.WriteLine("[TEST 8] Reading Counter C5:0.ACC...");
    try
    {
        var tag = plc.Read("C5:0.ACC");
        Console.WriteLine($"  {tag.Address} = {tag.Value} ({tag.Value?.GetType().Name})");
        Console.WriteLine("  Result: SUCCESS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 9: Read Multiple Tags
    Console.WriteLine("[TEST 9] Reading multiple tags N7:0-2...");
    try
    {
        var tags = plc.Read("N7:0", "N7:1", "N7:2");
        foreach (var tag in tags)
        {
            Console.WriteLine($"  {tag.Address} = {tag.Value}");
        }
        Console.WriteLine("  Result: SUCCESS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 10: Async Read
    Console.WriteLine("[TEST 10] Async read N7:0...");
    try
    {
        var tag = await plc.ReadAsync("N7:0");
        Console.WriteLine($"  {tag.Address} = {tag.Value}");
        Console.WriteLine("  Result: SUCCESS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Test 11: Write to N7:1
    Console.WriteLine("[TEST 11] Writing to N7:1...");
    try
    {
        // Read current value
        var before = plc.Read("N7:1");
        Console.WriteLine($"  Before: N7:1 = {before.Value}");

        // Write value 10
        Console.WriteLine("  Writing value 10...");
        var result = plc.Write("N7:1", (short)10);
        Console.WriteLine($"  Write returned: {result}");

        // Read back
        var after = plc.Read("N7:1");
        Console.WriteLine($"  After: N7:1 = {after.Value}");
        Console.WriteLine($"  Expected: 10");
        Console.WriteLine($"  Match: {Convert.ToInt16(after.Value) == 10}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.GetType().Name}: {ex.Message}");
        if (ex.InnerException != null)
            Console.WriteLine($"  Inner: {ex.InnerException.Message}");
    }
    Console.WriteLine();

    // Test 12: Close
    Console.WriteLine("[TEST 12] Closing connection...");
    plc.Close();
    Console.WriteLine($"  Connected: {plc.Connected}");
    Console.WriteLine("  Result: SUCCESS");
    Console.WriteLine();

    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine("All tests completed!");
    Console.WriteLine("=".PadRight(60, '='));

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("FATAL ERROR:");
    Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
    Console.WriteLine();
    Console.WriteLine("Stack Trace:");
    Console.WriteLine(ex.StackTrace);
    return 1;
}
