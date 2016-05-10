// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor.Gutters;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetGutters
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

            var selected = GutterManager.GetActiveRendererIDs();

            var root = database.GetItem("/sitecore/content/Applications/Content Editor/Gutters");
            if (root == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            foreach (Item child in root.Children)
            {
                result.Append(child.ID);

                result.Append(selected.Contains(child.ID) ? "1" : "0");

                result.AppendLine(child.DisplayName);
            }

            return result.ToString();
        }
    }
}
