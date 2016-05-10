// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command(Submenu = SpeakSubmenu.Name)]
    public class CreateParametersTemplate : CommandBase
    {
        public static readonly FieldId BaseTemplateFieldId = new FieldId(new Guid("{12C33F3F-86C5-43A5-AEB4-5598CEC45116}"));

        public static readonly ItemId ControlBaseItemId = new ItemId(new Guid("{6A9A5956-5354-48B4-A25A-DABABA738424}"));

        public static readonly FieldId DataSourceLocation = new FieldId(new Guid("{B5B27AF1-25EF-405C-87CE-369B3A004016}"));

        public static readonly FieldId DataSourceTemplate = new FieldId(new Guid("{1A7C85E5-DC0B-490D-9187-BB1DBCB4C72F}"));

        public static readonly ItemId ViewRenderingId = new ItemId(new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}"));

        public CreateParametersTemplate()
        {
            Text = "Create Parameters Template";
            Group = "Renderings";
            SortingValue = 50;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var item = context.Items.FirstOrDefault() as ITemplatedItem;
            if (item == null)
            {
                return false;
            }

            if (!item.ItemUri.Site.IsSitecoreVersion(SitecoreVersion.Version70))
            {
                return false;
            }

            if (item.TemplateId != ViewRenderingId)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            var parentUri = item.ItemUri;
            var templateUri = new ItemUri(item.ItemUri.DatabaseUri, IdManager.GetItemId("/sitecore/templates/System/Templates/Template"));

            var parameterTemplateUri = parentUri.Site.DataService.AddFromTemplate(parentUri, templateUri, item.Name + @"-Parameters");
            if (parameterTemplateUri == ItemUri.Empty)
            {
                return;
            }

            AppHost.Server.UpdateItem(parameterTemplateUri, BaseTemplateFieldId, ControlBaseItemId.ToString());

            var parameterTemplateFieldId = new FieldId(new Guid("{7D24E54F-5C16-4314-90C9-6051AA1A7DA1}"));

            var fields = new List<Tuple<FieldId, string>>
            {
                new Tuple<FieldId, string>(parameterTemplateFieldId, parameterTemplateUri.ItemId.ToString()),
                new Tuple<FieldId, string>(DataSourceLocation, "PageSettings"),
                new Tuple<FieldId, string>(DataSourceTemplate, parameterTemplateUri.ItemId.ToString())
            };

            AppHost.Server.UpdateItem(parentUri, fields);

            AppHost.Windows.OpenTemplateDesigner(parameterTemplateUri);

            Notifications.RaiseItemAdded(this, new ItemVersionUri(parameterTemplateUri, LanguageManager.CurrentLanguage, Data.Version.Latest), parentUri);

            var activeContentTree = ActiveContext.ActiveContentTree;
            if (item is ItemTreeViewItem && activeContentTree != null)
            {
                var treeViewItem = activeContentTree.ContentTreeView.ExpandTo(parameterTemplateUri);
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = true;
                }
            }
        }
    }
}
