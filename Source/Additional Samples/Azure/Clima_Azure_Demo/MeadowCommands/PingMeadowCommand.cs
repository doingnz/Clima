using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Cloud;

namespace Clima_Azure_Demo.MeadowCommands
{
    public class PingMeadowCommand : IMeadowCommand
    {
        public int Seconds { get; set; }

        const int DelayMilliseconds = 1000;

        private static readonly PingMeadowCommand Instance = new PingMeadowCommand();

        public static void Initialise()
        {
            Resolver.CommandService?.Subscribe<PingMeadowCommand>(Instance.HandleCommand);
        }

        private void HandleCommand(PingMeadowCommand command)
        {
            Resolver.Log.Info($"Received PingMeadowCommand command with countdown: {command.Seconds}");

            while (command.Seconds-- > 0)
            {
                Resolver.Log.Info($"Remaining {command.Seconds}s...");
                Thread.Sleep(DelayMilliseconds); // use Thread.Sleep() to stop task switching?
            }
        }
    }
   
}
