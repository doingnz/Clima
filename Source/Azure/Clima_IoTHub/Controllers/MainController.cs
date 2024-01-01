using Clima_OTA.Services;
using Meadow;
using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Clima_IoTHub.Model;

namespace Clima_OTA.Controllers
{
    internal class MainController
    {
        readonly int TIMEZONE_OFFSET = +13; // UTC+12 (+1 for DST)

        readonly IMeadowAzureIoTHubHardware hardware;
        readonly IWiFiNetworkAdapter network;
        IDisplayController displayController;
        readonly IStorageService storageService;

        public MainController(IMeadowAzureIoTHubHardware hardware, IWiFiNetworkAdapter network, IStorageService storageService)
        {
            this.hardware = hardware;
            this.network = network;
            this.storageService = storageService;
        }

        public async Task Initialize()
        {
            hardware.Initialize();

            displayController = new DisplayController(hardware.RgbPwmLed);
            displayController.ShowSplashScreen();
            Thread.Sleep(3000);
            displayController.ShowDataScreen();

            await InitializeIoTHub();

            hardware.Updated += EnvironmentalSensorUpdated;
        }

  
        private async Task InitializeIoTHub()
        {
            while (!network.IsConnected || !storageService.isAuthenticated)
            {
                displayController.UpdateStatus("Authenticating...");

                bool authenticated = await storageService.Initialize();

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

        private async Task SendDataToStorageService(ClimaRecord data)
        {
            if (network.IsConnected && storageService.isAuthenticated)
            {
                var st = new Stopwatch();
                st.Start();
                //displayController.UpdateSyncStatus(true);
                displayController.UpdateStatus("Sending data...");

                await storageService.SendEnvironmentalReading(data);

                //displayController.UpdateSyncStatus(false);
                displayController.UpdateStatus("Data sent!");
                st.Stop();
                Resolver.Log.Info($"SendDataToStorageService: {st.ElapsedMilliseconds} ms");

                //Thread.Sleep(2000);
                displayController.UpdateLastUpdated(DateTime.Now.AddHours(TIMEZONE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss"));

                //displayController.UpdateStatus(DateTime.Now.AddHours(TIMEZONE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        private async void EnvironmentalSensorUpdated(object sender, Meadow.IChangeResult<ClimaRecord> e)
        {
            _ = hardware.RgbPwmLed.StartBlink(Color.Blue);

            await SendDataToStorageService(e.New);

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

                    _ = hardware.RgbPwmLed.StartBlink(Color.Red);

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}