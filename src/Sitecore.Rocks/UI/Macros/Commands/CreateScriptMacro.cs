// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.UI.Macros.Commands
{
    /* [Command(Submenu = "Macros")] */

    public class CreateScriptMacro : CommandBase
    {
        public CreateScriptMacro()
        {
            Text = Resources.CreateScriptMacro_CreateScriptMacro_Create_Script_Macro;
            Group = "Edit";
            SortingValue = 5000;
        }

        public override bool CanExecute([CanBeNull] object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            return true;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            if (!context.Items.Any())
            {
                return;
            }

            var item = context.Items.First();

            GetValueCompleted<ItemHeader> c = delegate(ItemHeader itemHeader)
            {
                var queryAnalyzer = AppHost.Windows.Factory.OpenQueryAnalyzer(item.ItemUri.DatabaseUri);
                if (queryAnalyzer == null)
                {
                    Trace.TraceError("Could not open query analyzer");
                    return;
                }

                var script = string.Format(@"set contextnode = {0};", itemHeader.Path);

                queryAnalyzer.AppendScript(script);
                queryAnalyzer.ShowMacroScriptHelp();
            };

            item.ItemUri.Site.DataService.GetItemHeader(item.ItemUri, c);
        }
    }
}
