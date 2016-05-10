// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Languages
{
    [Command(ExcludeFromSearch = true), ToolbarElement(typeof(ContentEditorContext), 1500, "Home", "Versions", Icon = "Resources/32x32/flag_generic.png", ElementType = RibbonElementType.LargeDropDown)]
    public class LanguagesSubmenu : CommandBase, IToolbarElement
    {
        public LanguagesSubmenu()
        {
            Text = Resources.Languages;
            Group = "Versions";
            SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            if (context.ContentEditor.ContentModel.IsEmpty)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var result = new List<ICommand>(CommandManager.GetCommands(parameter, "Languages"));

            if (!context.ContentEditor.ContentModel.IsSingle)
            {
                return result;
            }

            var item = context.ContentEditor.ContentModel.FirstItem;

            var selectedLanguage = item.Uri.Language;

            foreach (var languageName in item.Languages)
            {
                var language = new Language(languageName);

                var isChecked = language == selectedLanguage;

                var command = new SetLanguage(language)
                {
                    Text = languageName,
                    IsChecked = isChecked
                };

                result.Add(command);
            }

            return result;
        }
    }
}
