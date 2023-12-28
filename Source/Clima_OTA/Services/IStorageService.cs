﻿using System.Threading.Tasks;
using Clima_OTA.Model;


namespace Clima_OTA.Services
{
    public interface IStorageService
    {
        public bool isAuthenticated { get; }
        public Task<bool> Initialize();
        public Task SendEnvironmentalReading(ClimaRecord reading);
    }
}