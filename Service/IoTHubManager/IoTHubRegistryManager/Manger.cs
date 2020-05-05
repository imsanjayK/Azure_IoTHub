using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Data;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IoTHubRegistryManager
{
    class Manger
    {
        private readonly RegistryManager _registryManager;
        private readonly string _connectionString = Environment.GetEnvironmentVariable("ConnectionString");
        public Manger()
        {
            _registryManager = RegistryManager.CreateFromConnectionString(_connectionString) 
                                ?? throw new ArgumentNullException(nameof(_connectionString));
        }

        public string Addx509CertificateDeviceToHub()
        {
            //var primaryThumbprint = Generatex509CertificateThumbprint();
            //var secondaryThumbprint = Generatex509CertificateThumbprint();

            var cert = DeviceAuthWithX509Certificate();

            string primaryThumbprint, secondaryThumbprint;
            primaryThumbprint = secondaryThumbprint = cert.Thumbprint;

            var deviceId = cert.Subject.Remove(cert.Subject.IndexOf(',')).Substring(3);

            var device = new Device(deviceId)
            { 
                Capabilities = new DeviceCapabilities()
                {
                    IotEdge = true
                },
                Authentication = new AuthenticationMechanism
                {
                    Type = AuthenticationType.CertificateAuthority,
                    X509Thumbprint = new X509Thumbprint
                    {
                        PrimaryThumbprint = primaryThumbprint,
                        SecondaryThumbprint = secondaryThumbprint
                    }
                }
            };
            string response = "Registration fails";
            Device registeredDevice = null;
            try
            {
                registeredDevice = _registryManager.AddDeviceAsync(device).Result;
                response = registeredDevice.Id;
            }
            catch (Exception ex)
            {
                throw new DuplicateNameException($"Device {deviceId} already exist", ex);
            }
            return response;
        }

        private static string Generatex509CertificateThumbprint()
        {
            var data = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());
            var hashData = new SHA1Managed().ComputeHash(data);
            var thumprint = string.Empty;
            foreach (var item in hashData)
            {
                thumprint += item.ToString("x2");
            }
            return thumprint;
        }

        private X509Certificate2 DeviceAuthWithX509Certificate()
        {
            var _certificateFileName = Path.GetFullPath(@"../../../../certificate/iothub-device1.p12");
            X509Certificate2 certificate = new X509Certificate2(_certificateFileName, "hello");

            return certificate;
        }

        private SecureString ToSecureString(string plainString)
        {
            var ss = new SecureString();
            foreach (var c in plainString)
            {
                ss.AppendChar(c);
            }

            return ss;
        }
    }
}
