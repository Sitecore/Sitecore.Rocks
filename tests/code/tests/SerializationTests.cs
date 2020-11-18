using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            await CleanUp(testId, Properties.MasterDb);

            await SerializeItem(testId, Properties.MasterDb);
            var filePath = await GetFilePath(testId, Properties.MasterDb);

            Assert.True(File.Exists(filePath), "Could not find serialized item file.");
        }

        [Fact]
        public async Task SerializesTree()
        {
            const string testId = "{D93BB9BE-BC6A-4F2B-800E-80C598942220}";
            const string childId = "{8AA237D4-A5F0-4F23-B501-6BA1BDF68A74}";
            const string grandchildId = "{04B680EC-9235-4428-9DBA-A04B0D00E063}";
            await CleanUp(testId, Properties.MasterDb);
            await CleanUp(childId, Properties.MasterDb);
            await CleanUp(grandchildId, Properties.MasterDb);

            await SerializeTree(testId, Properties.MasterDb);
            var filePath = await GetFilePath(testId, Properties.MasterDb);
            var childFilePath = await GetFilePath(childId, Properties.MasterDb);
            var grandchildFilePath = await GetFilePath(grandchildId, Properties.MasterDb);

            Assert.True(File.Exists(filePath), "Could not find serialized item file.");
            Assert.True(File.Exists(childFilePath), "Could not find serialized child item file.");
            Assert.True(File.Exists(grandchildFilePath), "Could not find serialized grandchild item file.");
        }

        [Fact]
        public async Task UpdatesItem()
        {
            const string testId = "{6DA6701C-2CDF-47E7-A289-260CBD7738A3}";
            const string oldTitle = "UpdatesItem";
            var newTitle = Guid.NewGuid().ToString("N").Substring(0, oldTitle.Length); //no dashes, same length
            await CleanUp(testId, Properties.MasterDb);

            await SerializeItem(testId, Properties.MasterDb);
            var filePath = await GetFilePath(testId, Properties.MasterDb);
            filePath = filePath.Trim(); // some sort of whitespace being added
            var text = File.ReadAllText(filePath);
            text = Regex.Replace(text, $"^{oldTitle}", newTitle, RegexOptions.Multiline);
            File.WriteAllText(filePath, text);
            await UpdateItem(testId, Properties.MasterDb);

            var response =
                await ClientFactory.Client.GetItemFieldsAsync(testId, Properties.Language, Properties.LatestVersion, false, Properties.MasterDb,
                    Properties.Credentials);
            var result = response?.Body?.GetItemFieldsResult?.ToDynamic();
            var titleField = ((IList<dynamic>)result.item.field).First(x => x.name.Equals("Title"));

            Assert.NotNull(titleField);
            Assert.Equal(newTitle, titleField.value);
        }

        [Fact]
        public async Task UpdatesTree()
        {
            const string testId = "{0FE98B31-D107-452A-84C5-BA0F7BC20D92}";
            const string grandchildId = "{B3AD8882-73A5-4FD0-BB76-0A872DE713D3}";
            const string oldTitle = "Grandchild";
            var newTitle = Guid.NewGuid().ToString("N").Substring(0, oldTitle.Length); //no dashes, same length
            await CleanUp(testId, Properties.MasterDb);
            await CleanUp(grandchildId, Properties.MasterDb);

            await SerializeTree(testId, Properties.MasterDb);
            var filePath = await GetFilePath(grandchildId, Properties.MasterDb);
            filePath = filePath.Trim(); // some sort of whitespace being added
            var text = File.ReadAllText(filePath);
            text = Regex.Replace(text, $"^{oldTitle}", newTitle, RegexOptions.Multiline);
            File.WriteAllText(filePath, text);
            await UpdateTree(testId, Properties.MasterDb);

            var response =
                await ClientFactory.Client.GetItemFieldsAsync(grandchildId, Properties.Language, Properties.LatestVersion, false, Properties.MasterDb,
                    Properties.Credentials);
            var result = response?.Body?.GetItemFieldsResult?.ToDynamic();
            var titleField = ((IList<dynamic>)result.item.field).First(x => x.name.Equals("Title"));

            Assert.NotNull(titleField);
            Assert.Equal(newTitle, titleField.value);
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
            var path = response.Body.ExecuteResult;

            // if instance is Dockerized, need to get the mounted path
            var mountedPath = System.Environment.GetEnvironmentVariable("SerializationMount");
            if (string.IsNullOrEmpty(mountedPath))
            {
                return path;
            }
            return path.ToLowerInvariant().Replace("c:\\inetpub\\wwwroot\\app_data\\serialization", mountedPath);
        }

        private async Task SerializeItem(string itemId, string database)
        {
            var response = await ClientFactory.Client.ExecuteAsync(
                $"{Properties.RequestNamespace}.Serialization.SerializeItem",
                new ArrayOfAnyType()
                {
                    itemId,
                    database
                },
                Properties.Credentials);
        }

        private async Task SerializeTree(string itemId, string database)
        {
            var response = await ClientFactory.Client.ExecuteAsync(
                $"{Properties.RequestNamespace}.Serialization.SerializeTree",
                new ArrayOfAnyType()
                {
                    itemId,
                    database
                },
                Properties.Credentials);
        }

        private async Task UpdateItem(string itemId, string database)
        {
            var response = await ClientFactory.Client.ExecuteAsync(
                $"{Properties.RequestNamespace}.Serialization.RevertItem",
                new ArrayOfAnyType()
                {
                    itemId,
                    database
                },
                Properties.Credentials);
        }

        private async Task UpdateTree(string itemId, string database)
        {
            var response = await ClientFactory.Client.ExecuteAsync(
                $"{Properties.RequestNamespace}.Serialization.RevertTree",
                new ArrayOfAnyType()
                {
                    itemId,
                    database
                },
                Properties.Credentials);
        }
    }
}
