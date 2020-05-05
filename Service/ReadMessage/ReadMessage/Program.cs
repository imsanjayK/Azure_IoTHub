using System;

namespace ReadMessage
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadDeviceToCloudMessages.MainAsync().GetAwaiter();
        }
    }
}
