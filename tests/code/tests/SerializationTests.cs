using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Rocks.Server.IntegrationTests.Extensions;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;
using Xunit;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class SerializationTests
    {
        [Fact]
        public async Task SerializesItem()
        {
            const string testId = "{03D3336B-1C82-4390-9400-148BA30E136C}";
            const string testDatabase = "master";
            await CleanUp(testId, Properties.MasterDb);

            await SerializeItem(testId, testDatabase);
            var filePath = await GetFilePath(testId, testDatabase);

            Assert.True(File.Exists(filePath), "Could not find serialized item file.");
        }

        [Fact]
        public async Task GetsSerializationStatusSerialized()
        {
            const string testId = "{E2802C10-9EC1-44C3-8037-A72C0843C9A1}";
            await CleanUp(testId, Properties.MasterDb);

            await SerializeItem(testId, Properties.MasterDb);
            var response =
                await ClientFactory.Client.GetItemFieldsAsync(testId, Properties.Language, Properties.LatestVersion, false, Properties.MasterDb,
                    Properties.Credentials);
            var status = response?.Body?.GetItemFieldsResult?.ToDynamic();

            Assert.NotNull(status);
            Assert.Equal("1", status.item.serializationstatus);
        }

        [Fact]
        public async Task GetsSerializationStatusNotSerialized()
        {
            const string testId = "{5D5CBF96-C190-4B91-94E7-51B34F88646D}";
            await CleanUp(testId, Properties.MasterDb);

            var response =
                await ClientFactory.Client.GetItemFieldsAsync(testId, Properties.Language, Properties.LatestVersion, false, Properties.MasterDb,
                    Properties.Credentials);
            var status = response?.Body?.GetItemFieldsResult?.ToDynamic();

            Assert.NotNull(status);
            Assert.Equal("0", status.item.serializationstatus);
        }

        [Fact]
        public async Task GetsSerializationStatusChanged()
        {
            const string testId = "{4DCD8719-56B1-405D-AB98-7A62B0FA5009}";
            const string fieldId = "{75577384-3C97-45DA-A847-81B00500E250}";
            await CleanUp(testId, Properties.MasterDb);

            // Serialize, make a change, then get item and its status
            await SerializeItem(testId, Properties.MasterDb);
            var saveRequest = new XDocument(
                new XElement("sitecore",
                    new XElement("field",
                        new XAttribute("itemid", testId),
                        new XAttribute("fieldid", fieldId),
                        new XAttribute("language", Properties.Language),
                        new XAttribute("version", Properties.Version),
                        new XElement("value", Guid.NewGuid().ToString())
                    )
                )
            );
            await ClientFactory.Client.SaveAsync(saveRequest.ToString(), Properties.MasterDb, Properties.Credentials);
            var response =
                await ClientFactory.Client.GetItemFieldsAsync(testId, Properties.Language, Properties.LatestVersion, false, Properties.MasterDb,
                    Properties.Credentials);
            var status = response?.Body?.GetItemFieldsResult?.ToDynamic();

            Assert.NotNull(status);
            Assert.Equal("2", status.item.serializationstatus);
        }

        private async Task CleanUp(string testId, string testDatabase)
        {
            var filePath = await GetFilePath(testId, testDatabase);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private async Task<string> GetFilePath(string itemId, string database)
        {
            var response = await ClientFactory.Client.ExecuteAsync(
                $"{Properties.RequestNamespace}.Serialization.GetItemSerializationPath",
                new ArrayOfAnyType()
                {
                    itemId,
                    database
                },
                Properties.Credentials);
            return response.Body.ExecuteResult;
        }

        private async Task SerializeItem(string itemId, string database)
        {
            var response = await ClientFactory.Client.ExecuteAsync(
                $"{Properties.RequestNamespace}.Serialization.SerializeItem",
                new ArrayOfAnyType()
                {
                    itemId,
                    "master"
                },
                Properties.Credentials);
        }
    }
}
