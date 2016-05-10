// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class GetTemplates
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, string includeBranches)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("templates");

            var templates = database.Templates.GetTemplates(LanguageManager.DefaultLanguage);
            if (templates == null)
            {
                throw new InvalidOperationException("No templates found. Is the default language set correctly?");
            }

            foreach (var template in templates.OrderBy(t => t.InnerItem.Parent.Name).ThenBy(t => t.Name))
            {
                var section = template.InnerItem.Parent.Name;
                var path = template.InnerItem.Paths.Path;

                output.WriteStartElement("template");

                output.WriteAttributeString("id", template.ID.ToString());
                output.WriteAttributeString("icon", Images.GetThemedImageSource(template.Icon, ImageDimension.id16x16));
                output.WriteAttributeString("section", section);
                output.WriteAttributeString("path", path);

                output.WriteValue(template.Name);

                output.WriteEndElement();
            }

            if (string.Compare(includeBranches, "true", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                var branches = database.SelectItems("fast://*[@@templateid='" + TemplateIDs.BranchTemplate + "']");

                foreach (var branch in branches.OrderBy(t => t.Parent.Name).ThenBy(t => t.Name))
                {
                    var section = branch.Parent.Name;
                    var path = branch.Paths.Path;

                    output.WriteStartElement("branch");

                    output.WriteAttributeString("id", branch.ID.ToString());
                    output.WriteAttributeString("icon", Images.GetThemedImageSource(branch.Appearance.Icon, ImageDimension.id16x16));
                    output.WriteAttributeString("section", section);
                    output.WriteAttributeString("path", path);

                    output.WriteValue(branch.Name);

                    output.WriteEndElement();
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        private void GetBranches(List<BranchItem> branches, Item item)
        {
            foreach (Item child in item.Children)
            {
                if (child.TemplateID == TemplateIDs.BranchTemplate)
                {
                    branches.Add(new BranchItem(child));
                }
                else
                {
                    GetBranches(branches, child);
                }
            }
        }
    }
}
