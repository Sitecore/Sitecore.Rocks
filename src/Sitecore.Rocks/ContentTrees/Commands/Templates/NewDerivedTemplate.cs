// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Templates
{
    [Command, CommandId(CommandIds.SitecoreExplorer.NewDerivedTemplate, typeof(ContentTreeContext))]
    public class NewDerivedTemplate : CommandBase
    {
        private static readonly FieldId baseTemplateFieldId = new FieldId(new Guid("{12C33F3F-86C5-43A5-AEB4-5598CEC45116}"));

        public NewDerivedTemplate()
        {
            Text = Resources.NewDerivedTemplate_NewDerivedTemplate_New_Derived_Template___;
            Group = "Templates";
            SortingValue = 200;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            var parent = item.Parent as ItemTreeViewItem;
            if (parent == null)
            {
                return false;
            }

            if (!item.IsTemplate)
            {
                return false;
            }

            if ((item.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.EditTemplate) != DataServiceFeatureCapabilities.EditTemplate)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var parent = item.Parent as ItemTreeViewItem;
            if (parent == null)
            {
                return;
            }

            var templateName = string.Format(Resources.NewDerivedTemplate_Execute_New__0_, item.Text);

            templateName = AppHost.Prompt(Resources.NewDerivedTemplate_Execute_Enter_the_Name_of_the_Derived_Template_, Resources.NewDerivedTemplate_Execute_Derived_Template, templateName);
            if (templateName == null)
            {
                return;
            }

            var parentUri = parent.ItemUri;

            // create template item
            var templateUri = new ItemUri(parent.ItemUri.DatabaseUri, IdManager.GetItemId("/sitecore/templates/System/Templates/Template"));

            var itemUri = parent.ItemUri.Site.DataService.AddFromTemplate(parentUri, templateUri, templateName);
            if (itemUri == ItemUri.Empty)
            {
                return;
            }

            // set "Base Template" field
            var baseTemplateField = new Field
            {
                Value = item.ItemUri.ItemId.ToString(),
                HasValue = true
            };
            baseTemplateField.FieldUris.Add(new FieldUri(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), baseTemplateFieldId));

            var fields = new List<Field>
            {
                baseTemplateField
            };

            itemUri.Site.DataService.Save(itemUri.DatabaseName, fields);

            // expand tree
            context.ContentTree.ExpandTo(itemUri);

            // design template
            AppHost.Windows.Factory.OpenTemplateDesigner(itemUri);

            Notifications.RaiseItemAdded(this, new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), parentUri);
        }
    }
}
