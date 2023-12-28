using Meadow.Foundation.Leds;
using Meadow;
using System;
using System.Collections.Generic;
using System.Text;
using Clima_OTA.Model;

namespace Clima_OTA.Controllers
{
    public interface IDisplayController
    {
        public void ShowDataScreen();
        public void ShowSplashScreen();
        public void UpdateClimaResults(ClimaRecord clima);
        public void UpdateLastUpdated(string v);
        public void UpdateStatus(string status);
        public void UpdateSyncStatus(bool syncConnected);
        public void UpdateWiFiStatus(bool isConnected);
    }
}
