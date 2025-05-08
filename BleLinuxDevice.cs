using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;

namespace Ble;

class CharacteristicNotificationData : ICharacteristicNotificationData
{
    private string uuid;
    private byte[] data;

    public CharacteristicNotificationData(string uuid, byte[] data)
    {
        this.uuid = uuid;
        this.data = data;
    }

    public string GetUuid()
    {
        return uuid;
    }

    public byte[] GetValue()
    {
        return data;
    }
}

class BleLinuxDevice : IBleDevice
{
    public Device device;
    private Device1Properties props;
    static Dictionary<string, IOnCharacteristicData> subscribers = new();

    public BleLinuxDevice(Device device, Device1Properties props)
    {
        this.device = device;
        this.props = props;
    }

    string? IBleDevice.GetName()
    {
        return props.Name;
    }

    private static async Task OnNotification(
        GattCharacteristic characteristic,
        GattCharacteristicValueEventArgs e
    )
    {
        var uuid = await characteristic.GetUUIDAsync();
        var data = e.Value;
        if (subscribers.TryGetValue(uuid, out var subscriber))
        {
            var notificationData = new CharacteristicNotificationData(uuid, data);
            await subscriber.OnNewData(notificationData);
        }
        else
        {
            Console.WriteLine("Couldn't find characteristic subscriber");
        }
    }

    public async Task SubscribeToCharacteristic(
        string serviceId,
        string characteristicId,
        IOnCharacteristicData onData
    )
    {
        IGattService1 service = await device.GetServiceAsync(serviceId);
        if (service == null)
        {
            Console.WriteLine($"Couldn't read service {serviceId}");
            return;
        }
        GattCharacteristic characteristic = await service.GetCharacteristicAsync(characteristicId);

        if (characteristic == null)
        {
            Console.WriteLine($"Couldn't read characteristic {characteristicId}");
            return;
        }
        await characteristic.StartNotifyAsync();
        characteristic.Value += OnNotification;
        subscribers.Add(await characteristic.GetUUIDAsync(), onData);
    }

    public async Task<byte[]> ReadCharacteristic(string serviceId, string characteristicId)
    {
        IGattService1 service = await device.GetServiceAsync(serviceId);
        if (service == null)
        {
            Console.WriteLine($"Couldn't read service {serviceId}");
            return [];
        }
        GattCharacteristic characteristic = await service.GetCharacteristicAsync(characteristicId);
        if (characteristic == null)
        {
            Console.WriteLine($"Couldn't read characteristic {characteristicId}");
            return [];
        }

        return await characteristic.ReadValueAsync(TimeSpan.FromSeconds(15));
    }

    async Task<bool> IBleDevice.IsConnected()
    {
        return await device.GetConnectedAsync();
    }

    async Task<bool> IBleDevice.Connect()
    {
        device.Connected += ConnectedAsync;
        device.Disconnected += DisconnectedAsync;
        device.ServicesResolved += ServicesResolvedAsync;

        await device.ConnectAsync();
        await device.WaitForPropertyValueAsync("Connected", value: true, TimeSpan.FromSeconds(15));
        await device.WaitForPropertyValueAsync(
            "ServicesResolved",
            value: true,
            TimeSpan.FromSeconds(15)
        );
        return true;
    }

    static Task ConnectedAsync(Device sender, BlueZEventArgs eventArgs)
    {
        return Task.CompletedTask;
    }

    static Task DisconnectedAsync(Device sender, BlueZEventArgs eventArgs)
    {
        return Task.CompletedTask;
    }

    static Task ServicesResolvedAsync(Device sender, BlueZEventArgs eventArgs)
    {
        return Task.CompletedTask;
    }
}
