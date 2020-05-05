
namespace EdgeManagement.DeviceService.DeviceTelemetry
{
    using EdgeManagement.DeviceService.Models;
    using Microsoft.Azure.Devices.Client;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using Newtonsoft.Json;
    using System.Text;
    using System.IO;
    using System;

    interface IDeviceSimulator
    {

    }
    class DeviceSimulator: IDeviceSimulator
    {
        private static int MESSAGE_COUNT = 1;
        //private static Messages _messages = new Messages();

        private ConnectionBuilder _connectionBuilder;
        public DeviceSimulator(ConnectionBuilder connectionBuilder)
        {
            _connectionBuilder = connectionBuilder;
        }

        public Task SendEventAsync( )
        {
           // _messages.MessageInfos = new Dictionary<string, MessageInfo>();
            string dataBuffer;
            
            //To Do: Replace all console with logs Service
            Console.WriteLine("Device sending messages to IoTHub...");
            // var filePath = CreateDirectoryFolder();

            while (true)
            {
                dataBuffer = Guid.NewGuid().ToString();

                //  _messages.MessageInfos = ReadJSONStringData(filePath);

                MessageInfo message = new MessageInfo();
                message.Message_Count = MESSAGE_COUNT++;
                message.Timestamp = DateTime.Now.ToLocalTime();
                message.Message = dataBuffer;

                // _messages.MessageInfos[dataBuffer] = message;
                //var content = JsonConvert.SerializeObject(_messages.MessageInfos);
                //string[] contentStream = { content };
                //  WriteJSONStringData(filePath, contentStream);

                Message eventMessage = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, Formatting.None)));
                var messageBody = JsonConvert.SerializeObject(message, Formatting.Indented);
                Console.WriteLine($"Sending message @ {message.Timestamp }\n{messageBody}");

                try
                {
                    _connectionBuilder.DeviceClient.SendEventAsync(eventMessage);
                    Task.Delay(_connectionBuilder.Delay).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //throw;
                }
            }
        }

        public async static Task ReceiveAsync(DeviceClient deviceClient)
        {
            Console.WriteLine("Device sending to messages to IoTHub...");

           // DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Amqp);
         
            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage =  deviceClient.ReceiveAsync().Result;

                if (receivedMessage != null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (byte b in receivedMessage.GetBytes())
                    {
                        sb.Append((char)b);
                    }

                    messageData = sb.ToString();

                    // dispose string builder
                    sb = null;

                    Console.WriteLine(DateTime.Now.ToLocalTime() + "> Received message: " + messageData);

                   await deviceClient.CompleteAsync(receivedMessage);
                }
                Thread.Sleep(3000);
            }
        }

        #region [ Private Methods ]
        /// <summary>
        /// Read json file and parse it to dictionary, create json file if file not exist. 
        /// </summary>
        /// <param name="filePath">file directory</param>
        ///<exception cref=""></exception>
        /// <returns>Dictionary of MessageInfo class</returns>
        private Dictionary<string, MessageInfo> ReadJSONStringData(string filePath)
        {
            var dataObj = new Dictionary<string, MessageInfo>();
            try
            {
                var filedata = File.ReadAllText(filePath);

                if (!String.IsNullOrEmpty(filedata))
                {
                    dataObj = JsonConvert.DeserializeObject<Dictionary<string, MessageInfo>>(filedata);
                }
            }
            catch (FileNotFoundException fileExp)
            {
                Console.WriteLine(fileExp.Message);
                using (var fileStream = File.Create(filePath))
                {
                    Console.WriteLine("File created");
                }
            }
            catch (Exception)
            {
                throw new Exception("Something went wrong");
            }
            return dataObj;
        }

        private static void WriteJSONStringData(string filepath, string[] messages)
        {
            Console.WriteLine("writting...");
            try
            {
                File.WriteAllLines( filepath, messages);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string CreateDirectoryFolder()
        {
            Console.WriteLine("Creating Directory Folder Backup...");
            string path = Path.Combine(Environment.CurrentDirectory, "Backup");
            //Generate folder 
            var directoryInfo = Directory.CreateDirectory(path);

            // Create a file name for the file you want to create. 
            string fileName = "message";
            string filePath = Path.Combine(path,$"{fileName}.json");

            return filePath;
        }
        #endregion [ Private Methods ]
    }
}
