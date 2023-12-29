namespace ClimaServices.Model;

public struct ClimaRecordDto
{
    public string TestRun { get; set; }
    public string DateTime { get; set; }
    public long Count { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public long TotalMemory { get; set; }
    public double BatteryVoltage { get; set; }
    public double SolarVoltage { get; set; }
}