// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("security")]
    public partial class SecurityField : IReusableFieldControl
    {
        private string fieldValue;

        private bool justSetField;

        private Field souceFieldValue;

        public SecurityField()
        {
            InitializeComponent();
        }

        public Control GetFocusableControl()
        {
            return this;
        }

        public string GetValue()
        {
            return fieldValue;
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

            stkMain.Children.Clear();

            fieldValue = sourceField.Value;
            souceFieldValue = sourceField;

            try
            {
                var data = XDocument.Parse(sourceField.DisplayData);
                DrawAccounts(data);
                justSetField = true;
            }
            catch (Exception e)
            {
                MainBorder.IsEnabled = false;
                DataNotSpecifiedNotification.Hide();
                ParseErrorNotification.Initialize(e);
                ParseErrorNotification.Show();
            }
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
                fieldValue = value;
                return;
            }

            if (fieldValue.Equals(value))
            {
                // No change ~> return
                return;
            }

            fieldValue = value;

            MainBorder.IsEnabled = false;
            DataNotSpecifiedNotification.Hide();
            OutdatedNotification.Show();

            var modified = ValueModified;
            if (modified != null)
            {
                modified();
            }
        }

        public void UnsetField()
        {
            souceFieldValue = null;
            OutdatedNotification.Hide();
            DataNotSpecifiedNotification.Hide();
            ParseErrorNotification.Hide();
            MainBorder.IsEnabled = true;
        }

        public event ValueModifiedEventHandler ValueModified;

        private void DrawAccountHeader([NotNull] XElement account, [NotNull] Panel container)
        {
            Debug.ArgumentNotNull(account, nameof(account));
            Debug.ArgumentNotNull(container, nameof(container));

            var stkHeader = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0.0, 6.0, 0.0, 0.0)
            };

            // Account type
            var icon = new Icon(souceFieldValue.FieldUris.First().Site, GetAttributeValue(account, SecurityStruct.Attribute.Icon));
            var img = new Image
            {
                Source = icon.GetSource(),
                Height = 16,
                Width = 16,
                Margin = new Thickness(0.0, 0.0, 3.0, 0.0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            stkHeader.Children.Add(img);

            // Account name
            stkHeader.Children.Add(new TextBlock
            {
                Text = GetAttributeValue(account, SecurityStruct.Attribute.Name),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0.0, 0.0, 6.0, 0.0)
            });

            // Add to container
            container.Children.Add(stkHeader);
        }

        private void DrawAccountPermissions([NotNull] XContainer account, [NotNull] Panel container)
        {
            Debug.ArgumentNotNull(account, nameof(account));
            Debug.ArgumentNotNull(container, nameof(container));

            foreach (var permission in account.Descendants(SecurityStruct.Node.Permission))
            {
                var permissionView = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0.0, 6.0, 0.0, 0.0)
                };

                var entityPermission = PermissionFromString(GetAttributeValue(permission, SecurityStruct.Attribute.EntityPermission));
                var descendantsPermission = PermissionFromString(GetAttributeValue(permission, SecurityStruct.Attribute.DescendantsPermission));

                if (entityPermission == descendantsPermission)
                {
                    permissionView.Children.Add(new PermissionToggle
                    {
                        State = entityPermission,
                        Height = 16
                    });
                }
                else
                {
                    var permissionToggles = new StackPanel();

                    var smallStyle = Resources[@"stlSmall"] as Style;

                    permissionToggles.Children.Add(new PermissionToggle
                    {
                        State = entityPermission,
                        Style = smallStyle,
                    });
                    permissionToggles.Children.Add(new PermissionToggle
                    {
                        State = descendantsPermission,
                        Style = smallStyle
                    });
                    permissionView.Children.Add(permissionToggles);
                }

                permissionView.Children.Add(new TextBlock
                {
                    Text = GetAttributeValue(permission, SecurityStruct.Attribute.Title),
                    Margin = new Thickness(3.0, 0.0, 0.0, 0.0)
                });
                container.Children.Add(permissionView);
            }
        }

        private void DrawAccounts([NotNull] XContainer element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            var accounts = element.Descendants(SecurityStruct.Node.Account);

            if (!accounts.Any())
            {
                MainBorder.IsEnabled = false;
                DataNotSpecifiedNotification.Show();
                return;
            }

            var listOfAccounts = new Grid();
            listOfAccounts.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });
            listOfAccounts.ColumnDefinitions.Add(new ColumnDefinition());

            listOfAccounts.RowDefinitions.Add(new RowDefinition());

            var txtAccounts = new TextBlock
            {
                Text = Rocks.Resources.SecurityField_DrawAccounts_Accounts,
                Margin = new Thickness(0.0, 0.0, 6.0, 0.0),
                Style = Resources[@"stlHeader"] as Style
            };
            Grid.SetRow(txtAccounts, 0);
            Grid.SetColumn(txtAccounts, 0);
            listOfAccounts.Children.Add(txtAccounts);

            var txtPermissions = new TextBlock
            {
                Text = Rocks.Resources.SecurityField_DrawAccounts_Permissions,
                Style = Resources[@"stlHeader"] as Style
            };
            Grid.SetRow(txtPermissions, 0);
            Grid.SetColumn(txtPermissions, 1);
            listOfAccounts.Children.Add(txtPermissions);

            var row = 1;

            foreach (var account in accounts)
            {
                listOfAccounts.RowDefinitions.Add(new RowDefinition());

                var accountHeader = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                DrawAccountHeader(account, accountHeader);
                Grid.SetRow(accountHeader, row);
                Grid.SetColumn(accountHeader, 0);
                listOfAccounts.Children.Add(accountHeader);

                var accountPermissions = new StackPanel
                {
                    Margin = new Thickness(0.0, 0.0, 0.0, 12.0)
                };
                DrawAccountPermissions(account, accountPermissions);
                Grid.SetRow(accountPermissions, row);
                Grid.SetColumn(accountPermissions, 1);
                listOfAccounts.Children.Add(accountPermissions);

                row++;
            }

            stkMain.Children.Add(listOfAccounts);
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

        private static PermissionToggle.Permission PermissionFromString([NotNull] string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            if (value.Equals(@"+"))
            {
                return PermissionToggle.Permission.Allow;
            }

            if (value.Equals(@"-"))
            {
                return PermissionToggle.Permission.Deny;
            }

            return PermissionToggle.Permission.NotSet;
        }

        private static class SecurityStruct
        {
            public static class Attribute
            {
                public const string DescendantsPermission = @"d";

                public const string EntityPermission = @"e";

                public const string Icon = @"i";

                public const string Name = @"n";

                public const string Title = @"t";
            }

            public static class Node
            {
                public const string Account = @"a";

                public const string Permission = @"p";
            }
        }
    }
}
