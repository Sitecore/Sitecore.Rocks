// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("tracking")]
    public partial class TrackingField : IReusableFieldControl
    {
        private string myvalue;

        public TrackingField()
        {
            InitializeComponent();
        }

        public Control GetFocusableControl()
        {
            return null;
        }

        public string GetValue()
        {
            return myvalue;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            myvalue = sourceField.Value;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            TreeView.Items.Clear();

            if (!value.Equals(string.Empty))
            {
                DataNotSpecifiedNotification.Hide();
            }

            var changed = myvalue != value;
            myvalue = value;

            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var tracking = XElement.Parse(value);

                    DrawProfiles(from p in tracking.Descendants(@"profile")
                        select p);
                    DrawEvents(from e in tracking.Descendants(@"event")
                        select e);
                }
                catch (Exception e)
                {
                    MainBorder.IsEnabled = false;
                    DataNotSpecifiedNotification.Hide();
                    ParseErrorNotification.Initialize(e);
                    ParseErrorNotification.Show();
                }
            }

            if (!changed)
            {
                return;
            }

            var modified = ValueModified;
            if (modified != null)
            {
                modified();
            }
        }

        public void UnsetField()
        {
            OutdatedNotification.Hide();
            DataNotSpecifiedNotification.Hide();
            ParseErrorNotification.Hide();
            MainBorder.IsEnabled = true;
        }

        public event ValueModifiedEventHandler ValueModified;

        private void DrawEvents([NotNull] IEnumerable<XElement> events)
        {
            Debug.ArgumentNotNull(events, nameof(events));

            if (!events.Any())
            {
                return;
            }

            var tviEvents = GetLevel0Item(Rocks.Resources.TrackingField_DrawEvents_Events);
            foreach (var e in events)
            {
                tviEvents.Items.Add(new TreeViewItem
                {
                    Header = e.Attributes(@"name").First().Value
                });
            }

            TreeView.Items.Add(tviEvents);
        }

        private void DrawProfiles([NotNull] IEnumerable<XElement> profiles)
        {
            Debug.ArgumentNotNull(profiles, nameof(profiles));

            if (!profiles.Any())
            {
                return;
            }

            var tviProfiles = GetLevel0Item(Rocks.Resources.TrackingField_DrawProfiles_Profiles);
            foreach (var p in profiles)
            {
                var tviProfile = GetLevel1Item(p.Attributes(@"name").First().Value);
                var keys = from k in p.Descendants(@"key")
                    select new
                    {
                        Name = k.Attributes(@"name").First().Value,
                        k.Attributes(@"value").First().Value
                    };
                if (keys.Any())
                {
                    foreach (var k in keys)
                    {
                        tviProfile.Items.Add(GetExpandedItem(string.Concat(k.Name, @": ", k.Value)));
                    }
                }

                tviProfiles.Items.Add(tviProfile);
            }

            TreeView.Items.Add(tviProfiles);
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        [NotNull]
        private static TreeViewItem GetExpandedItem([NotNull] string header)
        {
            Debug.ArgumentNotNull(header, nameof(header));

            return new TreeViewItem
            {
                IsExpanded = true,
                Header = header
            };
        }

        [NotNull]
        private TreeViewItem GetLevel0Item([NotNull] string header)
        {
            Debug.ArgumentNotNull(header, nameof(header));

            var trv = GetExpandedItem(header);
            trv.HeaderTemplate = Resources[@"dtLevel0"] as DataTemplate;
            trv.Margin = new Thickness(0, 5, 0, 5);
            return trv;
        }

        [NotNull]
        private TreeViewItem GetLevel1Item([NotNull] string header)
        {
            Debug.ArgumentNotNull(header, nameof(header));

            var trv = GetExpandedItem(header);
            trv.HeaderTemplate = Resources[@"dtLevel1"] as DataTemplate;
            return trv;
        }
    }
}
