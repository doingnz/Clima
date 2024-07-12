﻿using Meadow.Devices.Clima.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Devices.Clima.Controllers;

public class LocationController
{
    private IGnssSensor gnss;

    public bool LogData { get; set; } = false;

    public event EventHandler<GnssPositionInfo> PositionReceived;

    public LocationController(IClimaHardware clima)
    {
        if (clima.Gnss is { } gnss)
        {
            this.gnss = gnss;
            this.gnss.GnssDataReceived += OnGnssDataReceived;
            this.gnss.StartUpdating();
        }
    }

    private void OnGnssDataReceived(object sender, IGnssResult e)
    {
        if (e is GnssPositionInfo pi)
        {
            if (pi.IsValid && pi.Position != null)
            {
                // we only need one position fix - weather stations don't move
                Resolver.Log.InfoIf(LogData, $"GNSS Position: lat: [{pi.Position.Latitude}], long: [{pi.Position.Longitude}]");
                PositionReceived?.Invoke(this, pi);
                gnss.StopUpdating();
            }
        }
    }
}