// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.Validations
{
    public class GetItemValidation
    {
        [NotNull]
        public string Execute([NotNull] string contextName, [NotNull] string databaseName, [NotNull] string itemId, [NotNull] string languageName, [NotNull] string version, bool deep)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(languageName, nameof(languageName));
            Assert.ArgumentNotNull(version, nameof(version));

            var v = Version.Latest;
            if (!string.IsNullOrEmpty(version))
            {
                v = Version.Parse(version);
            }

            var database = Factory.GetDatabase(databaseName);
            var language = LanguageManager.GetLanguage(languageName);
            var rootItem = database.GetItem(itemId, language, v);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("validations");

            TempFolder.EnsureFolder();
            var fileName = Path.Combine(FileUtil.MapPath(TempFolder.Folder), string.Format("validation_{0}_{1}_{2}_{3}_{4}{5}.xml", contextName, database.Name, rootItem.ID.ToShortID(), language.Name, v, deep ? "_deep" : string.Empty));
            var tempFileName = fileName + ".tmp";

            if (FileUtil.Exists(tempFileName))
            {
                output.WriteAttributeString("generating", "true");
            }

            if (FileUtil.Exists(fileName))
            {
                var text = FileUtil.ReadUTF8File(fileName);

                output.WriteCData(text);
            }

            output.WriteEndElement();
            output.Flush();

            return writer.ToString();
        }
    }
}
