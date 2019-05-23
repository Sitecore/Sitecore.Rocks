using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Rocks.Server.IntegrationTests.Extensions;
using Xunit;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class ContentTree
    {
        [Fact]
        public async Task GetsDatabases()
        {
            var response = await ClientFactory.Client.GetDatabasesAsync(Properties.Credentials);
            var result = response?.Body?.GetDatabasesResult?.ToDynamic();
            Assert.NotNull(result);
            Assert.Collection((IList<dynamic>) result.sitecore.database,
                x => Assert.Contains("core", x.text),
                x => Assert.Contains("master", x.text),
                x => Assert.Contains("web", x.text),
                x => Assert.Contains("filesystem", x.text));
        }

        [Fact]
        public async Task GetsChildren()
        {
            const string TestId = "{717AB89B-7086-48A1-BADD-CA6D114CCE95}";
            var response = await ClientFactory.Client.GetChildrenAsync(TestId, Properties.MasterDb, Properties.Credentials);
            var result = response?.Body?.GetChildrenResult.ToDynamic();
            Assert.NotNull(result);
            Assert.Collection((IList<dynamic>)result.children.item,
                x => Assert.Contains("A", x.text),
                x => Assert.Contains("B", x.text),
                x => Assert.Contains("C", x.text));
        }
    }
}
