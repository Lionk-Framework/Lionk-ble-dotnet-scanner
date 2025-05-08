namespace Ble;

interface IDeviceRegister
{
    Task<bool> CanRegister(IBleDevice device);
    Task<bool> Register(IBleDevice device);
}

interface IBleService
{
    Task<IEnumerable<IBleDevice>> GetAvailableDevices();
    void RegisterForNewDevices(IDeviceRegister Register);
    void RegisterDevice(IBleDevice device);
}
