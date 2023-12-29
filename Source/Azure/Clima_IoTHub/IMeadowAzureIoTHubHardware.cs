using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using Clima_IoTHub.Model;

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