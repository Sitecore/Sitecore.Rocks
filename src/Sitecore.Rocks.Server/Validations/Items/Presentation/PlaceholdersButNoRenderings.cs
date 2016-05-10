// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Layouts;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;

namespace Sitecore.Rocks.Server.Validations.Items.Presentation
{
    [Validation("Placeholders but no renderings", "Presentation")]
    public class PlaceholdersButNoRenderings : ItemValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            var layoutField = item.Fields[FieldIDs.LayoutField];
            if (layoutField.ContainsStandardValue)
            {
                return;
            }

            var value = GetFieldValuePipeline.Run().WithParameters(layoutField).Value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var layoutDefinition = LayoutDefinition.Parse(value);
            if (layoutDefinition == null)
            {
                return;
            }

            foreach (DeviceDefinition device in layoutDefinition.Devices)
            {
                var renderings = device.Renderings;
                if (renderings != null && renderings.Count > 0)
                {
                    continue;
                }

                var layoutId = device.Layout;
                if (string.IsNullOrEmpty(layoutId))
                {
                    continue;
                }

                var layoutItem = item.Database.GetItem(layoutId);
                if (layoutItem == null)
                {
                    continue;
                }

                var layoutFile = layoutItem["Path"];

                var path = FileUtil.MapPath(layoutFile);
                string source;

                try
                {
                    source = FileUtil.ReadFromFile(path);
                }
                catch
                {
                    continue;
                }

                var matches = Regex.Matches(source, "<[^>]*Placeholder[^>]*Key=\"([^\"]*)\"[^>]*>", RegexOptions.IgnoreCase);
                if (matches.Count == 0)
                {
                    continue;
                }

                var list = new List<string>();

                foreach (Match match in matches)
                {
                    list.Add(match.Groups[1].ToString());
                }

                var placeholders = string.Join(", ", list.ToArray());

                output.Write(SeverityLevel.Suggestion, "Placeholders but no renderings", string.Format("The layout \"{0}\" in contains the placeholder(s) \"{1}\" but no renderings are assigned. When the layout is rendered the placeholders will be empty.", layoutFile, placeholders), "Either remove the placeholder (if they are not used by any other item) or assign renderings.", item);
            }
        }
    }
}
