using System;

namespace IoTEdgeModule
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Soil sensor simulator model");
            Console.WriteLine("=============================");

            new SoilSensor().Simulate().GetAwaiter().GetResult();
        }
    }
}
