using Meadow.Units;

namespace Clima_IoTHub.Model
{
    public struct ClimaRecord
    {
        public string DateTime { get; set; }
        public long Count { get; set; }
        public Temperature Temperature { get; set; }
        public RelativeHumidity Humidity { get; set; }
        public Pressure Pressure { get; set; }
        public long TotalMemory { get; set; }
        public Voltage BatteryVoltage { get; set; }
        public Voltage SolarVoltage { get; set; }
    }
}
