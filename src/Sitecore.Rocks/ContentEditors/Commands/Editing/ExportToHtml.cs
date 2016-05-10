// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Commands.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command(Submenu = "Tools"), Feature(FeatureNames.Exporting)]
    public class ExportToHtml : ReportCommand
    {
        public ExportToHtml()
        {
            Text = Resources.ExportToBrowser_ExportToBrowser_Export_to_Html;
            Group = "Exporting";
            SortingValue = 6000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ContentEditorContext;
        }

        protected override void GenerateReport(XmlTextWriter output, object parameter)
        {
            Debug.ArgumentNotNull(output, nameof(output));

            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            var contentModel = context.ContentEditor.ContentModel;

            WriteStartTable(output, "Item", "Field Name", "Field Values");

            foreach (var field in contentModel.Fields)
            {
                WriteRow(output, field.Name, field.Control != null ? field.Control.GetValue() : field.Value);
            }

            WriteEndTable(output);
        }
    }
}
