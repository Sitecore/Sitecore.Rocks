// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Languages
{
    public class GetLanguages
    {
        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("languages");

            output.WriteAttributeString("current", Context.Language.Name);

            foreach (var language in LanguageManager.GetLanguages(database))
            {
                output.WriteStartElement("language");
                output.WriteAttributeString("name", language.Name);
                output.WriteAttributeString("displayname", language.GetDisplayName());
                output.WriteAttributeString("itemid", LanguageManager.GetLanguageItemId(language, database).ToString());
                output.WriteAttributeString("icon", Images.GetThemedImageSource(language.GetIcon(database), ImageDimension.id16x16));
                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
