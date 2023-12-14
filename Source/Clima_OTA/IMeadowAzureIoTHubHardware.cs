using Clima_OTA.Model;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Atmospheric;
using System;

namespace Clima_OTA
{
    internal interface IMeadowAzureIoTHubHardware
    {
        public RgbPwmLed RgbPwmLed { get; }

        public void Initialize();
        void StartUpdatingWeather();
        void StartUpdatingVoltages();

        public event EventHandler<Meadow.IChangeResult<ClimaRecord>> Updated;
    }
}