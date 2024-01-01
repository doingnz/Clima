namespace Clima_OTA.Controllers
{
    public class Secrets
    {
        /// <summary>
        /// Name of the Azure IoT Hub created
        /// </summary>
        public const string HUB_NAME = "HUB_NAME";

        /// <summary>
        /// Name of the Azure IoT Hub created
        /// </summary>
        public const string DEVICE_ID = "DEVICE_ID";

        /// <summary>
        /// example "SharedAccessSignature sr=MeadowIoTHub ..... "
        /// 
        /// az iot hub generate-sas-token --hub-name HUB_NAME --device-id DEVICE_ID --resource-group RESOURCE_GROUP --login [Open Shared access policies -> Select iothubowner -> copy Primary connection string] --duration 300000
        /// </summary>
        public const string SAS_TOKEN = "SharedAccessSignature ....";

        /// <summary>
        /// CRT fILE CONTENTS PASTED IN HERE. Alternate is to include .crt file into project.
        /// </summary>
        public const string DEVICE_CERTIFICATE = @"
-----BEGIN CERTIFICATE-----
MIIDcjCCAlqgAwIBAgIRAI+HuSiEaW6SBiFEAj1k554wDQYJKoZIhvcNAQELBQAw
... deleted for security
rNTelqc6LqOhU53IDWCa7CSXbTCUiQ==
-----END CERTIFICATE-----

-----BEGIN CERTIFICATE-----
MIIDAjCCAeqgAwIBAgIQUXpUtzSTBe3rYf7+ku81njANBgkqhkiG9w0BAQsFADAb
... deleted for security
1Y4YnosD
-----END CERTIFICATE-----
-----BEGIN PRIVATE KEY-----
MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQD5jgqPKwE3aSWc
... deleted for security
HU8gSxnfKVu8A+B7kQ8om38wVA==
-----END PRIVATE KEY-----
";
    }
}