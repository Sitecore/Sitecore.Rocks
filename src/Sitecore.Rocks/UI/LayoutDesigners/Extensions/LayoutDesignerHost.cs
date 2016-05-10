// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using TaskDialogInterop;

namespace Sitecore.Rocks.UI.LayoutDesigners.Extensions
{
    public class LayoutDesignerHost
    {
        public void Open([NotNull] string document, [NotNull] Item item, bool confirmStandardValues = true)
        {
            Assert.ArgumentNotNull(document, nameof(document));
            Assert.ArgumentNotNull(item, nameof(item));

            var field = item.Fields.FirstOrDefault(f => f.Name == @"__Renderings");
            if (field == null)
            {
                return;
            }

            var layout = field.Value;

            if (confirmStandardValues && field.StandardValue && !string.IsNullOrEmpty(layout))
            {
                SelectItemToEdit(document, item.Name, field, item);
                return;
            }

            Open(document, item.Name, field, layout);
        }

        private void CopyStandardValuesLayout([NotNull] string document, [NotNull] string paneCaption, [NotNull] Field field, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(document, nameof(document));
            Debug.ArgumentNotNull(paneCaption, nameof(paneCaption));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(item, nameof(item));

            GetValueCompleted<Item> completed = delegate(Item standardValueItem)
            {
                if (standardValueItem == Item.Empty)
                {
                    return;
                }

                var standardValueField = item.Fields.FirstOrDefault(f => f.Name == @"__Renderings");
                if (standardValueField == null)
                {
                    return;
                }

                Open(document, paneCaption, field, standardValueField.Value);
            };

            item.ItemUri.Site.DataService.GetItemFieldsAsync(new ItemVersionUri(new ItemUri(item.ItemUri.DatabaseUri, item.StandardValuesId), Language.Current, Version.Latest), completed);
        }

        private void EditStandardValues([NotNull] string document, [NotNull] string paneCaption, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(document, nameof(document));
            Debug.ArgumentNotNull(paneCaption, nameof(paneCaption));
            Debug.ArgumentNotNull(item, nameof(item));

            GetValueCompleted<Item> completed = delegate(Item standardValueItem)
            {
                if (standardValueItem == Item.Empty)
                {
                    return;
                }

                var standardValueField = standardValueItem.Fields.FirstOrDefault(f => f.Name == @"__Renderings");
                if (standardValueField == null)
                {
                    return;
                }

                document = "PageDesigner" + standardValueItem.ItemUri;

                Open(document, standardValueField.Name, standardValueField, standardValueField.Value);
            };

            item.ItemUri.Site.DataService.GetItemFieldsAsync(new ItemVersionUri(new ItemUri(item.ItemUri.DatabaseUri, item.StandardValuesId), Language.Current, Version.Latest), completed);
        }

        private void Open([NotNull] string document, [NotNull] string paneCaption, [NotNull] Field field, [NotNull] string layout)
        {
            Debug.ArgumentNotNull(document, nameof(document));
            Debug.ArgumentNotNull(paneCaption, nameof(paneCaption));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(layout, nameof(layout));

            var designer = AppHost.OpenDocumentWindow<LayoutDesigner>(document);
            if (designer != null)
            {
                designer.Initialize(paneCaption, field.FieldUris, layout);
            }
        }

        private void SelectItemToEdit([NotNull] string document, [NotNull] string paneCaption, [NotNull] Field field, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(document, nameof(document));
            Debug.ArgumentNotNull(paneCaption, nameof(paneCaption));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(item, nameof(item));

            var options = new TaskDialogOptions
            {
                Title = "Design Layout",
                CommonButtons = TaskDialogCommonButtons.None,
                MainInstruction = "The layout on this item is inherited from the Standard Values item.",
                MainIcon = VistaTaskDialogIcon.Information,
                DefaultButtonIndex = 0,
                CommandButtons = new[]
                {
                    "Edit the layout on the Standard Values item",
                    "Copy and edit the layout from the Standard Value item (if any) to this item",
                    "Edit a blank layout on this item"
                },
                AllowDialogCancellation = true
            };

            var r = TaskDialog.Show(options).CommandButtonResult;
            if (r == null)
            {
                return;
            }

            switch (r)
            {
                case 0:
                    EditStandardValues(document, paneCaption, item);
                    break;
                case 1:
                    CopyStandardValuesLayout(document, paneCaption, field, item);
                    break;
                case 2:
                    Open(document, paneCaption, field, field.Value);
                    break;
            }
        }
    }
}
