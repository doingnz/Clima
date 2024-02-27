using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Update;
using Clima_Azure_Demo.MeadowCommands;
using Meadow.Logging;
using Stateless;

namespace Clima_Azure_Demo
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        private IWiFiNetworkAdapter wifi = null;
        private NtpClient ntpClient;
        readonly string line = new string('-', 50);

        private ClimaWeather climaWeather;

        public MeadowApp()
        {
            climaWeather = new ClimaWeather("Clima");
        }

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // Instantiate a new state machine in the Open state
            climaWeather.Assign("Joe");
            climaWeather.Defer();
            climaWeather.Assign("Harry");
            climaWeather.Assign("Fred");
            climaWeather.Close();

            ntpClient = Resolver.Services.Get<NtpClient>();
            ntpClient.TimeChanged += NtpClient_TimeChanged;

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += Wifi_NetworkConnected;
            wifi.NetworkDisconnected += Wifi_NetworkDisconnected;
            wifi.NetworkError += Wifi_NetworkError;

            wifi.SetAntenna(AntennaType.External, true);
            if (wifi.IsConnected)
            {
                Wifi_NetworkConnected(wifi, new NetworkConnectionEventArgs(wifi.IpAddress, wifi.SubnetMask, wifi.Gateway));
            }

            RebootMeadowCommand.Initialise();
            PingMeadowCommand.Initialise();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Resolver.Log.Info($"Initialize Done: {GC.GetTotalMemory(true)}");

            return Task.CompletedTask;
        }

        private void Wifi_NetworkError(INetworkAdapter sender, NetworkErrorEventArgs args)
        {
            Resolver.Log.Info($"Wifi_NetworkError:  ErrorCode={args.ErrorCode}.");
        }

        private void Wifi_NetworkDisconnected(INetworkAdapter sender, NetworkDisconnectionEventArgs args)
        {
            Resolver.Log.Info($"Wifi_NetworkError:  Reason={args.Reason}.");
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

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Resolver.Log.Info($"Run GetTotalMemory= {GC.GetTotalMemory(true)}");
                }
            });

            Resolver.Log.Info("Run Task.CompletedTask()");
            return Task.CompletedTask;
        }
    }
}