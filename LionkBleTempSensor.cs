using System.Text;
using Ble;

namespace Sensor;

class LionkBleTempSensor : ITemperatureSensor, IBatterySensor, IOnCharacteristicData
{
    private IBleDevice device;

    private static string VersionServiceID = "00000005-7669-6163-616d-2d63616c6563";
    private static string VersionCharacteristicID = "00000006-7669-6163-616d-2d63616c6563";
    private static string DataServiceID = "00000007-7669-6163-616d-2d63616c6563";
    private static string DataCharacteristicID = "00000008-7669-6163-616d-2d63616c6563";

    private DateTime LastTimestamp;
    private double Temperature;
    private double Voltage;

    private LionkBleTempSensor(IBleDevice device)
    {
        this.device = device;
        this.LastTimestamp = DateTime.MinValue;
    }

    public static async Task<LionkBleTempSensor> CreateAsync(IBleDevice device)
    {
        bool connected = await device.Connect();
        if (!connected)
        {
            throw new Exception("Failed to Connect to device");
        }
        LionkBleTempSensor sensor = new LionkBleTempSensor(device);
        var deviceVersion = Encoding.UTF8.GetString(
            await device.ReadCharacteristic(VersionServiceID, VersionCharacteristicID)
        );
        Console.WriteLine(
            $"Connected to Lionk Ble Temperature Sensor {device.GetName()} - v{deviceVersion}"
        );
        await device.SubscribeToCharacteristic(DataServiceID, DataCharacteristicID, sensor);
        Console.WriteLine($"Subscribed to Lionk Ble Temperature Sensor {device.GetName()}");
        return sensor;
    }

    public double GetPercentage()
    {
        return Voltage - 3.5 / (4.2 - 3.5) * 100;
    }

    public double GetTemperature()
    {
        return Temperature;
    }

    public double GetVoltage()
    {
        return Voltage;
    }

    public bool IsLowVoltage()
    {
        return Voltage <= 3.6;
    }

    public Task OnNewData(ICharacteristicNotificationData data)
    {
        Console.WriteLine("Got new data!");
        byte[] payload = data.GetValue();
        byte payloadVersion = payload[0];
        int rawTemperature = (payload[1] << 8) | payload[2];
        int rawVoltage = (payload[3] << 8) | payload[4];
        Temperature = rawTemperature / 10.0;
        Voltage = rawVoltage / 1000.0;
        Console.WriteLine($"Got new data! {payloadVersion} {Temperature}°C {Voltage}V");
        return Task.CompletedTask;
    }
}
