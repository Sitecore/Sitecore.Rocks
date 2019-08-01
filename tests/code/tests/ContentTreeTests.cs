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
    public class ContentTreeTests
    {
        [Fact]
        public async Task GetsDatabases()
        {
            var response = await ClientFactory.Client.GetDatabasesAsync(Properties.Credentials);
            var result = response?.Body?.GetDatabasesResult?.ToDynamic();
            Assert.NotNull(result);
            Assert.Collection((IList<dynamic>) result.sitecore.database,
                x => Assert.Contains("core", x),
                x => Assert.Contains("master", x),
                x => Assert.Contains("web", x),
                x => Assert.Contains("filesystem", x));
        }

        [Fact]
        public async Task GetsChildren()
        {
            const string testId = "{717AB89B-7086-48A1-BADD-CA6D114CCE95}";
            var response = await ClientFactory.Client.GetChildrenAsync(testId, Properties.MasterDb, Properties.Credentials);
            var result = response?.Body?.GetChildrenResult?.ToDynamic();
            Assert.NotNull(result);
            Assert.Collection((IList<dynamic>)result.children.item,
                x => Assert.Contains("A", x.text),
                x => Assert.Contains("B", x.text),
                x => Assert.Contains("C", x.text));
        }

        [Fact]
        public async Task CreatesItem()
        {
            const string testId = "{969DCD6B-84AD-4F8F-A05B-6F4F9C86C7D9}";
            const string templateId = "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}"; // Sample Item
            const string name = "Lorem";
            var response = await ClientFactory.Client.AddFromTemplateAsync(testId, templateId, name, Properties.MasterDb,
                Properties.Credentials);
            var result = response?.Body?.AddFromTemplateResult?.ToDynamic();
            Assert.NotNull(result);
            Assert.Equal("ok", result.sitecore.status);
            Assert.True(Guid.TryParse(result.sitecore.data.data, out Guid guid));
        }

        [Fact]
        public async Task SavesItem()
        {
            const string testId = "{BAEA3739-F2D2-4B53-A1F8-55019F8FEAEA}";
            const string value = "Lorem ipsum";

            Func<Task<dynamic>> getTestField = async () =>
            {
                var getFieldsResponse = await ClientFactory.Client.GetItemFieldsAsync(testId, Properties.Language, Properties.Version, true,
                    Properties.MasterDb, Properties.Credentials);
                var fields = getFieldsResponse.Body.GetItemFieldsResult.ToDynamic();
                var saveField = ((IList<dynamic>) fields.item.field).First(x => !x.name.StartsWith("__"));
                return saveField;
            };

            var field = await getTestField();
            var saveRequest = new XDocument(
                new XElement("sitecore",
                    new XElement("field",
                        new XAttribute("itemid", testId),
                        new XAttribute("fieldid", field.fieldid),
                        new XAttribute("language", Properties.Language),
                        new XAttribute("version", Properties.Version),
                        new XElement("value", value)
                    )
                )
            );
            await ClientFactory.Client.SaveAsync(saveRequest.ToString(), Properties.MasterDb, Properties.Credentials);
            field = await getTestField();

            Assert.Equal(value, field.value);
        }
    }
}
