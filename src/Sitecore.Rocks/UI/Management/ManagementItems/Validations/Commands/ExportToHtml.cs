// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Commands.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class ExportToHtml : ReportCommand
    {
        public ExportToHtml()
        {
            Text = Resources.ExportToBrowser_ExportToBrowser_Export_to_Html;
            Group = "Export";
            SortingValue = 2300;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Validations.Any())
            {
                return false;
            }

            return true;
        }

        protected override void GenerateReport(XmlTextWriter output, object parameter)
        {
            Debug.ArgumentNotNull(output, nameof(output));

            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            var hiddenItems = AppHost.Settings.Get("Management\\Validation\\Hidden", context.ValidationViewer.Context.Site.Name, string.Empty) as string ?? string.Empty;

            WriteStartTable(output, "Validation", "Severity", "TItle", "Problem", "Solution", "Item");

            foreach (var entry in context.Validations.OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Title))
            {
                var key = entry.GetKey();
                if (!hiddenItems.Contains(key))
                {
                    WriteRow(output, entry.Severity.ToString(), entry.Title, entry.Problem, entry.Solution, entry.ItemPath);
                }
            }

            WriteEndTable(output);
        }
    }
}
