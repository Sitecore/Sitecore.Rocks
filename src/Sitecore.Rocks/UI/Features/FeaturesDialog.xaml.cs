// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Features
{
    public partial class FeaturesDialog
    {
        public FeaturesDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            RenderFeatures(AppHost.Features.Features);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var modified = false;

            foreach (var checkBox in FeatureStackPanel.Children.OfType<CheckBox>())
            {
                var feature = checkBox.Tag as FeatureDescriptor;
                if (feature == null)
                {
                    continue;
                }

                var isEnabled = checkBox.IsChecked == true;

                modified = modified || feature.IsEnabled != isEnabled;

                feature.IsEnabled = isEnabled;
            }

            this.Close(true);

            if (!modified)
            {
                return;
            }

            AppHost.Extensibility.Reinitialize();
            Notifications.RaiseFeaturesChanged(this);
        }

        private void RenderFeatures([NotNull] IEnumerable<FeatureDescriptor> features)
        {
            Debug.ArgumentNotNull(features, nameof(features));

            var thickness = new Thickness(0, 2, 0, 2);

            foreach (var feature in features.OrderBy(f => f.Name))
            {
                var checkBox = new CheckBox
                {
                    Content = feature.Name,
                    IsChecked = feature.IsEnabled,
                    Tag = feature,
                    Margin = thickness
                };

                FeatureStackPanel.Children.Add(checkBox);
            }
        }

        private void Set([NotNull] Func<string, bool> isEnabled)
        {
            Debug.ArgumentNotNull(isEnabled, nameof(isEnabled));

            foreach (var checkBox in FeatureStackPanel.Children.OfType<CheckBox>())
            {
                var feature = checkBox.Tag as FeatureDescriptor;
                if (feature == null)
                {
                    continue;
                }

                checkBox.IsChecked = isEnabled(feature.Name);
            }
        }

        private void SetAdvanced([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var features = new[]
            {
                FeatureNames.Experimental,
                FeatureNames.Cloning,
                FeatureNames.Folders
            };

            Set(s => !features.Contains(s));
        }

        private void SetAll([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Set(s => s != FeatureNames.Experimental);
        }

        private void SetBasic([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var features = new[]
            {
                FeatureNames.Publishing,
                FeatureNames.Sorting,
                FeatureNames.Workflow,
                FeatureNames.Browsing,
            };

            Set(features.Contains);
        }

        private void SetDevelopment([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var features = new[]
            {
                FeatureNames.AdvancedNavigation,
                FeatureNames.AdvancedOperations,
                FeatureNames.AdvancedPublishing,
                FeatureNames.Browsing,
                FeatureNames.Commandy,
                FeatureNames.CommonTools,
                FeatureNames.Exporting,
                FeatureNames.Gutters,
                FeatureNames.Management,
                FeatureNames.Packages,
                FeatureNames.PowerShell,
                FeatureNames.Publishing,
                FeatureNames.Scripting,
                FeatureNames.Security,
                FeatureNames.Serialization,
                FeatureNames.Sorting,
                FeatureNames.Validation,
                FeatureNames.WebServer,
                FeatureNames.Workflow,
                FeatureNames.SitecoreExplorer.Files,
                FeatureNames.Speak,
            };

            Set(features.Contains);
        }

        private void SetMedium([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var features = new[]
            {
                FeatureNames.Browsing,
                FeatureNames.CommonTools,
                FeatureNames.Exporting,
                FeatureNames.Gutters,
                FeatureNames.Management,
                FeatureNames.Packages,
                FeatureNames.Publishing,
                FeatureNames.Scripting,
                FeatureNames.Security,
                FeatureNames.Serialization,
                FeatureNames.Sorting,
                FeatureNames.Validation,
                FeatureNames.Workflow,
                FeatureNames.SitecoreExplorer.Files,
            };

            Set(features.Contains);
        }

        private void SetMinimum([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));
            Set(s => false);
        }
    }
}
