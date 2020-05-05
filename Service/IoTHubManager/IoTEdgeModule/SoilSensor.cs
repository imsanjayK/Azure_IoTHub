using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using System.IO;
using System;

namespace IoTEdgeModule
{
    class SoilSensor
    {
        private static readonly Random Rnd = new Random();
        private readonly TimeSpan messageDelay;
        private readonly int messageCount;
        private readonly TransportType transportType;
        private readonly string certificate;
        private readonly string password;
        private readonly string hostName;
        public SoilSensor()
        {
            messageCount = AppConfig().GetValue("MessageCount", 50);
            messageDelay = AppConfig().GetValue("MessageDelay", TimeSpan.FromSeconds(3));
            transportType = FromString<TransportType>(AppConfig().GetValue<string>("ClientTransportType"));
            certificate = AppConfig().GetValue<string>("Certificate");
            password = AppConfig().GetValue<string>("Password");
            hostName = AppConfig().GetValue<string>("HostName");
        }
        public async Task Simulate()
        {
            DeviceClient client = CreateModuleClientAsync(transportType);
           
            await client.OpenAsync();
            DeviceClient userContext = client;
            
            await SendEventsAsync(client, messageCount, messageDelay);
            await client.SetDesiredPropertyUpdateCallbackAsync(UpdateReportedPropertiesAsync, userContext);
        }
        
        private  DeviceClient CreateModuleClientAsync(TransportType transportType)
        {
            var cert = DeviceAuthWithX509Certificate(certificate, password);
            using (var security = new SecurityProviderX509Certificate(cert))
            {
                   IAuthenticationMethod auth = new DeviceAuthenticationWithX509Certificate(security.GetRegistrationID(),
                                               (security as SecurityProviderX509).GetAuthenticationCertificate());

                DeviceClient client = null;
                try
                {
                    ITransportSettings[] settings = new ITransportSettings[] { new MqttTransportSettings(transportType) };  
                    DeviceClient deviceClient = DeviceClient.Create(hostName, auth, settings);
                    client = deviceClient;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return client;
            }
        }

        static async Task UpdateReportedPropertiesAsync(TwinCollection twinPropertiesPatch, object userContext)
        {
            var client = (DeviceClient)userContext;
            await client.UpdateReportedPropertiesAsync(twinPropertiesPatch);
        }

        static async Task SendEventsAsync( DeviceClient client, int messageCount ,TimeSpan messageDelay)
        {
            int count = 1;

            while ( messageCount >= count)
            {
                var tempData = new Messages
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now.ToLocalTime(),
                    Ambient = new Ambient
                    {
                        Temperature = Rnd.Next(40, 50) - Rnd.NextDouble(),
                        Humidity = Rnd.Next(0, 100),
                        Moisture = Rnd.Next(10, 85),
                        pH = Rnd.Next(0,14) - (Rnd.NextDouble() * 0.5)
                    }
                };

                string dataBuffer = JsonConvert.SerializeObject(tempData);
                var eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));

                eventMessage.Properties.Add("msg-sequence-number", count.ToString());
                eventMessage.Properties.Add("alert-moisture", tempData.Ambient.Moisture < 15? "Moisture reduced":"Normal");

                Console.WriteLine($"[Sending message: {count}]\nBody:{dataBuffer}\n");

                await client.SendEventAsync(eventMessage);
                count++;

                Twin twin = new Twin();
                twin.Properties.Reported = new TwinCollection(dataBuffer);

                await client.UpdateReportedPropertiesAsync(twin.Properties.Reported);
                await Task.Delay(messageDelay);
            }

            if (messageCount < count)
            {
                Console.WriteLine($"Done sending {messageCount} messages");
            }
        }

        private static X509Certificate2 DeviceAuthWithX509Certificate(string fileName, string password)
        {
            var _certificateFileName = Path.GetFullPath($@"../../../../certificate/{fileName}");
            X509Certificate2 certificate = new X509Certificate2(_certificateFileName, password);

            return certificate;
        }
        
        private IConfiguration AppConfig()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile(@"config/appsettings.json", optional: true)
                           .AddEnvironmentVariables()
                           .Build();

            return configuration;
        }

        private T FromString<T>( string value)
        {
            var enums = Enum.GetValues(typeof(T));
            foreach (var item in enums)
            {
                if (value.Equals(item.ToString()))
                {
                    return (T)item;
                }
            }
            return (T)enums.GetValue(enums.Length-1);
        }
    }
}



