// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Scripts
{
    [Command(Submenu = "Script"), CommandId(CommandIds.SitecoreExplorer.ScriptUpdate, typeof(ContentTreeContext), Text = "Script Update"), Feature(FeatureNames.Scripting)]
    public class Update : ScriptItemsCommand
    {
        private readonly List<string> ignore;

        public Update()
        {
            Text = Resources.Update_Update_Update;
            Group = "Standard";
            SortingValue = 1100;

            ignore = new List<string>
            {
                @"__updated by",
                @"__updated",
                @"__created",
                @"__created by",
                @"__revision",
            };
        }

        protected override string GetScript(List<Item> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var output = new StringWriter();

            foreach (var item in items)
            {
                Render(output, item);
            }

            return output.ToString();
        }

        private void Render([NotNull] StringWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var fields = item.Fields.Where(field => field != null && !string.IsNullOrEmpty(field.Value)).ToList();

            output.WriteLine(@"update set");

            for (var index = fields.Count - 1; index >= 0; index--)
            {
                var field = fields[index];
                if (ignore.Contains(field.Name.ToLowerInvariant()))
                {
                    fields.RemoveAt(index);
                }
            }

            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                output.WriteLine(@"  @#{0}# = '{1}'{2}", field.Name, field.Value, index < fields.Count - 1 ? @"," : string.Empty);
            }

            output.WriteLine(@"from {0};", FormatPath(item.GetPath()));
        }
    }
}
