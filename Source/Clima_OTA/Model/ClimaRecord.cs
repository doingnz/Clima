using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clima_OTA.Model
{

    public struct ClimaRecord
    {
        public ClimaRecord(ClimaRecord old)
        {
            this.Temperature = old.Temperature;
            this.Humidity = old.Humidity;
            this.Pressure = old.Pressure;
            this.Memory = old.Memory;  
            this.BatteryVoltage = old.BatteryVoltage;
            this.SolarVoltage = old.SolarVoltage;
        }

        public Temperature Temperature { get; set; }
        public RelativeHumidity Humidity { get; set; }
        public Pressure Pressure { get; set; }
        public long Memory { get; set; }
        public Voltage BatteryVoltage { get; set; }
        public Voltage SolarVoltage { get; set; }
    }
}
