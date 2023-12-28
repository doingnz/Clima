using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clima_OTA.Model
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
