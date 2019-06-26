using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Rocks.Server.IntegrationTests.Extensions;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;
using Xunit;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class IndexesTests
    {
        /*
         * Index rebuild is tested in ToolsTests.RebuildsIndexAndGetsJobs
         */

        [Fact]
        public async Task GetIndexes()
        {
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Indexes.GetIndexes",
                new ArrayOfAnyType(), Properties.Credentials);
            var result = response?.Body?.ExecuteResult?.ToDynamic();
            Assert.NotNull(result);
            var indexes = (IList<dynamic>) result.indexes.index;
            Assert.Contains(indexes, index => index.name == "sitecore_master_index");
            Assert.Contains(indexes, index => index.name == "sitecore_web_index");
            Assert.Contains(indexes, index => index.name == "sitecore_core_index");
        }
    }
}
