using Azure.Messaging.EventHubs;
using ClimaServices.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace ClimaServices
{
    public class FunctionStoreClimaData
    {
        private readonly ILogger<FunctionStoreClimaData> _logger;

        public FunctionStoreClimaData(ILogger<FunctionStoreClimaData> logger)
        {
            _logger = logger;
        }

        [Function(nameof(FunctionStoreClimaData))] 
        public async Task Run(
            [EventHubTrigger(eventHubName: "hsl-meadow-iot-hub", Connection = "ConnectionString_IoTHub")] EventData[] events)
        {
            TableServiceClient tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("ConnectionString_Table"));
            // New instance of TableClient class referencing the server-side table
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: "clima");

            foreach (EventData @event in events)
            {
                _logger.LogInformation($"Event Data: {@event.Data}");

                var success = @event.SystemProperties.TryGetValue("iothub-connection-device-id", out var iotHubDeviceId);
                string deviceId = iotHubDeviceId as string ?? string.Empty;
                
                //var deviceData = JsonSerializer.Deserialize<ClimaRecordDto>(@event.Data);
                var deviceData = JsonSerializer.Deserialize(@event.Data, MyJsonContext_ClimaRecordDto.Default.ClimaRecordDto);
                await tableClient.CreateIfNotExistsAsync();

                var tableEntity = new ClimaRecordTableEntity
                {
                    DateTime = deviceData.DateTime,
                    DeviceId = deviceId,
                    PartitionKey = $"{deviceData.TestRun}",//$"{deviceId}-{deviceData.TestRun}",
                    RowKey = $"{deviceId}-{@event.EnqueuedTime.UtcTicks}",
                    TestRun = deviceData.TestRun,
                    
                    Temperature = deviceData.Temperature,
                    TotalMemory = deviceData.TotalMemory,
                    SolarVoltage = deviceData.SolarVoltage,
                    BatteryVoltage = deviceData.BatteryVoltage,
                    Humidity = deviceData.Humidity,
                    Pressure = deviceData.Pressure,
                    Count = deviceData.Count,

                    WindMetersPerSecond = deviceData.WindMetersPerSecond,
                    RainMillimeters = deviceData.RainMillimeters,
                    WindCompassCardinalName = deviceData.WindCompassCardinalName,
                    WindCompassDecimalDegrees = deviceData.WindCompassDecimalDegrees,
                    SCD40PartsPerMillion = deviceData.SCD40PartsPerMillion,
                    SCD40Temperature = deviceData.SCD40Temperature,
                    SCD40Humidity = deviceData.SCD40Humidity,
                    Longitude = deviceData.Longitude,
                    Latitude = deviceData.Latitude
                };

                // Add new item to server-side table
                await tableClient.AddEntityAsync<ClimaRecordTableEntity>(tableEntity);

                //yield return deviceData;
            }
        }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ClimaRecordDto))]
    internal partial class MyJsonContext_ClimaRecordDto : JsonSerializerContext
    {
    }
}
