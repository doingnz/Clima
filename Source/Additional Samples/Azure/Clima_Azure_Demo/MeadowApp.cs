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
using System.IO;

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
        public override Task OnShutdown()
        {
            Resolver.Log.Info($"OnShutdown called");

            return base.OnShutdown();
        }

        public override Task OnError(Exception e)
        {
            Resolver.Log.Info($"OnError called with {e.Message}");
            return base.OnError(e);
        }
        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

  //          DriveInfo[] allDrives = DriveInfo.GetDrives();AbandonedMutexException 

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
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"Wifi_NetworkError:  ErrorCode={args.ErrorCode}.");
            Resolver.Log.Trace(line);
        }

        private void Wifi_NetworkDisconnected(INetworkAdapter sender, NetworkDisconnectionEventArgs args)
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"Wifi_NetworkDisconnected:  Reason={args.Reason}.");
            Resolver.Log.Trace(line);
        }

        private void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"IP Address is {wifi.IpAddress}");
            //Resolver.Log.Info("\n\nAdding UDP Logging ...");
            //Resolver.Log.AddProvider(new UdpLogger());
            Resolver.Log.Trace(line);
        }

        private void NtpClient_TimeChanged(DateTime utcTime)
        {
            Resolver.Log.Trace(line);
            Resolver.Log.Info($"TimeChanged (UTC): {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
            Resolver.Log.Trace(line);
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            Resolver.Log.Info("Initialise Update Service.");

            //var svc = Resolver.Services.Get<IUpdateService>() as Meadow.Update.UpdateService;
            //// Uncomment to clear any persisted update info. This allows installing the same update multiple times, such as you might do during development.
            //while (svc.State != UpdateState.Dead && svc.State != UpdateState.Connected)
            //{
            //    Resolver.Log.Trace($"svc.State = {svc.State}");
            //    await Task.Delay(1000);
            //}
            //svc.ClearUpdates();

            //svc.OnUpdateAvailable += (updateService, info) =>
            //{
            //    Resolver.Log.Info("Update available!");

            //    // Queue update for retrieval "later"
            //    Task.Run(async () =>
            //    {
            //        await Task.Delay(5000);
            //        updateService.RetrieveUpdate(info);
            //    });
            //};

            //svc.OnUpdateRetrieved += (updateService, info) =>
            //{
            //    Resolver.Log.Info("Update retrieved!");

            //    Task.Run(async () =>
            //    {
            //        await Task.Delay(5000);
            //        updateService.ApplyUpdate(info);
            //    });
            //};

            Resolver.Log.Info("Hello, Meadow Core-Compute!");

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);
                    Resolver.Log.Info($"Time (UTC): {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Resolver.Log.Info($"Run GetTotalMemory= {GC.GetTotalMemory(true)}");
                    Resolver.Log.Info($"IP Address is currently {wifi.IpAddress} & IsConnected={wifi.IsConnected}");
                }
            });

            Resolver.Log.Info("Run Task.CompletedTask()");
            return;// Task.CompletedTask;
        }
    }
}