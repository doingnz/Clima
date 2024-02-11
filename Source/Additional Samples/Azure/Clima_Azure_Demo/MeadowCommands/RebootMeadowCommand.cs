using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Cloud;

namespace Clima_Azure_Demo.MeadowCommands
{
    public class RebootMeadowCommand : IMeadowCommand
    {
        public int Seconds { get; set; }

        const int DelayMilliseconds = 1000;

        private static readonly RebootMeadowCommand Instance = new RebootMeadowCommand();


        public static void Initialise()
        {
            Resolver.CommandService?.Subscribe<RebootMeadowCommand>(Instance.HandleCommand);
        }

        private void HandleCommand(RebootMeadowCommand command)
        {
            Resolver.Log.Info($"Received RebootMeadowCommand command with countdown: {command.Seconds}");

            while (command.Seconds-- > 0)
            {
                Resolver.Log.Info($"Remaining {command.Seconds}s...");
                Thread.Sleep(DelayMilliseconds); // use Thread.Sleep() to stop task switching?
            }

            Resolver.Device.PlatformOS.Reset();
        }
    }
   
}
