// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Commandy.Commands
{
    public class SetBaseTemplate : CommandBase
    {
        public SetBaseTemplate([NotNull] TemplateHeader templateHeader)
        {
            TemplateHeader = templateHeader;
            Assert.ArgumentNotNull(templateHeader, nameof(templateHeader));

            Text = "Set Base Template " + templateHeader.Name;
            Group = "Template";
            SortingValue = 1000;
        }

        [NotNull]
        protected TemplateHeader TemplateHeader { get; }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var templateId = TemplateHeader.TemplateUri.ItemId.ToString();
            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Template/Data/__Base template");

            foreach (var item in context.Items)
            {
                ItemModifier.Edit(item.ItemUri, fieldId, templateId);
            }
        }
    }
}
