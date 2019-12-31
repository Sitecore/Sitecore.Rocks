using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public static class ClientFactory
    {
        private static string EndPoint
        {
            get
            {
                var host = System.Environment.GetEnvironmentVariable("HardRocksHost") ??
                           "https://rockstest930.local";
                return $"{host}/sitecore/shell/webservice/service2.asmx";
            }
        }

        public static Version SitecoreVersion
        {
            get
            {
                var version = System.Environment.GetEnvironmentVariable("SitecoreVersion") ??
                           "9.1";
                return new Version(version);
            }
        }

        public static SitecoreWebService2SoapClient Client
        {
            get
            {
                var client = new SitecoreWebService2SoapClient();
                client.Endpoint.Address = new EndpointAddress(EndPoint);
                var binding = (BasicHttpBinding)client.Endpoint.Binding;
                if (!EndPoint.StartsWith("https"))
                {
                    binding.Security.Mode = BasicHttpSecurityMode.None;
                }
                // Need to up buffer size due to size of some results, e.g. GetsLayout on core
                binding.MaxReceivedMessageSize = 1024 * 1024 * 16;
                return client;
            }
        }
    }
}
