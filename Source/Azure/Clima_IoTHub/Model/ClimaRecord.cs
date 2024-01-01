using Meadow.Peripherals.Sensors.Location;
using Meadow.Units;

namespace Clima_IoTHub.Model
{
    public struct ClimaRecord
    {
        public string DateTime { get; internal set; }
        public long Count { get; internal set; }
        public Temperature Temperature { get; internal set; }
        public RelativeHumidity Humidity { get; internal set; }
        public Pressure Pressure { get; internal set; }
        public long TotalMemory { get; internal set; }
        public Voltage BatteryVoltage { get; internal set; }
        public Voltage SolarVoltage { get; internal set; }
        public double WindMetersPerSecond { get; internal set; }
        public double RainMillimeters { get; internal set; }
        public Azimuth16PointCardinalNames WindCompassCardinalName { get; internal set; }
        public double WindCompassDecimalDegrees { get; internal set; }
        public double SCD40PartsPerMillion { get; internal set; }
        public double SCD40Temperature { get; internal set; }
        public double SCD40Humidity { get; internal set; }
        public DegreesMinutesSecondsPosition Longitude { get; internal set; }
        public DegreesMinutesSecondsPosition Latitude { get; internal set; }
    }
}
