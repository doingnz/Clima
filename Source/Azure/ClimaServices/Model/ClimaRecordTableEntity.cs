using Azure;
using Azure.Data.Tables;

namespace ClimaServices.Model
{
    internal record  ClimaRecordTableEntity : ITableEntity
    {
        // ITableEntity
        public required string PartitionKey { get; set; }
        public required string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Message meta data
        public string DeviceId { get; set; } = "";

        //User Data
        public string TestRun { get; set; } = "";
        public string DateTime { get; set; } = "";
        public long Count { get; set; }
        public long TotalMemory { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double BatteryVoltage { get; set; }
        public double SolarVoltage { get; set; }
        public double WindMetersPerSecond { get; set; }
        public double RainMillimeters { get; set; }
        public string WindCompassCardinalName { get; set; } = "";
        public double WindCompassDecimalDegrees { get; set; }
        public double SCD40PartsPerMillion { get; set; }
        public double SCD40Temperature { get; set; }
        public double SCD40Humidity { get; set; }
        public string Longitude { get; set; } = "";
        public string Latitude { get; set; } = "";
    }
}
