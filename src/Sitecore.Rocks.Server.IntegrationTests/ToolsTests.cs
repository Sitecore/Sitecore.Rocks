using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Sitecore.Rocks.Server.IntegrationTests.Extensions;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;
using Xunit;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class ToolsTests
    {
        [Fact]
        public async Task GetsJobs()
        {
            // trigger a job first
            await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Indexes.RebuildIndex", new ArrayOfAnyType() { "sitecore_master_index" }, Properties.Credentials);

            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.UI.JobViewer.GetJobs", new ArrayOfAnyType(), Properties.Credentials);
            var jobs = response?.Body?.ExecuteResult?.ToDynamic();

            Assert.NotNull(jobs);
            Assert.Contains((IList<dynamic>) jobs.jobs.job, job => job.category == "Indexing");
        }
    }
}
