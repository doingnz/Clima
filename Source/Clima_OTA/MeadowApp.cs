using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Peripherals.Sensors.Location.Gnss;
using Meadow.Units;
using Meadow.Update;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Clima_Demo
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IClimaHardware clima;

        public override Task Initialize()
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info("Initialize hardware...");

            clima = Clima.Create();
            clima.ColorLed.SetColor(Color.Red);

            Resolver.Log.Info($"Running on Clima Hardware {clima.RevisionString}");

            if (clima.AtmosphericSensor is { } bme688)
            {
                bme688.Updated += Bme688Updated;
            }

            if (clima.EnvironmentalSensor is { } scd40)
            {
                scd40.Updated += Scd40Updated;
            }

            if (clima.WindVane is { } windVane)
            {
                windVane.Updated += WindvaneUpdated;
            }

            if (clima.RainGauge is { } rainGuage)
            {
                rainGuage.Updated += RainGuageUpdated;
            }

            if (clima.Anemometer is { } anemometer)
            {
                anemometer.WindSpeedUpdated += AnemometerUpdated;
            }

            if (clima.SolarVoltageInput is { } solarVoltage)
            {
                solarVoltage.Updated += SolarVoltageUpdated;
            }

            if (clima.BatteryVoltageInput is { } batteryVoltage)
            {
                batteryVoltage.Updated += BatteryVoltageUpdated;
            }

            if (clima.Gnss is { } gnss)
            {
                //gnss.GsaReceived += GnssGsaReceived;
                //gnss.GsvReceived += GnssGsvReceived;
                //gnss.VtgReceived += GnssVtgReceived;
                gnss.RmcReceived += GnssRmcReceived;
                gnss.GllReceived += GnssGllReceived;
            }

            Resolver.Log.Info("Initialization complete");

            Resolver.Log.Trace(line);
            Resolver.Log.Trace("Add Event Handlers for NTP and WIFI");

            //---- set LED green
            clima.ColorLed.SetColor(Color.Purple);

            ntpClient = Resolver.Services.Get<NtpClient>();
            ntpClient.TimeChanged += NtpClient_TimeChanged;

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += Wifi_NetworkConnected;

            Task.Run(async () =>
            {
                while (true)
                {
                    Resolver.Log.Info("Start Wifi Connection to get Connected Event!");
                    await wifi.Connect("ASUS_10_2G", "Lunatic16042021!", TimeSpan.FromSeconds(45));
                }
            });
            

            return base.Initialize();
        }

        public static string TimeStamp()
        {
            return $"{System.DateTime.Now:yyyy-MM-ddTHH:mm:ss}";
        }

        public static string LogMemory(string function, string commentCount = "", int count = 0, string comment = "")
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            string msg = $"LogMemory, {TimeStamp(),25},{GC.GetTotalMemory(true),12},\"{function,-30}\",\"{(string.IsNullOrWhiteSpace(commentCount) ? "" : commentCount)}\",{(string.IsNullOrWhiteSpace(commentCount) ? "" : count.ToString())},\"{(string.IsNullOrWhiteSpace(comment) ? "" : comment)}\"";
            //Resolver.Log.Info(msg);
            return msg;
        }

        //const string udpIP = "192.168.2.194";
        //private const int udpPort = 5100;
        private IWiFiNetworkAdapter wifi = null;
        private NtpClient ntpClient;

        private void NtpClient_TimeChanged(DateTime utcTime)
        {
            Resolver.Log.Info($"TimeChanged (UTC): {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
            clima.ColorLed.SetColor(Color.GreenYellow);
        }

        private void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"IP Address is {wifi.IpAddress}");
            Resolver.Log.Info("\n\nAdding UDP Logging ...");
            Resolver.Log.AddProvider(new UdpLogger());

            StartUpdatingWeather();
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

        readonly string line = new string('-', 50);

        public override async Task Run()
        {
            Resolver.Log.Info(line);

            Resolver.Log.Info("Setup OTA Service ...");

            var svc = Resolver.Services.Get<IUpdateService>() as Meadow.Update.UpdateService;

            while (svc.State != UpdateState.Dead && svc.State != UpdateState.Idle)
            {
                Resolver.Log.Trace($"svc.State = {svc.State}");
                await Task.Delay(1000);
            }

            // Uncomment to clear any persisted update info. This allows installing the same update multiple times, such as you might do during development.
            svc.ClearUpdates();

            svc.OnUpdateAvailable += (updateService, info) =>
            {
                if (info?.Metadata == string.Empty)
                {
                    Resolver.Log.Info("Update available!");
                }
                else
                {
                    Resolver.Log.Info($"Update available! meta: {info?.Metadata}");
                }

                // Queue update for retrieval "later"
                Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    updateService.RetrieveUpdate(info);
                });
            };

            svc.OnUpdateRetrieved += (updateService, info) =>
            {
                Resolver.Log.Info("Update retrieved!");

                Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    updateService.ApplyUpdate(info);
                });
            };

            Resolver.Log.Info(line);

            Resolver.Log.Info("Run...");

            StartUpdatingVoltages();

            await base.Run();
        }

        private void StartUpdatingWeather()
        {
            Resolver.Log.Info("Start Updating Weather ...");

            var updateInterval = TimeSpan.FromSeconds(5);

            if (clima.AtmosphericSensor is { } bme688)
            {
                bme688.StartUpdating(updateInterval);
            }

            if (clima.EnvironmentalSensor is { } scd40)
            {
                scd40.StartUpdating(updateInterval);
            }

            if (clima.WindVane is { } windVane)
            {
                windVane.StartUpdating(updateInterval);
            }

            if (clima.RainGauge is { } rainGuage)
            {
                rainGuage.StartUpdating(updateInterval);
            }

            if (clima.Anemometer is { } anemometer)
            {
                anemometer.StartUpdating(updateInterval);
            }

            if (clima.Gnss is { } gnss)
            {
                gnss.StartUpdating();
            }
        }

        private void StartUpdatingVoltages()
        {
            Resolver.Log.Info("Start Updating Voltages ...");

            var updateInterval = TimeSpan.FromSeconds(5);

            if (clima.SolarVoltageInput is { } solarVoltage)
            {
                solarVoltage.StartUpdating(updateInterval);
            }

            if (clima.BatteryVoltageInput is { } batteryVoltage)
            {
                batteryVoltage.StartUpdating(updateInterval);
            }
        }

        private void Bme688Updated(object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> e)
        {
            Resolver.Log.Info($"BME688         : {(int)e.New.Temperature?.Celsius:0.0}C, {(int)e.New.Humidity?.Percent:0.#}%, {(int)e.New.Pressure?.Millibar:0.#}mbar");
        }

        public int Counter { get; set; }

        private void SolarVoltageUpdated(object sender, IChangeResult<Voltage> e)
        {
            Resolver.Log.Info($"Solar Voltage  : {e.New.Volts:0.#} volts");

            Counter++;
            Resolver.Log.Trace($"Counter        : {Counter}");
            Resolver.Log.Trace($"GetTotalMemory : {GC.GetTotalMemory(true)}");
            Resolver.Log.Trace($"Time           : {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
        }

        private void BatteryVoltageUpdated(object sender, IChangeResult<Voltage> e)
        {
            Resolver.Log.Info($"Battery Voltage: {e.New.Volts:0.#} volts");
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
    }
}