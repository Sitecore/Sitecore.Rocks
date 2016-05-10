// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes
{
    public abstract class HelpFix : IFix
    {
        public abstract bool CanFix(ValidationDescriptor validationDescriptor);

        public abstract void Fix(ValidationDescriptor validationDescriptor);

        protected void SetHelp(ItemVersionUri itemUri)
        {
            var item = itemUri.Site.DataService.GetItemFields(itemUri);

            var shortHelpField = item.Fields.FirstOrDefault(f => f != null && f.Name == "__Short description");
            var longHelpField = item.Fields.FirstOrDefault(f => f != null && f.Name == "__Long description");

            var shortHelp = shortHelpField != null ? shortHelpField.Value : string.Empty;
            var longHelp = longHelpField != null ? longHelpField.Value : string.Empty;

            var d = new SetHelpDialog();

            d.Initialize(shortHelp, longHelp);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var shortProperty = d.GetType().GetField("ShortHelp", BindingFlags.NonPublic | BindingFlags.Instance);
            var shortHelpTextBox = shortProperty.GetValue(d) as TextBox;
            if (shortHelpTextBox != null)
            {
                shortHelp = shortHelpTextBox.Text ?? string.Empty;
            }

            var longProperty = d.GetType().GetField("LongHelp", BindingFlags.NonPublic | BindingFlags.Instance);
            var longHelpTextBox = longProperty.GetValue(d) as TextBox;
            if (longHelpTextBox != null)
            {
                longHelp = longHelpTextBox.Text ?? string.Empty;
            }

            ItemModifier.Edit(itemUri, "__Short description", shortHelp);
            ItemModifier.Edit(itemUri, "__Long description", longHelp);
        }
    }
}
