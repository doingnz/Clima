using Clima_OTA.Model;
using Meadow;
using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Clima_OTA.Controllers
{
    internal class MainController
    {
        int TIMEZONE_OFFSET = +13; // UTC+12 (+1 for DST)

        IMeadowAzureIoTHubHardware hardware;
        IWiFiNetworkAdapter network;
        IDisplayController displayController;
        IoTHubController iotHubService;

        public MainController(IMeadowAzureIoTHubHardware hardware, IWiFiNetworkAdapter network)
        {
            this.hardware = hardware;
            this.network = network;
        }

        public async Task Initialize()
        {
            hardware.Initialize();

            displayController = new DisplayController(hardware.RgbPwmLed);
            displayController.ShowSplashScreen();
            Thread.Sleep(3000);
            displayController.ShowDataScreen();

            iotHubService = new IoTHubController();
            await InitializeIoTHub();

            hardware.Updated += EnvironmentalSensorUpdated;
        }

  
        private async Task InitializeIoTHub()
        {
            while (!network.IsConnected || !iotHubService.isAuthenticated)
            {
                displayController.UpdateStatus("Authenticating...");

                bool authenticated = await iotHubService.Initialize();

                if (authenticated)
                {
                    displayController.UpdateStatus($"Authenticated {DateTime.Now.AddHours(TIMEZONE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss")}");
                }
                else
                {
                    displayController.UpdateStatus("Not Authenticated");
                }
            }
        }

        private async Task SendDataToIoTHub(ClimaRecord data)
        {
            if (network.IsConnected && iotHubService.isAuthenticated)
            {
                //displayController.UpdateSyncStatus(true);
                displayController.UpdateStatus("Sending data...");

                await iotHubService.SendEnvironmentalReading(data);

                //displayController.UpdateSyncStatus(false);
                displayController.UpdateStatus("Data sent!");
                //Thread.Sleep(2000);
                displayController.UpdateLastUpdated(DateTime.Now.AddHours(TIMEZONE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss"));

                //displayController.UpdateStatus(DateTime.Now.AddHours(TIMEZONE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        private void EnvironmentalSensorUpdated(object sender, ClimaRecord e)
        {
            throw new NotImplementedException();
        }
        private async void EnvironmentalSensorUpdated(object sender, Meadow.IChangeResult<ClimaRecord> e)
        {
            _ = hardware.RgbPwmLed.StartBlink(Color.Orange);

            await SendDataToIoTHub(e.New);

            _ = hardware.RgbPwmLed.StartBlink(Color.Green);
        }

        public async Task Run()
        {
            hardware.StartUpdatingVoltages();
            hardware.StartUpdatingWeather();

            while (true)
            {
                //displayController.UpdateWiFiStatus(network.IsConnected);

                if (network.IsConnected)
                {
                    displayController.UpdateStatus($"UpdateWiFiStatus : IsConnected {DateTime.Now.AddHours(TIMEZONE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss")}");

                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                else
                {
                    displayController.UpdateStatus("Offline...");

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}