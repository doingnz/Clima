using Amqp;
using Amqp.Framing;
using Clima_IoTHub.Model;
using Clima_OTA.Services;
using Meadow;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Clima_OTA.Controllers
{
    using Amqp.Listener;
    using Amqp.Sasl;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.Json.Serialization;

    /// <summary>
    ///  https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
    /// </summary>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ClimaRecord))]
    internal partial class MyJsonContext_ClimaRecord : JsonSerializerContext
    {
    }


    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ClimaRecordDto))]
    internal partial class MyJsonContext_ClimaRecordDto : JsonSerializerContext
    {
    }

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
        private const string Cert = Secrets.DEVICE_CERTIFICATE;

        public bool isAuthenticated { get; private set; }

        private Connection connection;
        private SenderLink sender;

        private int testRunId = 0;
        //private int messageId = 0;

        public IoTHubController(int testRunId)
        {
            this.testRunId = testRunId;
        }

        public async Task<bool> Initialize()
        {
            try
            {
                string hostName = HubName + ".azure-devices.net";
                string userName = DeviceId + "@sas." + HubName;
                string senderAddress = "devices/" + DeviceId + "/messages/events";

                Address address = new Address(host:hostName, port:5671, user:userName, scheme:"AMQPS");//, password:SasToken);

                var x = GetCertificateFromFile(DeviceId + "-all");
                var y = GetCertificateFromFile(DeviceId);

                // start a host with custom SSL and SASL settings
                Console.WriteLine("Starting server...");
                ContainerHost host = new ContainerHost(address);
                var listener = host.Listeners[0];
                listener.SSL.Certificate = GetCertificate(Cert);
                listener.SSL.ClientCertificateRequired = true;
                listener.SSL.RemoteCertificateValidationCallback = ValidateServerCertificate;
                listener.SASL.EnableExternalMechanism = true;
                host.Open();
                Resolver.Log.Info($"IoTHubController: Container host is listening on {address.Host}:{address.Port}");

                string messageProcessor = "message_processor";
                host.RegisterMessageProcessor(messageProcessor, new MessageProcessor());
                Resolver.Log.Info($"IoTHubController: Message processor is registered on {messageProcessor}");

                Resolver.Log.Info("IoTHubController: Create connection factory..."); 
                ConnectionFactory factory = new ConnectionFactory();
                factory.SSL.ClientCertificates.Add(GetCertificate(Cert));
                factory.SSL.RemoteCertificateValidationCallback = ValidateServerCertificate;
                factory.SASL.Profile = SaslProfile.External;
                
                Resolver.Log.Info("IoTHubController: Create connection ...");
                connection = await factory.CreateAsync(address);

                Resolver.Log.Info("IoTHubController: Create session ...");
                var session = new Session(connection);

                Resolver.Log.Info("IoTHubController: Create SenderLink ...");
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

        class MessageProcessor : IMessageProcessor
        {
            int IMessageProcessor.Credit
            {
                get { return 300; }
            }

            void IMessageProcessor.Process(MessageContext messageContext)
            {
                Resolver.Log.Info("Received a message.");
                messageContext.Complete();
            }
        }

        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Resolver.Log.Info($"Received remote certificate. Subject: {certificate.Subject}, Policy errors: {sslPolicyErrors}");
            return true;
        }

        /// <summary>
        /// Read X509 Certificate from text file
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use the steps here to make rootca, subca and a device key using the same Device ID as will be registered in Iot Hub.
        ///   Tutorial: Create and upload certificates for testing: https://learn.microsoft.com/en-us/azure/iot-hub/tutorial-x509-test-certs?tabs=linux
        ///
        /// Use ideas from here to make application to sign-on to Azure IoT.dddddddddddddddddddddddddddddddddddddddddddd
        /// Example AMQP establish secure connection notes: https://github.com/WildernessLabs/amqpnetlite/blob/master/docs/articles/building_application.md#connectionfactory
        /// Example AMQP code that uses certificates: https://github.com/WildernessLabs/amqpnetlite/blob/19f974e7e083a1f9f9c183aaa21566997d866808/Examples/PeerToPeer/PeerToPeer.Certificate/Program.cs#L53
        /// 
        /// 
        /// </remarks>


        static X509Certificate2 GetCertificateFromFile(string deviceId)
        {
            //string certFile = Path.Combine(MeadowOS.FileSystem.DocumentsDirectory, $"{deviceId}.crt");
            string certFile = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, $"{deviceId}.crt");

            if (File.Exists(certFile))
            {
                Resolver.Log.Info($"Found Cert file: {certFile}");
                byte[] rawCertData = File.ReadAllBytes(certFile);
                X509Certificate2 certificate = new X509Certificate2(rawCertData, password: "1234");
                return certificate;
            }

            return null;
        }

        static X509Certificate2 GetCertificate(string certFindValue)
        {
            byte[] rawCertData = Encoding.Default.GetBytes(certFindValue);
            X509Certificate2 certificate = new X509Certificate2(rawCertData, password: "1234");
            return certificate;

//            throw new ArgumentException("No certificate can be found using the find value " + certFindValue);
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
                    TestRun = $"{testRunId}",
                    Count = reading.Count,
                    DateTime = reading.DateTime,
                    TotalMemory = reading.TotalMemory,
                    Temperature = reading.Temperature.Celsius,
                    Humidity = reading.Humidity.Percent,
                    Pressure = reading.Pressure.Millibar,
                    BatteryVoltage = reading.BatteryVoltage.Volts,
                    SolarVoltage = reading.SolarVoltage.Volts,  
                    WindMetersPerSecond = reading.WindMetersPerSecond,
                    RainMillimeters = reading.RainMillimeters,
                    WindCompassCardinalName = $"{reading.WindCompassCardinalName}",
                    WindCompassDecimalDegrees = reading.WindCompassDecimalDegrees,
                    SCD40PartsPerMillion = reading.SCD40PartsPerMillion,
                    SCD40Temperature = reading.SCD40Temperature,
                    SCD40Humidity = reading.SCD40Humidity,
                    Longitude = reading.Longitude?.ToString() ?? "",
                    Latitude = reading.Latitude?.ToString() ?? ""
                };
#endif
                //string messagePayload = JsonSerializer.Serialize<ClimaRecordDto>(data);

                // Serializer invokes pre-generated serialization logic for increased throughput and other benefits.
                string messagePayload = JsonSerializer.Serialize(data, MyJsonContext_ClimaRecordDto.Default.ClimaRecordDto);

                Resolver.Log.Info("Create message");
                var payloadBytes = Encoding.UTF8.GetBytes(messagePayload);
                var message = new Message()
                {
                    BodySection = new Data() { Binary = payloadBytes }
                };

                //sender.SendAsync(message);
                sender.Send(message);

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