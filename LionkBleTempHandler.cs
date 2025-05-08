using Ble;
using Sensor;

namespace SensorHandler;

class LionkBleTempHandler : IDeviceRegister
{
    private List<LionkBleTempSensor> sensors;

    public LionkBleTempHandler(IBleService bleService)
    {
        bleService.RegisterForNewDevices(this);
        sensors = new();
    }

    public async Task<bool> Register(IBleDevice device)
    {
        var sensor = await LionkBleTempSensor.CreateAsync(device);
        sensors.Add(sensor);
        return true;
    }

    public List<LionkBleTempSensor> GetSensors()
    {
        return sensors;
    }

    public Task<bool> CanRegister(IBleDevice device)
    {
        var name = device.GetName();
        if (name == null)
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(name.StartsWith("Lionk-Temp"));
    }
}
