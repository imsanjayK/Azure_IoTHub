
namespace EdgeManagement.DeviceService
{
    using EdgeManagement.DeviceService.DeviceTelemetry;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
           

            var device = (DeviceSimulator)Provider.GetService(typeof(IDeviceSimulator));
            device.SendEventAsync().ConfigureAwait(false);
            //DeviceSimulator.ReceiveAsync(DeviceConnectionString).GetAwaiter();
        }
        private static IServiceProvider Provider
        {
            get {
                return ConfigureServices();
            }  
        }
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            #region Transient Instances

            //services.AddTransient<IDeviceSimulator, DeviceSimulator>();
            services.AddTransient<ConnectionBuilder>(provider => { return new ConnectionBuilder(); });
            services.AddTransient<IDeviceSimulator>(provider => { return new DeviceSimulator(provider.GetService<ConnectionBuilder>()); });
            #endregion

            return services.BuildServiceProvider();
        }
    }
}
