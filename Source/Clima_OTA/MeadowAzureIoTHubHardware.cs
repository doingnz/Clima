using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;
using System.Security.Claims;
using System;
using Meadow.Foundation;
using Meadow.Peripherals.Sensors.Location.Gnss;
using Clima_OTA.Model;

namespace Clima_OTA
{
    internal class MeadowAzureIoTHubHardware : IMeadowAzureIoTHubHardware
    {
        protected IClimaHardware Clima { get; private set; }

        ClimaRecord Old;

        public event EventHandler<Meadow.IChangeResult<ClimaRecord>> Updated = default!;

        protected virtual void OnUpdated(ClimaRecord NewClimaRecord)
        {
            NewClimaRecord.Memory = GC.GetTotalMemory(true);
            
            Counter++;
            Resolver.Log.Trace($"Counter        : {Counter}");
            Resolver.Log.Trace($"Time           : {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
            Resolver.Log.Trace($"GetTotalMemory : {NewClimaRecord.Memory}");

            Meadow.IChangeResult<ClimaRecord> e = new Meadow.ChangeResult<ClimaRecord>(NewClimaRecord, Old);
            EventHandler<Meadow.IChangeResult<ClimaRecord>> handler = Updated;
            if (handler != null)
            {
                handler(this, e);
            }
            Old = NewClimaRecord;
        }

        public RgbPwmLed RgbPwmLed { get; set; }

        readonly string line = new string('#', 50);

        public void Initialize()
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info("Initialize MeadowAzureIoTHubHardware ...");

            Clima = Meadow.Devices.Clima.Create();
            Clima.ColorLed.SetColor(Color.Red);

            Resolver.Log.Info($"Running on Clima Hardware {Clima.RevisionString}");

            if (Clima.AtmosphericSensor is { } bme688)
            {
                bme688.Updated += Bme688Updated;
            }

            if (Clima.EnvironmentalSensor is { } scd40)
            {
                scd40.Updated += Scd40Updated;
            }

            if (Clima.WindVane is { } windVane)
            {
                windVane.Updated += WindvaneUpdated;
            }

            if (Clima.RainGauge is { } rainGuage)
            {
                rainGuage.Updated += RainGuageUpdated;
            }

            if (Clima.Anemometer is { } anemometer)
            {
                anemometer.Updated += AnemometerUpdated;
            }

            if (Clima.SolarVoltageInput is { } solarVoltage)
            {
                solarVoltage.Updated += SolarVoltageUpdated;
            }

            if (Clima.BatteryVoltageInput is { } batteryVoltage)
            {
                batteryVoltage.Updated += BatteryVoltageUpdated;
            }

            if (Clima.Gnss is { } gnss)
            {
                //gnss.GsaReceived += GnssGsaReceived;
                //gnss.GsvReceived += GnssGsvReceived;
                //gnss.VtgReceived += GnssVtgReceived;
                gnss.RmcReceived += GnssRmcReceived;
                gnss.GllReceived += GnssGllReceived;
            }

            Resolver.Log.Info("Initialization complete");

            //Display = ProjLab.Display;

            RgbPwmLed = Clima.ColorLed;
        }


        public void StartUpdatingWeather()
        {
            Resolver.Log.Info("Start Updating Weather ...");

            var updateInterval = TimeSpan.FromSeconds(10);

            if (Clima.AtmosphericSensor is { } bme688)
            {
                bme688.StartUpdating(updateInterval);
            }

            if (Clima.EnvironmentalSensor is { } scd40)
            {
                scd40.StartUpdating(updateInterval);
            }

            if (Clima.WindVane is { } windVane)
            {
                windVane.StartUpdating(updateInterval);
            }

            if (Clima.RainGauge is { } rainGuage)
            {
                rainGuage.StartUpdating(updateInterval);
            }

            if (Clima.Anemometer is { } anemometer)
            {
                anemometer.StartUpdating(updateInterval);
            }

            if (Clima.Gnss is { } gnss)
            {
                gnss.StartUpdating();
            }
        }

