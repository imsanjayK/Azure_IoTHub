using System;
using System.Data;
using System.Management;
using System.Windows.Forms.DataVisualization.Charting;

namespace IoTHubRegistryManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub Management using Registry manager");
           
            Manger manger = new Manger();
            try
            {
                var device = manger.Addx509CertificateDeviceToHub();
                Console.WriteLine($"Register Device Id: {device}");
            }
            catch (DuplicateNameException ex)
            {

                Console.WriteLine(ex.Message);
            }
        }
       
    }
}
