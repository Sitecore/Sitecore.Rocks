// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.StatusIcons;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public class BaseTreeViewItemHeader : StackPanel
    {
        private static readonly Thickness iconMargin = new Thickness(1, 1, 2, 1);

        public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(BaseTreeViewItemHeader), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(BaseTreeViewItemHeader), new PropertyMetadata(default(bool)));

        private static readonly Thickness labelMargin = new Thickness(2, 1, 2, 0);

        private static readonly Thickness noMargin = new Thickness(0, 0, 0, 0);

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(BaseTreeViewItemHeader), new PropertyMetadata(default(string)));

        private TextBox editBox;

        private Icon icon;

        private bool isActive;

        private string oldText;

        public BaseTreeViewItemHeader()
        {
            InitializeComponent();

            Orientation = Orientation.Horizontal;
            icon = Icon.Empty;
            IsEditable = true;
        }

        [NotNull]
        public Icon Icon
        {
            get { return icon; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                icon = value;
                IconImage.Source = icon.GetSource();
            }
        }

        public bool IsActive
        {
            get { return isActive; }

            set
            {
                isActive = value;

                Label.FontWeight = isActive ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        public bool IsEditable { get; set; }

        public bool IsInEditMode
        {
            get { return (bool)GetValue(IsInEditModeProperty); }

            set
            {
                if (!IsEditable)
                {
                    return;
                }

                if (value == IsInEditMode)
                {
                    return;
                }

                SetValue(IsInEditModeProperty, value);

                if (value)
                {
                    StartEditMode();
                }
                else
                {
                    EndEditMode();
                }
            }
        }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }

            set
            {
                SetValue(IsLoadingProperty, value);
                Loading.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        [NotNull]
        public StackPanel StatusIcons { get; private set; }

        [NotNull]
        public string Text
        {
            get { return GetValue(TextProperty) as string ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                SetValue(TextProperty, value);
                Label.Text = value;
            }
        }

        [NotNull]
        private TextBox EditBox
        {
            get
            {
                if (editBox == null)
                {
                    editBox = new TextBox
                    {
                        Visibility = Visibility.Collapsed,
                        Margin = noMargin,
                        Padding = noMargin,
                        Height = 16,
                        BorderThickness = noMargin
                    };

                    editBox.KeyDown += HandleKeyDown;
                    editBox.LostFocus += HandleLostFocus;

                    Children.Add(editBox);
                }

                return editBox;
            }
        }

        [NotNull]
        private Image IconImage { get; set; }

        [NotNull]
        private TextBlock Label { get; set; }

        [NotNull]
        private TextBlock Loading { get; set; }

        public void Edit()
        {
            IsInEditMode = true;
        }

        public event EventHandler TextChanged;

        protected void InitializeComponent()
        {
            StatusIcons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Width = AppHost.Env.StatusIcons().StatusIcons.Count() * 7
            };

            IconImage = new Image
            {
                Height = 16,
                Width = 16,
                Margin = iconMargin
            };

            Label = new TextBlock
            {
                Margin = labelMargin
            };

            Loading = new TextBlock
            {
                Text = @" - " + Rocks.Resources.BaseTreeViewItemHeader_InitializeComponent_loading,
                Visibility = Visibility.Collapsed,
                Foreground = SystemColors.GrayTextBrush
            };

            Children.Add(StatusIcons);
            Children.Add(IconImage);
            Children.Add(Label);
            Children.Add(Loading);
        }

        private void Accept()
        {
            Text = EditBox.Text;
            IsInEditMode = false;
            RaiseTextChanged();
        }

        private void EndEditMode()
        {
            Label.Visibility = Visibility.Visible;
            EditBox.Visibility = Visibility.Collapsed;
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                Accept();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Reject();
                e.Handled = true;
            }
        }

        private void HandleLostFocus([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (IsInEditMode)
            {
                Accept();
            }
        }

        private void RaiseTextChanged()
        {
            var handler = TextChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void Reject()
        {
            Text = oldText;
            IsInEditMode = false;
        }

        private void StartEditMode()
        {
            oldText = Text;
            EditBox.Text = Text;

            Label.Visibility = Visibility.Collapsed;
            EditBox.Visibility = Visibility.Visible;

            EditBox.SelectAll();

            AppHost.DoEvents();

            Keyboard.Focus(EditBox);
        }
    }
}
