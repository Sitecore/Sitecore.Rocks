// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("layout")]
    public partial class LayoutField : IReusableFieldControl
    {
        private readonly StackSettings deviceStackSettings;

        private bool justSetField;

        private Field layoutSourceField;

        private string layoutValue;

        public LayoutField()
        {
            InitializeComponent();

            var renderStackSettings = new StackSettings(Orientation.Vertical, Orientation.Horizontal, 16, @"Controls", Resources[@"stlRenderStack"] as Style, Resources[@"stlRenderStackItemBorder"] as Style, Resources[@"stlRenderStackHeaderBorder"] as Style, Resources[@"ctlRenderHeader"] as ControlTemplate);
            var layoutStackSettings = new StackSettings(Orientation.Horizontal, Orientation.Horizontal, 16, null, Resources[@"stlLayoutStack"] as Style, Resources[@"stlLayoutStackItemBorder"] as Style, Resources[@"stlLayoutStackHeaderBorder"] as Style, Resources[@"ctlLayoutHeader"] as ControlTemplate, LayoutStruct.Node.Rendering, renderStackSettings);
            deviceStackSettings = new StackSettings(Orientation.Vertical, Orientation.Vertical, 32, null, Resources[@"stlDeviceStack"] as Style, Resources[@"stlDeviceStackItemBorder"] as Style, Resources[@"stlDeviceStackHeaderBorder"] as Style, Resources[@"ctlDeviceHeader"] as ControlTemplate, LayoutStruct.Node.Layout, layoutStackSettings);
        }

        public Control GetFocusableControl()
        {
            return this;
        }

        [NotNull]
        public string GetValue()
        {
            return layoutValue;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void SetField(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            Devices.Children.Clear();

            layoutSourceField = sourceField;

            RenderDisplayData();

            Resizer.FieldId = sourceField.FieldUris.First().FieldId;

            justSetField = true;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));

            if (!value.Equals(string.Empty))
            {
                DataNotSpecifiedNotification.Hide();
            }

            if (justSetField)
            {
                justSetField = false;
                layoutValue = value;
                return;
            }

            if (layoutValue.Equals(value))
            {
                // No change ~> return
                return;
            }

            layoutValue = value;

            if (layoutSourceField.Value.Equals(value))
            {
                MainBorder.IsEnabled = true;
                OutdatedNotification.Show();
            }
            else
            {
                MainBorder.IsEnabled = false;
                OutdatedNotification.Show();
            }

            var modified = ValueModified;
            if (modified != null)
            {
                modified();
            }

            justSetField = false;
        }

        public void UnsetField()
        {
            layoutSourceField = null;
            OutdatedNotification.Hide();
            DataNotSpecifiedNotification.Hide();
            ParseErrorNotification.Hide();
            MainBorder.IsEnabled = true;
        }

        public event ValueModifiedEventHandler ValueModified;

        [NotNull]
        private StackPanel DrawPanel([NotNull] IEnumerable<XElement> elements, [NotNull] StackSettings settings)
        {
            Debug.ArgumentNotNull(elements, nameof(elements));
            Debug.ArgumentNotNull(settings, nameof(settings));

            var stkElements = new StackPanel
            {
                Orientation = settings.StackOrientation
            };

            if (settings.StackStyle != null)
            {
                stkElements.Style = settings.StackStyle;
            }

            if (elements.Any())
            {
                foreach (var e in elements)
                {
                    var stkElement = new StackPanel
                    {
                        Orientation = settings.StackOrientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal
                    };

                    // <ICON-LABEL>
                    var stkHead = new StackPanel
                    {
                        Orientation = settings.HeaderStackOrientation
                    };

                    // ICON
                    var icon = new Icon(layoutSourceField.FieldUris.First().Site, GetAttributeValue(e, LayoutStruct.Attribute.Icon));

                    var image = new Image
                    {
                        Source = icon.GetSource(),
                        Height = settings.HeaderIconScale,
                        Width = settings.HeaderIconScale
                    };
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                    stkHead.Children.Add(image);

                    // LABEL
                    var label = new Label
                    {
                        Content = GetAttributeValue(e, LayoutStruct.Attribute.DisplayName)
                    };
                    if (settings.LabelTemplate != null)
                    {
                        label.Template = settings.LabelTemplate;
                    }

                    // LINK
                    if (e.Attribute(LayoutStruct.Attribute.ItemId) != null)
                    {
                        var element = e;

                        label.MouseUp += delegate(object sender, MouseButtonEventArgs args)
                        {
                            if (args.ChangedButton == MouseButton.Left)
                            {
                                GoToItemId(GetAttributeValue(element, LayoutStruct.Attribute.ItemId));
                            }
                        };
                    }

                    stkHead.Children.Add(label);

                    // BORDER
                    var brdHead = new Border();
                    if (settings.StackHeaderBorderStyle != null)
                    {
                        brdHead.Style = settings.StackHeaderBorderStyle;
                    }

                    brdHead.Child = stkHead;
                    stkElement.Children.Add(brdHead);

                    // </ICON-Label>

                    // CHILDREN
                    if (!string.IsNullOrEmpty(settings.ChildXNodeName) && settings.ChildSettings != null)
                    {
                        stkElement.Children.Add(DrawPanel(e.Descendants(settings.ChildXNodeName), settings.ChildSettings));
                    }

                    // BORDER
                    var brdItem = new Border();
                    if (settings.StackItemBorderStyle != null)
                    {
                        brdItem.Style = settings.StackItemBorderStyle;
                    }

                    brdItem.Child = stkElement;

                    // ADD TO PARENT
                    stkElements.Children.Add(brdItem);
                }
            }

            return stkElements;
        }

        [NotNull]
        private static string GetAttributeValue([NotNull] XElement element, [NotNull] string name)
        {
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(name, nameof(name));

            var attr = element.Attribute(name);
            return attr != null ? attr.Value : Rocks.Resources.LayoutField_GetAttributeValue_Default;
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        private void GoToItemId([NotNull] string itemId)
        {
            Debug.ArgumentNotNull(itemId, nameof(itemId));

            var databaseUri = layoutSourceField.FieldUris[0].ItemVersionUri.DatabaseUri;
            var itemUri = new ItemUri(databaseUri, new ItemId(new Guid(itemId)));
            var itemVersionUri = new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);

            AppHost.OpenContentEditor(itemVersionUri);
        }

        private void NoDisplayDataMessage()
        {
            MainBorder.IsEnabled = false;
            DataNotSpecifiedNotification.Show();
        }

        private void RenderDisplayData()
        {
            if (string.IsNullOrEmpty(layoutSourceField.DisplayData))
            {
                NoDisplayDataMessage();
                return;
            }

            try
            {
                var displayData = XDocument.Parse(layoutSourceField.DisplayData);

                var devices = from d in displayData.Descendants(LayoutStruct.Node.Device).OrderBy(element => element.GetAttributeValue("dn"))
                    select d;

                if (devices.Any())
                {
                    Devices.Children.Add(DrawPanel(devices, deviceStackSettings));
                }
                else
                {
                    NoDisplayDataMessage();
                }
            }
            catch (Exception e)
            {
                MainBorder.IsEnabled = false;
                DataNotSpecifiedNotification.Hide();
                ParseErrorNotification.Initialize(e);
                ParseErrorNotification.Show();
            }
        }

        private static class LayoutStruct
        {
            public static class Attribute
            {
                public const string DisplayName = @"dn";

                public const string Icon = @"i";

                public const string ItemId = @"id";
            }

            public static class Node
            {
                public const string Device = @"d";

                public const string Layout = @"l";

                public const string Rendering = @"r";
            }
        }

        private class StackSettings
        {
            public StackSettings(Orientation stackOrientation, Orientation headerStackOrientation, int headerIconScale, [CanBeNull] string header = null, [CanBeNull] Style stackStyle = null, [CanBeNull] Style stackItemBorderStyle = null, [CanBeNull] Style stackHeaderBorderStyle = null, [CanBeNull] ControlTemplate labelTemplate = null, [CanBeNull] string childXNodeName = null, [CanBeNull] StackSettings childSettings = null)
            {
                // Not adding a default orientation because that would make the init less intuitive.
                StackOrientation = stackOrientation;
                HeaderStackOrientation = headerStackOrientation;
                HeaderIconScale = headerIconScale;
                Header = header;
                StackStyle = stackStyle;
                StackItemBorderStyle = stackItemBorderStyle;
                StackHeaderBorderStyle = stackHeaderBorderStyle;
                LabelTemplate = labelTemplate;
                ChildXNodeName = childXNodeName;
                ChildSettings = childSettings;
            }

            [CanBeNull]
            public StackSettings ChildSettings { get; }

            [CanBeNull]
            public string ChildXNodeName { get; }

            public int HeaderIconScale { get; }

            public Orientation HeaderStackOrientation { get; }

            [CanBeNull]
            public ControlTemplate LabelTemplate { get; }

            [CanBeNull]
            public Style StackHeaderBorderStyle { get; }

            [CanBeNull]
            public Style StackItemBorderStyle { get; }

            public Orientation StackOrientation { get; }

            [CanBeNull]
            public Style StackStyle { get; }

            [CanBeNull]
            private string Header
            {
                [UsedImplicitly]
                get;
                set;
            }
        }
    }
}
