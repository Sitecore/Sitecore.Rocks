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
    public class LinksTests
    {
        [Fact]
        public async Task GetsReferences()
        {
            const string testId = "{ABD4054F-0173-4F50-B73F-0CCFEE32B3BF}";
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Links.GetLinks",
                new ArrayOfAnyType {Properties.MasterDb, testId}, Properties.Credentials);
            var result = response?.Body?.ExecuteResult?.ToDynamic();
            Assert.NotNull(result);
            Assert.Equal(3, ((IList<dynamic>)result.links.references.item).Count(x => x.category == "Content"));
        }

        [Fact]
        public async Task GetsReferrers()
        {
            const string testId = "{DB7B6F7E-16ED-40C9-B507-22350B26B4C6}";
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Links.GetLinks",
                new ArrayOfAnyType { Properties.MasterDb, testId }, Properties.Credentials);
            var result = response?.Body?.ExecuteResult?.ToDynamic();
            Assert.NotNull(result);
            Assert.Equal(3, ((IList<dynamic>)result.links.referrers.item).Count(x => x.category == "Content"));
        }
    }
}
