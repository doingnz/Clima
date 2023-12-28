using Clima_OTA.Controllers;
using Clima_OTA.Services;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Update;
using System;
using System.Threading.Tasks;

namespace Clima_OTA
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        MainController mainController;

        //const string udpIP = "192.168.2.194";
        //private const int udpPort = 5100;
        private IWiFiNetworkAdapter wifi = null;
        private NtpClient ntpClient;

        readonly string line = new string('-', 50);

        public override async Task Initialize()
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info("Initialize MeadowApp ...");

            var hardware = new MeadowAzureIoTHubHardware();

            Resolver.Log.Trace("Add Event Handlers for NTP and WIFI");

            ntpClient = Resolver.Services.Get<NtpClient>();
            ntpClient.TimeChanged += NtpClient_TimeChanged;

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += Wifi_NetworkConnected;

            Resolver.Log.Info("Start Wifi Connection to get Connected Event!");
            await wifi.Connect("ASUS_10_2G", "Lunatic16042021!", TimeSpan.FromSeconds(45));

            IStorageService storageService = new IoTHubController();

            mainController = new MainController(hardware, wifi, storageService);

            await mainController.Initialize();
        }

        public static string TimeStamp()
        {
            return $"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}";
        }

        public static string LogMemory(string function, string commentCount = "", int count = 0, string comment = "")
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            string msg = $"LogMemory, {TimeStamp(),25},{GC.GetTotalMemory(true),12},\"{function,-30}\",\"{(string.IsNullOrWhiteSpace(commentCount) ? "" : commentCount)}\",{(string.IsNullOrWhiteSpace(commentCount) ? "" : count.ToString())},\"{(string.IsNullOrWhiteSpace(comment) ? "" : comment)}\"";
            //Resolver.Log.Info(msg);
            return msg;
        }

        private void NtpClient_TimeChanged(DateTime utcTime)
        {
            Resolver.Log.Info($"TimeChanged (UTC): {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
        }

        private void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"IP Address is {wifi.IpAddress}");
            Resolver.Log.Info("\n\nAdding UDP Logging ...");
            Resolver.Log.AddProvider(new UdpLogger());
        }


        public override async Task Run()
        {
            Resolver.Log.Info(line);

            Resolver.Log.Info("Setup OTA Service ...");

            var svc = Resolver.Services.Get<IUpdateService>() as UpdateService;

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

            await mainController.Run();

            //await base.Run();
        }
    }
}