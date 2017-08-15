// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Publishing
{
    public partial class PublishingRestrictions
    {
        [NotNull]
        private readonly List<PublishingTarget> publishingTargets = new List<PublishingTarget>();

        [NotNull]
        private readonly List<Restriction> restrictions = new List<Restriction>();

        public PublishingRestrictions([NotNull] ItemVersionUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            InitializeComponent();
            this.InitializeDialog();

            ItemUri = itemUri;

            LoadItemRestrictions();
        }

        [NotNull]
        public ItemVersionUri ItemUri { get; }

        private void LoadItemRestrictions()
        {
            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                Loading.HideLoading(Tabs);

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                Parse(response);
                Refresh();
            };

            ItemUri.Site.DataService.ExecuteAsync("Publishing.GetPublishingRestrictions", completed, ItemUri.DatabaseName.ToString(), ItemUri.ItemId.ToString(), ItemUri.Language.ToString());
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Save();

            this.Close(true);
        }

        private void Parse([NotNull] string response)
        {
            Debug.ArgumentNotNull(response, nameof(response));

            restrictions.Clear();
            publishingTargets.Clear();

            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            ItemPublishable.IsChecked = root.GetAttributeValue("publishable") == @"1";
            ItemPublishFrom.Value = ToDateTime(root, "publishfrom");
            ItemPublishTo.Value = ToDateTime(root, "publishto");

            var versions = root.Element(@"versions");
            if (versions != null)
            {
                foreach (var element in versions.Elements())
                {
                    var restriction = new Restriction
                    {
                        Version = element.GetAttributeInt("number", 0),
                        Publishable = element.GetAttributeValue("publishable") == @"1",
                        PublishFrom = ToDateTime(element, "publishfrom"),
                        PublishTo = ToDateTime(element, "publishto"),
                    };

                    restrictions.Add(restriction);
                }
            }

            var targets = root.Element(@"targets");
            if (targets != null)
            {
                foreach (var element in targets.Elements())
                {
                    var publishingTarget = new PublishingTarget
                    {
                        Id = element.GetAttributeValue("id"),
                        Name = element.GetAttributeValue("name"),
                        IsSelected = element.GetAttributeValue("isselected") == @"1"
                    };

                    publishingTargets.Add(publishingTarget);
                }
            }
        }

        private void Refresh()
        {
            VersionsGrid.ItemsSource = null;
            VersionsGrid.ItemsSource = restrictions;

            foreach (var publishingTarget in publishingTargets)
            {
                var item = new ListBoxItem
                {
                    Content = publishingTarget.Name,
                    Tag = publishingTarget,
                    IsSelected = publishingTarget.IsSelected
                };

                PublishingTargets.Items.Add(item);
            }
        }

        private void Save()
        {
            var publishFrom = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Publishing/Publishing/__Publish");
            var publishTo = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Publishing/Publishing/__Unpublish");
            var neverPublish = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Publishing/Publishing/__Never publish");
            var validTo = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Lifetime/Lifetime/__Valid to");
            var validFrom = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Lifetime/Lifetime/__Valid from");
            var hideVersion = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Lifetime/Lifetime/__Hide version");
            var publishingGroups = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Publishing/Publishing/__Publishing groups");

            var fields = new List<Tuple<FieldUri, string>>();

            fields.Add(new Tuple<FieldUri, string>(new FieldUri(ItemUri, publishFrom), ToIsoDateTime(ItemPublishFrom.Value)));
            fields.Add(new Tuple<FieldUri, string>(new FieldUri(ItemUri, publishTo), ToIsoDateTime(ItemPublishTo.Value)));
            fields.Add(new Tuple<FieldUri, string>(new FieldUri(ItemUri, neverPublish), ItemPublishable.IsChecked == true ? string.Empty : @"1"));

            foreach (var restriction in restrictions)
            {
                var itemVersionUri = new ItemVersionUri(ItemUri.ItemUri, ItemUri.Language, new Data.Version(restriction.Version));

                fields.Add(new Tuple<FieldUri, string>(new FieldUri(itemVersionUri, validFrom), ToIsoDateTime(restriction.PublishFrom)));
                fields.Add(new Tuple<FieldUri, string>(new FieldUri(itemVersionUri, validTo), ToIsoDateTime(restriction.PublishTo)));
                fields.Add(new Tuple<FieldUri, string>(new FieldUri(itemVersionUri, hideVersion), restriction.Publishable ? string.Empty : @"1"));
            }

            var targets = string.Empty;
            foreach (var item in PublishingTargets.Items)
            {
                var listBoxItem = item as ListBoxItem;
                if (listBoxItem == null)
                {
                    continue;
                }

                if (!listBoxItem.IsSelected)
                {
                    continue;
                }

                var publishingTarget = listBoxItem.Tag as PublishingTarget;
                if (publishingTarget == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(targets))
                {
                    targets += @"|";
                }

                targets += publishingTarget.Id;
            }

            fields.Add(new Tuple<FieldUri, string>(new FieldUri(ItemUri, publishingGroups), targets));

            ItemModifier.Edit(ItemUri.DatabaseUri, fields);
        }

        private DateTime? ToDateTime([NotNull] XElement root, [Localizable(false), NotNull] string name)
        {
            Debug.ArgumentNotNull(root, nameof(root));
            Debug.ArgumentNotNull(name, nameof(name));

            var value = root.GetAttributeIsoDateTime(name, DateTime.MinValue);
            if (value == DateTime.MinValue)
            {
                return null;
            }

            return value;
        }

        [NotNull]
        private string ToIsoDateTime(DateTime? dateTime)
        {
            var value = DateTime.MinValue;

            if (dateTime != null)
            {
                value = (DateTime)dateTime;
            }

            return DateTimeExtensions.ToIsoDate(value);
        }

        public class PublishingTarget
        {
            [NotNull]
            public string Id { get; set; }

            public bool IsSelected { get; set; }

            [NotNull]
            public string Name { get; set; }
        }

        public class Restriction
        {
            public bool Publishable { get; set; }

            [CanBeNull]
            public DateTime? PublishFrom { get; set; }

            [NotNull, UsedImplicitly]
            public string PublishFromFormatted
            {
                get
                {
                    if (PublishFrom == null || PublishFrom == DateTime.MinValue)
                    {
                        return string.Empty;
                    }

                    var value = (DateTime)PublishFrom;

                    return value.ToString(CultureInfo.CurrentCulture);
                }
            }

            [CanBeNull]
            public DateTime? PublishTo { get; set; }

            [NotNull, UsedImplicitly]
            public string PublishToFormatted
            {
                get
                {
                    if (PublishTo == null || PublishTo == DateTime.MinValue)
                    {
                        return string.Empty;
                    }

                    var value = (DateTime)PublishTo;

                    return value.ToString(CultureInfo.CurrentCulture);
                }
            }

            public int Version { get; set; }
        }
    }
}
