﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Clima.Meadow.Pro.Models;
using Meadow;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Weather;

namespace Clima.Meadow.Pro
{
    /// <summary>
    /// Basically combines all the sensors into one and enables the whole system
    /// to be read at once. Then it can go to sleep in between.
    ///
    /// ## Design considerations:
    ///
    /// we can probably get rid of the StartUpdating and StopUppating stuff
    /// in favor of managing the lifecycle elsewhere for sleep purposes. but we may
    /// not need to, depending on how we design the sleep APIs
    ///
    /// </summary>
    public class ClimateMonitorAgent
    {
        //==== events
        /// <summary>
        /// Raised when a new climate reading has been taken. 
        /// </summary>
        public event EventHandler<ClimateConditions> ClimateConditionsUpdated = delegate { };

        //==== internals
        IF7MeadowDevice Device => MeadowApp.Device;
        object samplingLock = new object();
        CancellationTokenSource? SamplingTokenSource;
        bool IsSampling = false;

        //==== peripherals
        Bme280? bme280;
        WindVane? windVane;
        SwitchingAnemometer? anemometer;

        //==== properties
        /// <summary>
        /// The last read conditions.
        /// </summary>
        public Climate? Climate { get; set; }

        //==== singleton
        private static readonly Lazy<ClimateMonitorAgent> instance =
            new Lazy<ClimateMonitorAgent>(() => new ClimateMonitorAgent());

        public static ClimateMonitorAgent Instance => instance.Value;

        // Only invoked via the singleton instance 
        private ClimateMonitorAgent()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("ClimateMonitor initializing.");

            anemometer = new SwitchingAnemometer(Device, Device.Pins.A01);
            anemometer.UpdateInterval = TimeSpan.FromSeconds(10);
            anemometer.StartUpdating();
            Console.WriteLine("Anemometer up.");

            windVane = new WindVane(Device, Device.Pins.A00);
            Console.WriteLine("WindVane up.");

            bme280 = new Bme280(Device.CreateI2cBus(), (byte)Bme280.Addresses.Default);
            Console.WriteLine("BME280 up.");

            Console.WriteLine("ClimateMonitor initialized.");
        }

        public void StartUpdating(TimeSpan updateInterval)
        {
            Console.WriteLine("ClimateMonitorAgent.StartUpdating()");

            // thread safety
            lock (samplingLock)
            {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                Climate oldClimate;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        Console.WriteLine("ClimateMonitorAgent: About to do a reading.");

                        // cleanup
                        if (ct.IsCancellationRequested)
                        {   // do task clean up here
                        //observers.ForEach(x => x.OnCompleted());
                        IsSampling = false;
                        break;
                        }

                        // capture history
                        oldClimate = Climate ?? new Climate();

                        // read
                        Climate = await Read().ConfigureAwait(false);

                        // build a new result with the old and new conditions
                        var result = new ClimateConditions(Climate, oldClimate);

                        Console.WriteLine("ClimateMonitorAgent: Reading complete.");

                        // let everyone know
                        RaiseEventsAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(updateInterval).ConfigureAwait(false);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            if (!IsSampling) return;

            lock (samplingLock)
            {
                SamplingTokenSource?.Cancel();

                IsSampling = false;
            }
        }

        protected virtual void RaiseEventsAndNotify(ClimateConditions changeResult)
        {
            ClimateConditionsUpdated.Invoke(this, changeResult);
        }

        public virtual async Task<Climate> Read()
        {
            //==== create the read tasks
            var bmeTask = bme280?.Read();
            var windVaneTask = windVane?.Read(); 
 
            //==== await until all tasks complete 
            await Task.WhenAll(bmeTask, windVaneTask);

            var climate = new Climate()
            {
                DateTime = DateTime.Now,
                Humidity = bmeTask?.Result.Humidity,
                Temperature = bmeTask?.Result.Temperature,
                Pressure = bmeTask?.Result.Pressure,
                WindDirection = windVaneTask?.Result,
                WindSpeed = anemometer?.WindSpeed,
            };

            return climate;
        }
    }
}