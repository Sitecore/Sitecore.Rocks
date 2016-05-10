// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("link"), FieldControl("general link")]
    public partial class LinkField : IReusableFieldControl, ISupportsXmlOperations
    {
        // <link text="Description" linktype="internal" url="/Home" anchor="Anchor" title="AltText" class="StyleClass" target="_blank" querystring="" id="{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}" />

        private string currentValue;

        public LinkField()
        {
            InitializeComponent();

            LastPath = string.Empty;

            Clear();
        }

        [NotNull]
        public string Anchor { get; set; }

        [NotNull]
        public string CssClass { get; set; }

        [NotNull]
        public string Description { get; set; }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        [NotNull, Localizable(false)]
        public string LinkType { get; set; }

        [NotNull]
        public string QueryString { get; set; }

        [CanBeNull]
        public Field SourceField { get; private set; }

        [NotNull]
        public string Target { get; set; }

        [NotNull]
        public string Title { get; set; }

        [NotNull]
        public string Url { get; set; }

        protected int DisableEvents { get; set; }

        [NotNull]
        private string LastPath { get; set; }

        public Control GetControl()
        {
            return this;
        }

        public Control GetFocusableControl()
        {
            return Link;
        }

        public string GetValue()
        {
            return currentValue;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void RaiseModified()
        {
            var modified = ValueModified;
            if (modified != null)
            {
                modified();
            }
        }

        public void Render()
        {
            DisableEvents++;

            Link.Text = ItemUri == ItemUri.Empty ? string.Empty : ItemUri.ItemId.ToString();

            switch (LinkType.ToLowerInvariant())
            {
                case "external":
                    LinkTypeExternal.IsSelected = true;
                    break;
                case "media":
                    LinkTypeMedia.IsSelected = true;
                    break;
                case "mailto":
                    LinkTypeMail.IsSelected = true;
                    break;
                case "javascript":
                    LinkTypeJavascript.IsSelected = true;
                    break;
                default:
                    LinkTypeInternal.IsSelected = true;
                    break;
            }

            switch (Target.ToLowerInvariant())
            {
                case "_blank":
                    TargetBlank.IsSelected = true;
                    break;
                case "_parent":
                    TargetParent.IsSelected = true;
                    break;
                case "_self":
                    TargetSelf.IsSelected = true;
                    break;
                case "_top":
                    TargetTop.IsSelected = true;
                    break;
                default:
                    TargetField.SelectedIndex = -1;
                    TargetField.Text = Target;
                    break;
            }

            CssClassField.Text = CssClass;
            TitleField.Text = Title;
            DescriptionField.Text = Description;
            AnchorField.Text = Anchor;
            UrlField.Text = Url;
            QueryStringField.Text = QueryString;

            DisableEvents--;

            UpdatePath(LastPath);
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            SourceField = sourceField;
            currentValue = sourceField.Value;

            ParseValue();
            Render();
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            if (currentValue == value)
            {
                return;
            }

            currentValue = value;

            ParseValue();
            Render();
            RaiseModified();
        }

        public void UnsetField()
        {
            SourceField = null;
        }

        public void UpdateValue()
        {
            var itemId = ItemUri == ItemUri.Empty ? string.Empty : ItemUri.ItemId.ToString();

            currentValue = string.Format(@"<link text=""{0}"" linktype=""{1}"" url=""{2}"" anchor=""{3}"" title=""{4}"" class=""{5}"" querystring=""{6}"" target=""{7}"" id=""{8}"" />", Description, LinkType, Url, Anchor, Title, CssClass, QueryString, Target, itemId);
        }

        public event ValueModifiedEventHandler ValueModified;

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var sourceField = SourceField;
            if (sourceField == null)
            {
                return;
            }

            var itemId = ItemUri.ItemId;
            if (itemId == ItemId.Empty)
            {
                itemId = IdManager.GetItemId("/sitecore/content");
            }

            var itemUri = new ItemUri(sourceField.FieldUris.First().ItemVersionUri.DatabaseUri, itemId);

            var dialog = new SelectItemDialog();

            dialog.Initialize(Rocks.Resources.Browse, itemUri);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            itemUri = dialog.SelectedItemUri;
            if (itemUri == ItemUri.Empty)
            {
                return;
            }

            ItemUri = itemUri;
            LinkType = @"internal";

            var selectedItemTemplateId = dialog.SelectedItemTemplateId;
            if (selectedItemTemplateId != null && IdManager.GetTemplateType(selectedItemTemplateId) == "media")
            {
                LinkType = @"media";
            }

            UpdateValue();
            RaiseModified();
            Render();
        }

        private void Clear()
        {
            ItemUri = ItemUri.Empty;
            Description = string.Empty;
            LinkType = string.Empty;
            Url = string.Empty;
            QueryString = string.Empty;
            Anchor = string.Empty;
            Title = string.Empty;
            CssClass = string.Empty;
            Target = string.Empty;
        }

        private void Commit()
        {
            if (LinkTypeExternal.IsSelected)
            {
                LinkType = @"external";
            }
            else if (LinkTypeMedia.IsSelected)
            {
                LinkType = @"media";
            }
            else if (LinkTypeMail.IsSelected)
            {
                LinkType = @"mail";
            }
            else if (LinkTypeJavascript.IsSelected)
            {
                LinkType = @"javascript";
            }
            else
            {
                LinkType = @"internal";
            }

            switch (TargetField.SelectedIndex)
            {
                case 0:
                    Target = @"_blank";
                    break;
                case 1:
                    Target = @"_parent";
                    break;
                case 2:
                    Target = @"_self";
                    break;
                case 3:
                    Target = @"_top";
                    break;
                default:
                    Target = TargetField.Text;
                    break;
            }

            CssClass = CssClassField.Text;
            Title = TitleField.Text;
            Description = DescriptionField.Text;
            Anchor = AnchorField.Text;
            Url = UrlField.Text;
            QueryString = QueryStringField.Text;
        }

        private void HandleComboBoxChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (DisableEvents > 0)
            {
                return;
            }

            Commit();
            RaiseModified();
            UpdateValue();
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            var baseItems = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (baseItems == null)
            {
                return;
            }

            if (baseItems.Count() != 1)
            {
                return;
            }

            e.Effects = DragDropEffects.Copy;
        }

        private void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var items = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (items == null)
            {
                return;
            }

            if (items.Count() != 1)
            {
                return;
            }

            var item = items.First();
            if (item == null)
            {
                return;
            }

            ItemUri = item.ItemUri;
            LinkType = @"internal";

            var templatedItem = item as ITemplatedItem;
            if (templatedItem != null)
            {
                if (IdManager.GetTemplateType(templatedItem.TemplateId) == "media")
                {
                    LinkType = @"media";
                }
            }

            UpdateValue();
            RaiseModified();
            Render();
        }

        private void HandleTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (DisableEvents > 0)
            {
                return;
            }

            Commit();
            RaiseModified();
            UpdateValue();
            UpdatePath(LastPath);
        }

        private bool HasValues()
        {
            if (!string.IsNullOrEmpty(Anchor))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(CssClass))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(Description))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(LinkType) && LinkType != "internal")
            {
                return true;
            }

            if (!string.IsNullOrEmpty(QueryString))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(Target))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(Title))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(Url))
            {
                return true;
            }

            return false;
        }

        private void ParseValue()
        {
            Clear();
            UpdatePath(string.Empty);

            XDocument doc;
            try
            {
                doc = XDocument.Parse(currentValue);
            }
            catch
            {
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            ItemUri = ItemUri.Empty;

            var id = root.GetAttributeValue("id");
            if (!string.IsNullOrEmpty(id) && SourceField != null)
            {
                Guid guid;
                if (Guid.TryParse(id, out guid))
                {
                    ItemUri = new ItemUri(SourceField.FieldUris.First().ItemVersionUri.DatabaseUri, new ItemId(guid));
                }
            }

            Description = root.GetAttributeValue("text");
            LinkType = root.GetAttributeValue("linktype");
            Url = root.GetAttributeValue("url");
            QueryString = root.GetAttributeValue("querystring");
            Anchor = root.GetAttributeValue("anchor");
            Title = root.GetAttributeValue("title");
            CssClass = root.GetAttributeValue("class");
            Target = root.GetAttributeValue("target");
        }

        private void TextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }

            var sourceField = SourceField;
            if (sourceField == null)
            {
                return;
            }

            var fieldUri = sourceField.FieldUris.FirstOrDefault();
            if (fieldUri == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(Link.Text))
            {
                UpdatePath(string.Empty);
                return;
            }

            UpdatePath("[Updating path]");

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result, true))
                {
                    UpdatePath("[Item not found]");
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(fieldUri.DatabaseUri, element);

                UpdatePath(itemHeader.Path);
            };

            fieldUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, Link.Text, fieldUri.DatabaseName.Name);
        }

        private void ToggleDetails([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Details.Visibility = Details.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            ToggleDetailsRun.Text = Details.Visibility == Visibility.Collapsed ? "More" : "Less";
        }

        private void UpdatePath([NotNull] string text)
        {
            LastPath = text;
            var url = UrlField.Text;

            if (string.IsNullOrEmpty(text))
            {
                if (!string.IsNullOrEmpty(url))
                {
                    PathTextBlock.Text = "Url: " + url;
                }
                else
                {
                    PathTextBlock.Text = "[Empty]";
                }

                return;
            }

            PathTextBlock.Text = text;
        }
    }
}
