using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Rocks.Server.IntegrationTests.Extensions;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;
using Xunit;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class LayoutTests
    {
        [Fact]
        public async Task GetsLayout()
        {
            const string testId = "{C694D1A4-6AD2-4BFE-AFC9-85120DE20262}";
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Layouts.GetRawLayout",
                new ArrayOfAnyType {Properties.MasterDb, testId}, Properties.Credentials);
            var result = response?.Body?.ExecuteResult?.ToDynamic();
            Assert.NotNull(result);
            var renderings = (IList<dynamic>) result.r.d.r;
            Assert.Equal(3, renderings.Count);
        }

        [Fact]
        public async Task SavesLayout()
        {
            const string testId = "{C1FB9ADD-2232-47B7-AE61-D04982668A9E}";
            var layout = @"<r>
  <d id=""{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}"">
    <r id=""{493B3A83-0FA7-4484-8FC9-4680991CF743}"" ph=""main"" uid=""{FF13E707-1AE1-49F2-9DEE-79765D770E95}"" />
    <r id=""{493B3A83-0FA7-4484-8FC9-4680991CF743}"" ph=""main"" uid=""{D29F8024-8A3E-43F9-BFD1-F8A53A97A1D1}"" />
    <r id=""{493B3A83-0FA7-4484-8FC9-4680991CF743}"" ph=""main"" uid=""{54270EF4-1EFC-4A32-9F8E-E9CE8A4CB317}"" />
  </d>
</r>";
            layout = layout.Replace("\r\n", "\n"); // make consistent w/ Sitecore internal storage
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Layouts.SaveLayout",
                new ArrayOfAnyType { Properties.MasterDb, testId, Properties.Language, Properties.Version, "__Renderings", layout},
                Properties.Credentials);
            var result = response?.Body?.ExecuteResult;
            Assert.NotNull(result);
            Assert.Equal(layout, result.Trim()); // Sitecore puts an extra line ending on layout XML
        }

        [Fact]
        public async Task GetsRenderingDefinitionsFromMaster()
        {
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Layouts.GetRenderings",
                new ArrayOfAnyType { Properties.MasterDb }, Properties.Credentials);
            var result = response?.Body?.ExecuteResult?.ToDynamic();
            Assert.NotNull(result);
            var renderings = (IList<dynamic>) result.renderings.item;
            Assert.Contains(renderings, rendering => rendering.id == "{493B3A83-0FA7-4484-8FC9-4680991CF743}"); // Sample Rendering
        }

        [Fact]
        public async Task GetsRenderingDefinitionsFromCore()
        {
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Layouts.GetRenderings",
                new ArrayOfAnyType { Properties.CoreDb }, Properties.Credentials);
            var result = response?.Body?.ExecuteResult?.ToDynamic();
            Assert.NotNull(result);
            var renderings = (IList<dynamic>)result.renderings.item;
            Assert.Contains(renderings, rendering => rendering.id == "{E6FBC365-732C-4DCD-8C56-10E8C3B963F5}"); // SearchDataSource
        }
    }
}