        public void StartUpdatingVoltages()
        {
            Resolver.Log.Info("Start Updating Voltages ...");

            var updateInterval = TimeSpan.FromSeconds(10);

            if (Clima.SolarVoltageInput is { } solarVoltage)
            {
                solarVoltage.StartUpdating(updateInterval);
            }

            if (Clima.BatteryVoltageInput is { } batteryVoltage)
            {
                batteryVoltage.StartUpdating(updateInterval);
            }
        }

        private void Bme688Updated(object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> e)
        {
            Resolver.Log.Info($"BME688         : {(int)e.New.Temperature?.Celsius:0.0}C, {(int)e.New.Humidity?.Percent:0.#}%, {(int)e.New.Pressure?.Millibar:0.#}mbar");

            ClimaRecord New = new ClimaRecord(Old);
            New.Temperature = (Temperature)e.New.Temperature;
            New.Humidity = (RelativeHumidity)e.New.Humidity;
            New.Pressure = (Pressure)e.New.Pressure;
            OnUpdated(New);
        }

        public int Counter { get; set; }

        private void SolarVoltageUpdated(object sender, IChangeResult<Voltage> e)
        {
            Resolver.Log.Info($"Solar Voltage  : {e.New.Volts:0.#} volts");
            ClimaRecord New = new ClimaRecord(Old);
            New = new ClimaRecord(Old);
            New.SolarVoltage = e.New;
            OnUpdated(New);
        }

        private void BatteryVoltageUpdated(object sender, IChangeResult<Voltage> e)
        {
            Resolver.Log.Info($"Battery Voltage: {e.New.Volts:0.#} volts");
            ClimaRecord New = new ClimaRecord(Old);
            New = new ClimaRecord(Old);
            New.BatteryVoltage = e.New;
            OnUpdated(New);
        }

        private void AnemometerUpdated(object sender, IChangeResult<Speed> e)
        {
            Resolver.Log.Info($"Anemometer     : {e.New.MetersPerSecond:0.#} m/s");
        }

        private void RainGuageUpdated(object sender, IChangeResult<Length> e)
        {
            Resolver.Log.Info($"Rain Gauge     : {e.New.Millimeters:0.#} mm");
        }

        private void WindvaneUpdated(object sender, IChangeResult<Azimuth> e)
        {
            Resolver.Log.Info($"Wind Vane      : {e.New.Compass16PointCardinalName} ({e.New.Radians:0.#} radians)");
        }

        private void Scd40Updated(object sender, IChangeResult<(Concentration? Concentration, Temperature? Temperature, RelativeHumidity? Humidity)> e)
        {
            Resolver.Log.Info($"SCD40          : {e.New.Concentration.Value.PartsPerMillion:0.#}ppm, {e.New.Temperature.Value.Celsius:0.0}C, {e.New.Humidity.Value.Percent:0.0}%");
        }
        private void GnssRmcReceived(object sender, GnssPositionInfo e)
        {
            if (e.Valid)
            {
                Resolver.Log.Info($"GNSS Position: lat: [{e.Position.Latitude}], long: [{e.Position.Longitude}]");
            }
        }

        private void GnssGllReceived(object sender, GnssPositionInfo e)
        {
            if (e.Valid)
            {
                Resolver.Log.Info($"GNSS Position: lat: [{e.Position.Latitude}], long: [{e.Position.Longitude}]");
            }
        }

        private void GnssGsaReceived(object sender, ActiveSatellites e)
        {
            if (e.SatellitesUsedForFix is { } sats)
            {
                Resolver.Log.Info($"Number of active satellites: {sats.Length}");
            }
        }

        private void GnssGsvReceived(object sender, SatellitesInView e)
        {
            Resolver.Log.Info($"Satellites in view: {e.Satellites.Length}");
        }

        private void GnssVtgReceived(object sender, CourseOverGround e)
        {
            if (e is { } cv)
            {
                Resolver.Log.Info($"{cv}");
            };
        }
    }
}