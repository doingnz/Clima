using System;
using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Update;
using Clima_Azure_Demo.MeadowCommands;

namespace Clima_Azure_Demo
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += (networkAdapter, networkConnectionEventArgs) =>
            {
                Resolver.Log.Info("Joined network");
                Resolver.Log.Info($"IP Address: {networkAdapter.IpAddress}.");
                Resolver.Log.Info($"Subnet mask: {networkAdapter.SubnetMask}");
                Resolver.Log.Info($"Gateway: {networkAdapter.Gateway}");
            };

            RebootMeadowCommand.Initialise();

            return Task.CompletedTask;
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