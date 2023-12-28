using Amqp;
using Amqp.Framing;
using Clima_OTA.Model;
using Clima_OTA.Services;
using Meadow;
using Meadow.Units;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClimaServices.Model;

namespace Clima_OTA.Controllers
{
    /// <summary>
    /// You'll need to create an IoT Hub - https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-create-through-portal
    /// Create a device within your IoT Hub
    /// And then generate a SAS token - this can be done via the Azure CLI 
    /// </summary>
    public class IoTHubController : IStorageService
    {
        private const string HubName = Secrets.HUB_NAME;
        private const string SasToken = Secrets.SAS_TOKEN;
        private const string DeviceId = Secrets.DEVICE_ID;

        public bool isAuthenticated { get; private set; }

        private Connection connection;
        private SenderLink sender;

        //private int messageId = 0;

        public IoTHubController() { }

        public async Task<bool> Initialize()
        {
            try
            {
                string hostName = HubName + ".azure-devices.net";
                string userName = DeviceId + "@sas." + HubName;
                string senderAddress = "devices/" + DeviceId + "/messages/events";

                Resolver.Log.Info("Create connection factory...");
                var factory = new ConnectionFactory();

                Resolver.Log.Info("Create connection ...");
                connection = await factory.CreateAsync(new Address(hostName, 5671, userName, SasToken));

                Resolver.Log.Info("Create session ...");
                var session = new Session(connection);

                Resolver.Log.Info("Create SenderLink ...");
                sender = new SenderLink(session, "send-link", senderAddress);

                isAuthenticated = true;
                return true;
            }
            catch (Exception ex)
            {
                Resolver.Log.Info($"{ex.Message}");
                isAuthenticated = false;
                return false;
            }
        }

        public Task SendEnvironmentalReading(ClimaRecord reading)
        {
            try
            {
                Resolver.Log.Info("Create payload");

                LogMemory("SendEnvironmentalReading");

#if false
                string messagePayload = $"" +
                        $"{{" +
                        $"\"count\":{reading.Count}," +
                        $"\"datetime\":{reading.DateTime}," +
                        $"\"totalmemory\":{reading.TotalMemory}," +
                        $"\"temperature\":{reading.Temperature.Celsius}," +
                        $"\"humidity\":{reading.Humidity.Percent}," +
                        $"\"pressure\":{reading.Pressure.Millibar}," +
                        $"\"batteryvoltage\":{reading.BatteryVoltage.Volts}," +
                        $"\"solarvoltage\":{reading.SolarVoltage.Volts}" +
                        $"}}";
#else
                ClimaRecordDto data = new ClimaRecordDto
                {
                    Count = reading.Count,
                    DateTime = reading.DateTime,
                    TotalMemory = reading.TotalMemory,
                    Temperature = reading.Temperature.Celsius,
                    Humidity = reading.Humidity.Percent,
                    Pressure = reading.Pressure.Millibar,
                    BatteryVoltage = reading.BatteryVoltage.Volts,
                    SolarVoltage = reading.SolarVoltage.Volts
                };
#endif
                string messagePayload = JsonSerializer.Serialize<ClimaRecordDto>(data);
                
                Resolver.Log.Info($"messagePayload: {messagePayload}");

                Resolver.Log.Info("Create message");
                var payloadBytes = Encoding.UTF8.GetBytes(messagePayload);
                var message = new Message()
                {
                    BodySection = new Data() { Binary = payloadBytes }
                };

                sender.SendAsync(message);

                Resolver.Log.Info($"*** DATA SENT - {messagePayload} ***");
            }
            catch (Exception ex)
            {
                Resolver.Log.Info($"-- D2C Error - {ex.Message} --");
            }

            return Task.CompletedTask;
        }

        public static string LogMemory(string function, string commentCount = "", int count = 0, string comment = "")
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            string msg = $"LogMemory, {TimeStamp(),25},{GC.GetTotalMemory(true),12},\"{function,-30}\",\"{(string.IsNullOrWhiteSpace(commentCount) ? "" : commentCount)}\",{(string.IsNullOrWhiteSpace(commentCount) ? "" : count.ToString())},\"{(string.IsNullOrWhiteSpace(comment) ? "" : comment)}\"";
            //Resolver.Log.Info(msg);
            return msg;
        }

        public static string TimeStamp()
        {
            return $"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}";
        }
    }

}