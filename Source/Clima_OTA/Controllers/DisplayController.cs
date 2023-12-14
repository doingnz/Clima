using Clima_OTA.Model;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Foundation.Sensors.Gnss;
using Meadow.Foundation.Sensors.Weather;
using Meadow.Hardware;
using System;
using System.Text;

namespace Clima_OTA.Controllers
{
    public class DisplayController : IDisplayController
    {
        private RgbPwmLed rgbPwmLed;

        public DisplayController(RgbPwmLed rgbPwmLed)
        {
            this.rgbPwmLed = rgbPwmLed;
        }

        public void ShowDataScreen()
        {
            Resolver.Log.Info("ShowDataScreen ...");
            rgbPwmLed.StartBlink(Color.Orange);
        }

        public void ShowSplashScreen()
        {
            Resolver.Log.Info("ShowSplashScreen ...");
            rgbPwmLed.StartBlink(Color.Red);
        }

        public void UpdateClimaResults(ClimaRecord clima)
        {
            Resolver.Log.Info($"Temperature    : {clima.Temperature.Celsius:0.0}C, Humidity: {clima.Humidity.Percent:0.#}%, Pressure: {clima.Pressure.Millibar:0.#}mbar");
            Resolver.Log.Info($"Memory         : {clima.Memory}");
            Resolver.Log.Info($"Solar Voltage  : {clima.SolarVoltage.Volts:0.#} volts");
            Resolver.Log.Info($"Battery Voltage: {clima.BatteryVoltage.Volts:0.#} volts");
        }
        
        public void UpdateLastUpdated(string status)
        {
            Resolver.Log.Info($"UpdateLastUpdated: {status}");
        }

        public void UpdateStatus(string status)
        {
            Resolver.Log.Info($"UpdateStatus     : {status}");
        }

        public void UpdateSyncStatus(bool isSyncConnected)
        {
            if (isSyncConnected)
            {
                Resolver.Log.Info("UpdateSyncStatus  : network.IsConnected && iotHubService.isAuthenticated");
                rgbPwmLed.StartBlink(Color.Green);
            }
            else
            {
                Resolver.Log.Info("UpdateSyncStatus  :Not connected! ");
                rgbPwmLed.StartBlink(Color.Purple);
            }
        }

        public void UpdateWiFiStatus(bool isConnected)
        {
            throw new NotImplementedException();
        }
    }
}
