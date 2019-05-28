using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class Properties
    {
        public static readonly Credentials Credentials = new Credentials
        {
            UserName = "sitecore\\admin",
            Password = "b"
        };

        public static readonly string MasterDb = "master";

        public static readonly string RequestNamespace = "Sitecore.Rocks.Server.Requests";

    }
}
