namespace Sensor;

interface IBatterySensor
{
    double GetVoltage();
    double GetPercentage();
    bool IsLowVoltage();
}
