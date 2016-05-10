// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Scripts
{
    [Command(Submenu = "Script"), CommandId(CommandIds.SitecoreExplorer.ScriptDelete, typeof(ContentTreeContext), Text = "Script Delete"), Feature(FeatureNames.Scripting)]
    public class Delete : ScriptCommand
    {
        public Delete()
        {
            Text = Resources.ScriptDelete_Delete_Delete;
            Group = "Standard";
            SortingValue = 1300;
        }

        protected override string GetScript(List<IItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var output = new StringWriter();

            foreach (var item in items)
            {
                Render(output, item);
            }

            return output.ToString();
        }

        private void Render([NotNull] StringWriter output, [NotNull] IItem item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteLine(@"delete from //*[@@id='{0}'];", item.ItemUri.ItemId);
        }
    }
}
