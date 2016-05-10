// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectTemplatesDialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Templates
{
    [Command, CommandId(CommandIds.SitecoreExplorer.NewTemplate, typeof(ContentTreeContext))]
    public class NewTemplate : CommandBase
    {
        private static readonly FieldId BaseTemplateFieldId = new FieldId(new Guid("{12C33F3F-86C5-43A5-AEB4-5598CEC45116}"));

        public NewTemplate()
        {
            Text = Resources.NewTemplate_NewTemplate_New_Template___;
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

            if (IdManager.GetTemplateType(item.Item.TemplateId) != "folder")
            {
                return false;
            }

            if (item.Item.TemplateId != IdManager.GetItemId("/sitecore/templates/System/Templates/Template Folder"))
            {
                var path = item.GetPath();
                if (!path.Contains(@"/sitecore/templates/"))
                {
                    return false;
                }
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
                AppHost.MessageBox(Resources.NewTemplate_Execute_, Resources.NewTemplate_Execute_Create_New_Template, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                AppHost.MessageBox(Resources.NewTemplate_Execute_There_is_no_active_item_, Resources.NewTemplate_Execute_Create_New_Template, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // get template name
            var dialog = new NewTemplateDialog();
            dialog.Initialize("New Template", item.ItemUri.DatabaseUri, new[]
            {
                IdManager.GetItemId("/sitecore/templates/System/Templates/Standard template")
            });
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            // create template item
            var templateName = dialog.ItemName;
            var templateUri = new ItemUri(item.ItemUri.DatabaseUri, IdManager.GetItemId("/sitecore/templates/System/Templates/Template"));

            var itemUri = item.ItemUri.Site.DataService.AddFromTemplate(item.ItemUri, templateUri, templateName);
            if (itemUri == ItemUri.Empty)
            {
                AppHost.MessageBox(Resources.NewTemplate_Execute_Failed_to_create_the_template_, Resources.NewTemplate_Execute_Create_New_Template, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var baseTemplates = string.Join("|", dialog.SelectedTemplates.Select(i => i.ToString()));
            if (string.IsNullOrEmpty(baseTemplates))
            {
                baseTemplates = IdManager.GetItemId("/sitecore/templates/System/Templates/Standard template").ToString();
            }

            // set "Base Template" field
            var baseTemplateField = new Field
            {
                Value = baseTemplates,
                HasValue = true
            };

            baseTemplateField.FieldUris.Add(new FieldUri(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), BaseTemplateFieldId));

            var fields = new List<Field>
            {
                baseTemplateField
            };

            itemUri.Site.DataService.Save(itemUri.DatabaseName, fields);

            if (dialog.CreateStandardValues)
            {
                CreateStandardValues(itemUri);
            }

            // expand tree
            context.ContentTree.ExpandTo(itemUri);

            // design template
            AppHost.Windows.Factory.OpenTemplateDesigner(itemUri);

            Notifications.RaiseItemAdded(this, new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), item.ItemUri);
        }

        private void CreateStandardValues(ItemUri templateUri)
        {
            var standardValuesItemUri = templateUri.Site.DataService.AddFromTemplate(templateUri, templateUri, @"__Standard Values");
            if (standardValuesItemUri == ItemUri.Empty)
            {
                return;
            }

            var standardValuesFieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Advanced/Advanced/__Standard values");
            var standardValuesField = new Field
            {
                Value = standardValuesItemUri.ItemId.ToString(),
                HasValue = true
            };

            standardValuesField.FieldUris.Add(new FieldUri(new ItemVersionUri(templateUri, LanguageManager.CurrentLanguage, Data.Version.Latest), standardValuesFieldId));

            var fields = new List<Field>
            {
                standardValuesField
            };

            ItemModifier.Edit(standardValuesItemUri.DatabaseUri, fields, true);
        }
    }
}
