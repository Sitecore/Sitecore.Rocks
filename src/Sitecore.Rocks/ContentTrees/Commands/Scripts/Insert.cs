// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Scripts
{
    [Command(Submenu = "Script"), CommandId(CommandIds.SitecoreExplorer.ScriptInsert, typeof(ContentTreeContext), Text = "Script Insert"), Feature(FeatureNames.Scripting)]
    public class Insert : ScriptItemsCommand
    {
        private readonly List<string> ignore;

        public Insert()
        {
            Text = Resources.InsertScript_Insert_Insert;
            Group = "Standard";
            SortingValue = 1200;

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

            var path = item.GetPath();
            var n = path.LastIndexOf('/');
            if (n >= 0)
            {
                path = path.Left(n);
            }

            var fields = item.Fields.Where(field => field != null && !string.IsNullOrEmpty(field.Value) && !ignore.Contains(field.Name.ToLowerInvariant())).ToList();

            for (var index = fields.Count - 1; index >= 0; index--)
            {
                var field = fields[index];
                if (ignore.Contains(field.Name.ToLowerInvariant()))
                {
                    fields.RemoveAt(index);
                }
            }

            output.WriteLine(@"insert into (");
            output.WriteLine(@"  @@itemname,");
            output.WriteLine(@"  @@templateitem,");
            output.WriteLine(@"  @@path{0}", fields.Any() ? @"," : string.Empty);

            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                output.WriteLine(@"  @#{0}#{1}", field.Name, index < fields.Count - 1 ? @"," : string.Empty);
            }

            output.WriteLine(@") values (");
            output.WriteLine(@"  '{0}',", item.Name);
            output.WriteLine(@"  //*[@@id='{0}'],", item.TemplateId);
            output.WriteLine(@"  {0}{1}", FormatPath(path), fields.Any() ? @"," : string.Empty);

            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                output.WriteLine(@"  '{0}'{1}", field.Value, index < fields.Count - 1 ? @"," : string.Empty);
            }

            output.WriteLine(@");");
        }
    }
}
