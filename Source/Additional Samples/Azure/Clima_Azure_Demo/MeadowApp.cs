using System;
using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Update;
using Clima_Azure_Demo.MeadowCommands;
using Meadow.Logging;

namespace Clima_Azure_Demo
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        private IWiFiNetworkAdapter wifi = null;
        private NtpClient ntpClient;
        readonly string line = new string('-', 50);

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            ntpClient = Resolver.Services.Get<NtpClient>();
            ntpClient.TimeChanged += NtpClient_TimeChanged;

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += Wifi_NetworkConnected;
            wifi.NetworkDisconnected += Wifi_NetworkDisconnected;
            wifi.NetworkError += Wifi_NetworkError;
            wifi.SetAntenna(AntennaType.External, true);

            RebootMeadowCommand.Initialise();

            return Task.CompletedTask;
        }

        private void Wifi_NetworkError(INetworkAdapter sender, NetworkErrorEventArgs args)
        {
            Resolver.Log.Info($"Wifi_NetworkError:  ErrorCode={args.ErrorCode}.");
        }

        private void Wifi_NetworkDisconnected(INetworkAdapter sender)
        {
            Resolver.Log.Info($"Wifi_NetworkDisconnected!");
        }

        private void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"IP Address is {wifi.IpAddress}");
            Resolver.Log.Info("\n\nAdding UDP Logging ...");
            Resolver.Log.AddProvider(new UdpLogger());
        }

        private void NtpClient_TimeChanged(DateTime utcTime)
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"TimeChanged (UTC): {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
            Resolver.Log.Trace(line);
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            Resolver.Log.Info("Initialise Update Service.");

            var svc = Resolver.Services.Get<IUpdateService>() as Meadow.Update.UpdateService;
            // Uncomment to clear any persisted update info. This allows installing the same update multiple times, such as you might do during development.
            // svc.ClearUpdates();

            svc.OnUpdateAvailable += (updateService, info) =>
            {
                Resolver.Log.Info("Update available!");

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

            Resolver.Log.Info("Hello, Meadow Core-Compute!");

            return Task.CompletedTask;
        }
    }
}