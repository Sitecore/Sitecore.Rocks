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
                           "https://rocksTest911.local";
                return $"{host}/sitecore/shell/webservice/service2.asmx";
            }
        }

        public static SitecoreWebService2SoapClient Client
        {
            get
            {
                var client = new SitecoreWebService2SoapClient();
                client.Endpoint.Address = new EndpointAddress(EndPoint);
                // Need to up buffer size due to size of some results, e.g. GetsLayout on core
                ((BasicHttpBinding) client.Endpoint.Binding).MaxReceivedMessageSize = 1024 * 1024 * 16;
                return client;
            }
        }
    }
}
