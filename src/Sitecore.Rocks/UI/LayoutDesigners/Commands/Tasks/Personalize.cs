// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.RuleEditors;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4010, "Rendering", "Tasks", Icon = "Resources/32x32/Users-Details-2.png", Text = "Personalize")]
    public class Personalize : CommandBase, IToolbarElement
    {
        public Personalize()
        {
            Text = "Personalize...";
            Group = "Personalize";
            SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            return context.SelectedItems.Count() == 1 && context.SelectedItems.All(r => r is RenderingItem);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var renderingItem = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            var ruleset = renderingItem.Ruleset;
            if (string.IsNullOrEmpty(ruleset) || ruleset.Contains("<ruleset />"))
            {
                ruleset = "<ruleset><rule /></ruleset>";
            }

            var root = ruleset.ToXElement();
            if (root == null)
            {
                return;
            }

            var dataSource = string.Empty;
            if (renderingItem.ItemUri.Site.SitecoreVersion >= Constants.Versions.Version7)
            {
                // /sitecore/system/Settings/Rules/Conditional Renderings
                dataSource = "{C3D3CAAF-1F07-43FB-B8FF-3EC6D712262C}";
            }

            var ruleModel = new RuleModel(root);

            var dialog = new RuleEditorDialog();
            dialog.Initialize(renderingItem.ItemUri.DatabaseUri, dataSource, ruleModel);

            if (AppHost.Shell.ShowDialog(dialog) == true)
            {
                renderingItem.Ruleset = ruleModel.ToString();
            }
        }
    }
}
