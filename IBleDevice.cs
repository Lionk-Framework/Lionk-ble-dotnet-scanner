namespace Ble;

interface ICharacteristicNotificationData
{
    string GetUuid();
    byte[] GetValue();
}

interface IOnCharacteristicData
{
    Task OnNewData(ICharacteristicNotificationData data);
}

interface IBleDevice
{
    public Task<bool> Connect();
    public string? GetName();
    public Task<bool> IsConnected();

    public Task SubscribeToCharacteristic(
        string serviceId,
        string characteristicId,
        IOnCharacteristicData cb
    );
    public Task<byte[]> ReadCharacteristic(string serviceId, string characteristicId);
}
