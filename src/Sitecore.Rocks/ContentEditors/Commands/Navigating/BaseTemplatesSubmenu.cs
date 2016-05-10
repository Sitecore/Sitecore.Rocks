// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Navigating
{
    [Command(ExcludeFromSearch = true), CommandId(CommandIds.ItemEditor.NavigateBaseTemplates, typeof(ContentEditorContext), Text = "Navigate to Base Templates")]
    public class BaseTemplatesSubmenu : CommandBase, IToolbarElement
    {
        public BaseTemplatesSubmenu()
        {
            Text = Resources.Base_Templates;
            Group = "Templates";
            SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is QuickInfo;
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as QuickInfo;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var result = new List<ICommand>();

            result.AddRange(CommandManager.GetCommands(parameter, "Base Templates"));

            if (context.ContentEditor.ContentModel.IsMultiple)
            {
                return result;
            }

            var item = context.ContentEditor.ContentModel.FirstItem;

            foreach (var baseTemplate in item.BaseTemplates)
            {
                var command = new NavigateToBaseTemplate(baseTemplate.ItemUri)
                {
                    Text = baseTemplate.Name
                };

                result.Add(command);
            }

            return result;
        }
    }
}
