// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Fixes;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.ActionSkins
{
    public partial class ValidationPresenter
    {
        private SeverityLevel severity;

        public ValidationPresenter([NotNull] IValidationViewerSkin skin, [NotNull] ValidationDescriptor item)
        {
            Assert.ArgumentNotNull(skin, nameof(skin));
            Assert.ArgumentNotNull(item, nameof(item));

            InitializeComponent();

            Skin = skin;
            Item = item;

            Severity = item.Severity;
            Title = item.Title;
            Problem = item.Problem;
            Solution = item.Solution;

            if (string.IsNullOrEmpty(item.Problem))
            {
                ProblemField.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(item.Solution))
            {
                SolutionField.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(item.ExternalLink))
            {
                ExternalLink.Visibility = Visibility.Collapsed;
            }

            if (item.ItemUri != ItemVersionUri.Empty)
            {
                ItemPath.Inlines.Add(item.ItemPath);
            }
            else
            {
                ItemLink.Visibility = Visibility.Collapsed;
            }

            var fix = FixManager.GetFix(item);
            if (fix == null)
            {
                FixButton.Visibility = Visibility.Collapsed;
            }
        }

        [NotNull]
        public ValidationDescriptor Item { get; }

        [NotNull]
        public string Problem
        {
            get { return ProblemField.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                ProblemField.Text = value;
            }
        }

        public SeverityLevel Severity
        {
            get { return severity; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                severity = value;

                switch (Severity)
                {
                    case SeverityLevel.Error:
                        StateField.Background = new LinearGradientBrush(Color.FromRgb(0xac, 0x01, 0x00), Color.FromRgb(0xdd, 0x01, 0x00), 90);
                        break;
                    case SeverityLevel.Warning:
                        StateField.Background = new LinearGradientBrush(Color.FromRgb(0xf2, 0xb1, 0x00), Color.FromRgb(0xfe, 0xcd, 0x48), 90);
                        break;
                    case SeverityLevel.Suggestion:
                        StateField.Background = new LinearGradientBrush(Color.FromRgb(0x1c, 0x5d, 0xb5), Color.FromRgb(0x1c, 0x70, 0xe0), 90);
                        break;
                    case SeverityLevel.Hint:
                        StateField.Background = new LinearGradientBrush(Color.FromRgb(0x97, 0xac, 0x1a), Color.FromRgb(0xbc, 0xd6, 0x1a), 90);
                        break;
                    default:
                        StateField.Background = Brushes.Green;
                        break;
                }
            }
        }

        [NotNull]
        public IValidationViewerSkin Skin { get; set; }

        [NotNull]
        public string Solution
        {
            get { return SolutionField.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                SolutionField.Text = value;
            }
        }

        [NotNull]
        public string Title
        {
            get { return TitleField.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                TitleField.Text = value;
            }
        }

        private void DisableValidation([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (AppHost.MessageBox(string.Format("Are you sure you want to disable the \"{0}\" validation?\n\nThe validation will remain in the report until the assessment is re-run.", Item.Name), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            Skin.ValidationViewer.Disable(Item);
        }

        private void Fix([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var fix = FixManager.GetFix(Item);
            if (fix == null)
            {
                return;
            }

            fix.Fix(Item);
        }

        private void HideItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Skin.Hide(Item);
        }

        private void ItemLinkClicked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.OpenContentEditor(Item.ItemUri);
        }

        private void LinkClicked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Browsers.Navigate(Item.ExternalLink);
        }
    }
}
