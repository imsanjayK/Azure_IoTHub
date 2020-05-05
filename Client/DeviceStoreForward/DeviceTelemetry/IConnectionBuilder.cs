namespace EdgeManagement.DeviceService.DeviceTelemetry
{
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Client;
    using System.Threading.Tasks;
    using System;
    internal interface IConnectionBuilder
    {
       
    }

    class ConnectionBuilder: IConnectionBuilder
    {
        private readonly DeviceClient deviceClient;

        public DeviceClient DeviceClient
        {
            get { return deviceClient; }
        }

        private readonly TimeSpan delay;

        public TimeSpan Delay
        {
            get { return delay; }
        }


        public ConnectionBuilder()
        {
            delay = TimeSpan.Parse(Environment.GetEnvironmentVariable("Delay"));
            deviceClient = CreateDeviceClientAsync(TransportType.Mqtt_Tcp_Only).Result;
        }

        private async Task<DeviceClient> CreateDeviceClientAsync(TransportType transportType)
        {
            var hostName = Environment.GetEnvironmentVariable("HostName");
            var deviceId = Environment.GetEnvironmentVariable("DeviceId");
            var key = Environment.GetEnvironmentVariable("SharedKey");
            
            string connectionStringBuilder = $"HostName={hostName}.azure-devices.net;DeviceId={deviceId};SharedAccessKey={key}";
            ITransportSettings[] transportSettings = TransportTypeHandler(transportType);
           
            DeviceClient client = null;
            client = DeviceClient.CreateFromConnectionString(connectionStringBuilder, transportSettings);
           
            await client.OpenAsync();

            return client;
        }

        private ITransportSettings[] TransportTypeHandler(TransportType transportType)
        {
            switch (transportType)
            {
                case TransportType.Mqtt:
                case TransportType.Mqtt_Tcp_Only:
                    return new ITransportSettings[] { new MqttTransportSettings(TransportType.Mqtt_Tcp_Only) };
                case TransportType.Mqtt_WebSocket_Only:
                    return new ITransportSettings[] { new MqttTransportSettings(TransportType.Mqtt_WebSocket_Only) };
                case TransportType.Amqp_WebSocket_Only:
                    return new ITransportSettings[] { new AmqpTransportSettings(TransportType.Amqp_WebSocket_Only) };
                default:
                    return new ITransportSettings[] { new AmqpTransportSettings(TransportType.Amqp_Tcp_Only) };
            }
        }
    }
}