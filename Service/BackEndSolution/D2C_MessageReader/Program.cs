using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace D2C_MessageReader
{
    class Program
    {
  
        static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub: Read device to cloud messages. Ctrl-C to exit.\n");

            MessageReader reader = new MessageReader();
            reader.InvokeMethod();
        }
    }
}
