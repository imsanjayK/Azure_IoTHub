using System;
using Microsoft.Azure.EventHubs;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Collections.Generic;


using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReadMessage
{
    class ReadDeviceToCloudMessages
    {
        // Event Hub-compatible endpoint
        // az iot hub show --query properties.eventHubEndpoints.events.endpoint --name {your IoT Hub name}
        private readonly static string _eventHubsCompatibleEndpoint = "sb://iothub-ns-egdedevelo-2882963-22318eef18.servicebus.windows.net/";

        // Event Hub-compatible name
        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
        private readonly static string _eventHubsCompatiblePath = "egdedevelopmenthub";

        // az iot hub policy show --name service --query primaryKey --hub-name {your IoT Hub name}
        private readonly static string _iotHubSasKey = "bQ9t9pnrCF9aUiWCaWnuWJsr+COFArMzpFbFR/41gkg=";

        private readonly static string _iotHubSasKeyName = "service";
        private static EventHubClient _eventHubClient;

        private static async Task ReceiveMessagesFromDeviceAsync(string partitionId, CancellationToken cts)
        {
            // Create the receiver using the default consumer group.
            var eventHubReceiver = _eventHubClient
                                    .CreateReceiver("$Default", partitionId, EventPosition.FromEnqueuedTime(DateTime.UtcNow));
            
            Console.WriteLine("Create receiver on partition: " + partitionId);
            while (true)
            {
                if (cts.IsCancellationRequested) break;

                Console.WriteLine($"Listening for messages on: {partitionId}");

                // Check for EventData - this methods times out if there is nothing to retrieve.
                var events = await eventHubReceiver.ReceiveAsync(100);

                // If there is data in the batch, process it.
                if (events == null) continue;

                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);

                    Console.WriteLine($"Message received on partition {partitionId}:");
                    Console.WriteLine("=======================================");
                    Console.WriteLine($"  Message: {data}");
                    Console.WriteLine("=======================================");
                    
                    Console.WriteLine("Application properties (set by device):");
                    foreach (var prop in eventData.Properties)
                    {
                        Console.WriteLine($"  {prop.Key}: {prop.Value}");
                    }
                    Console.WriteLine("System properties (set by IoT Hub):");
                    foreach (var prop in eventData.SystemProperties)
                    {
                        Console.WriteLine($"  {prop.Key}: {prop.Value}");
                    }
                }
            }
        }

        public static async Task MainAsync()
        {
            Console.WriteLine("IoT Hub: Read device to cloud (D2C) messages. Ctrl-C to exit.\n");

            // Create an EventHubClient instance to connect to the
            // IoT Hub Event Hubs-compatible endpoint.
            try
            {
                var endpoint = new Uri(_eventHubsCompatibleEndpoint);
                //var connectionString = new EventHubsConnectionStringBuilder(endpoint, s_eventHubsCompatiblePath, s_iotHubSasKeyName, s_iotHubSasKey);
                var connectionString = "Endpoint=sb://ihsuprodblres096dednamespace.servicebus.windows.net/;" +
                                       "SharedAccessKeyName=iothubowner;SharedAccessKey=ZiNjmwUpVNz+xcjJhqXwO8qVffa7Y82myv5YbapN4EI=;" +
                                       "EntityPath=iothub-ehub-demoedgehu-3340686-c51830f149";
                _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString.ToString());

                // Create a PartitionReciever for each partition on the hub.
                var runtimeInfo =  _eventHubClient.GetRuntimeInformationAsync().GetAwaiter().GetResult();
                var d2cPartitions = runtimeInfo.PartitionIds;
                CancellationTokenSource cts = new CancellationTokenSource();

                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("Exiting...");
                };
               
                var tasks = new List<Task>();
                foreach (string partition in d2cPartitions)
                {
                    tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
                }
                // Wait for all the PartitionReceivers to finsih.
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex )
            {

                throw;
            }
        }
    }
}
