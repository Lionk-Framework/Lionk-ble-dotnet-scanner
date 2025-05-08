using Ble;
using SensorHandler;

IBleService bleService = await BleLinuxService.CreateAsync();

var tempHandler = new LionkBleTempHandler(bleService);

var devices = await bleService.GetAvailableDevices();

foreach (var dev in devices)
{
    bleService.RegisterDevice(dev);
}

await Task.Delay(Timeout.Infinite);
/*
using System;
using System.Linq;
using System.Threading.Tasks;
using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;

async Task<string> GetDeviceDescriptionAsync(IDevice1 device)
{
    var deviceProperties = await device.GetAllAsync();
    return $"{deviceProperties.Alias} (Address: {deviceProperties.Address}, RSSI: {deviceProperties.RSSI})";
}
Adapter? adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();

if (adapter is null)
{
    Console.WriteLine("Couldn't find a valid bluetooth adapter");
    return;
}
int scanSeconds = 2;

var adapterPath = adapter.ObjectPath.ToString();
var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/") + 1);
Console.WriteLine($"Using Bluetooth adapter {adapterName}");
Console.WriteLine($"Adapter's full path:    {adapterPath}");

// Print out the devices we already know about.
Console.WriteLine();
Console.WriteLine("Getting known devices...");

var devices = await adapter.GetDevicesAsync();

foreach (var device in devices)
{
    string deviceDescription = await GetDeviceDescriptionAsync(device);
    Console.WriteLine($" - {deviceDescription}");
}

Console.WriteLine($"Found {devices.Count} paired device(s).");
Console.WriteLine();

// Scan for more devices.
Console.WriteLine($"Scanning for {scanSeconds} seconds...");

int newDevices = 0;
using (
    await adapter.WatchDevicesAddedAsync(async device =>
    {
        newDevices++;
        // Write a message when we detect new devices during the scan.
        string deviceDescription = await GetDeviceDescriptionAsync(device);
        Console.WriteLine($"[NEW] {deviceDescription}");
    })
)
{
    await adapter.StartDiscoveryAsync();
    await Task.Delay(TimeSpan.FromSeconds(scanSeconds));
    await adapter.StopDiscoveryAsync();
}

Console.WriteLine($"Scan complete. {newDevices} new device(s) found.");
/*
await adapter.StartDiscoveryAsync();
IReadOnlyList<Device> devices = await adapter.GetDevicesAsync();

List<string> names = new();

foreach (var dev in devices)
{
names.Add(await dev.GetNameAsync());
Console.WriteLine($"{names[names.Count - 1]}");
}
*/
