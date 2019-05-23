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
        public static SitecoreWebService2SoapClient Client
        {
            get
            {
                var client = new SitecoreWebService2SoapClient();
                client.Endpoint.Address = new EndpointAddress(Properties.EndPoint);
                return client;
            }
        }
    }
}
