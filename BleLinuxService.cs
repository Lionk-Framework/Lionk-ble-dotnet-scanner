using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;

namespace Ble;

class BleLinuxService : IBleService
{
    private Adapter adapter;
    private List<IDeviceRegister> subscribers;

    private BleLinuxService(Adapter adapter)
    {
        this.adapter = adapter;
        this.subscribers = new();
    }

    public static async Task<BleLinuxService> CreateAsync()
    {
        var adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();
        if (adapter == null)
        {
            throw new Exception("Failed to find a valid adapter");
        }

        return new BleLinuxService(adapter);
    }

    async Task<string> GetDeviceDescriptionAsync(IDevice1 device)
    {
        var deviceProperties = await device.GetAllAsync();
        return $"{deviceProperties.Alias} (Address: {deviceProperties.Address}, RSSI: {deviceProperties.RSSI})";
    }

    async Task<IEnumerable<IBleDevice>> IBleService.GetAvailableDevices()
    {
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
            await Task.Delay(TimeSpan.FromSeconds(5));
            await adapter.StopDiscoveryAsync();
        }
        /*var devices = await adapter.GetDevicesAsync();*/
        var result = new List<BleLinuxDevice>();
        /*return $"{deviceProperties.Alias} (Address: {deviceProperties.Address}, RSSI: {deviceProperties.RSSI})";*/

        await adapter.StartDiscoveryAsync();
        IReadOnlyList<Device> devices = await adapter.GetDevicesAsync();

        foreach (var dev in devices)
        {
            var deviceProperties = await dev.GetAllAsync();
            string deviceDescription = await GetDeviceDescriptionAsync(dev);
            Console.WriteLine($" - {deviceDescription}");
            result.Add(new(dev, deviceProperties));
        }

        return result;
    }

    public void RegisterForNewDevices(IDeviceRegister register)
    {
        this.subscribers.Add(register);
    }

    public async void RegisterDevice(IBleDevice device)
    {
        var deviceName = device.GetName();
        Console.WriteLine($"Registring device {deviceName}");
        bool registered = false;
        foreach (var sub in subscribers)
        {
            bool canRegister = await sub.CanRegister(device);
            if (canRegister)
            {
                bool ok = await sub.Register(device);
                if (!ok)
                {
                    Console.WriteLine($"Failed to register device {deviceName}");
                    continue;
                }
                registered = true;
            }
        }
        if (!registered)
        {
            Console.WriteLine($"Couldn't find a possible target to register device {deviceName}");
        }
    }
}
