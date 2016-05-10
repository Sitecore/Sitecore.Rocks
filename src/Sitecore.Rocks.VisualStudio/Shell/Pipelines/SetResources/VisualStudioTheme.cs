// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.SetResources
{
    [Pipeline(typeof(SetResourcePipeline), 3000)]
    public class VisualStudioTheme : PipelineProcessor<SetResourcePipeline>
    {
        protected override void Process(SetResourcePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var resourceDictionary = pipeline.FrameworkElement.Resources;
            RemoveDictionaries(resourceDictionary);

            var appTheme = AppHost.Options.AppTheme;
            if (appTheme == AppTheme.System)
            {
                return;
            }

            if (appTheme == AppTheme.Automatic)
            {
                appTheme = AppHost.Shell.VisualStudioTheme;
            }

            if (appTheme == AppTheme.Dark)
            {
                AddStandardTheme(resourceDictionary);
                MapSystemColors(resourceDictionary, pipeline.FrameworkElement);
            }
        }

        private void AddStandardTheme([NotNull] ResourceDictionary resourceDictionary)
        {
            Debug.ArgumentNotNull(resourceDictionary, nameof(resourceDictionary));

            var dictionary = new RocksResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Sitecore.Rocks.VisualStudio;component/Resources/Theme.xaml")
            };

            resourceDictionary.MergedDictionaries.Add(dictionary);

            dictionary = new RocksResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Sitecore.Rocks.VisualStudio;component/Resources/Themes/Styles.xaml")
            };

            resourceDictionary.MergedDictionaries.Add(dictionary);
        }

        private void MapSystemColors([NotNull] ResourceDictionary resourceDictionary, [NotNull] FrameworkElement frameworkElement)
        {
            Debug.ArgumentNotNull(resourceDictionary, nameof(resourceDictionary));
            Debug.ArgumentNotNull(frameworkElement, nameof(frameworkElement));

            var dictionary = new ResourceDictionary();
            resourceDictionary.MergedDictionaries.Add(dictionary);

            /*
            dictionary.Add(AppColors.CommandBarBrushKey, frameworkElement.TryFindResource(VsBrushes.CommandBarGradientKey));
            dictionary.Add(AppColors.LabelTextBrushKey, frameworkElement.TryFindResource(VsBrushes.GrayTextKey));
            dictionary.Add(AppColors.ProjectBackgroundGradientBrushKey, frameworkElement.TryFindResource(VsBrushes.ToolWindowBackgroundKey));
            dictionary.Add(AppColors.ToolWindowBackgroundBrushKey, frameworkElement.TryFindResource(VsBrushes.ToolWindowBackgroundKey));
            dictionary.Add(AppColors.ToolWindowBorderBrushKey, frameworkElement.TryFindResource(VsBrushes.ToolWindowBorderKey));
            */

            /*
            dictionary.Add(SystemColors.MenuBrushKey, frameworkElement.TryFindResource(VsBrushes.MenuKey));
            dictionary.Add(SystemColors.MenuTextBrushKey, frameworkElement.TryFindResource(VsBrushes.MenuTextKey));
            */

            dictionary.Add(SystemColors.ActiveBorderBrushKey, frameworkElement.TryFindResource(VsBrushes.ActiveBorderKey));
            dictionary.Add(SystemColors.ActiveCaptionBrushKey, frameworkElement.TryFindResource(VsBrushes.ActiveCaptionKey));
            dictionary.Add(SystemColors.AppWorkspaceBrushKey, frameworkElement.TryFindResource(VsBrushes.AppWorkspaceKey));

            dictionary.Add(SystemColors.ControlLightLightBrushKey, frameworkElement.TryFindResource(VsBrushes.AccentPaleKey));
            dictionary.Add(SystemColors.ControlLightBrushKey, frameworkElement.TryFindResource(VsBrushes.AccentLightKey));
            dictionary.Add(SystemColors.ControlBrushKey, frameworkElement.TryFindResource(VsBrushes.EnvironmentBackgroundKey));
            dictionary.Add(SystemColors.ControlDarkBrushKey, frameworkElement.TryFindResource(VsBrushes.AccentMediumKey));
            dictionary.Add(SystemColors.ControlDarkDarkBrushKey, frameworkElement.TryFindResource(VsBrushes.AccentDarkKey));
            dictionary.Add(SystemColors.ControlTextBrushKey, frameworkElement.TryFindResource(VsBrushes.CaptionTextKey));

            dictionary.Add(SystemColors.WindowBrushKey, frameworkElement.TryFindResource(VsBrushes.WindowKey));
            dictionary.Add(SystemColors.WindowTextBrushKey, frameworkElement.TryFindResource(VsBrushes.WindowTextKey));
            dictionary.Add(SystemColors.WindowFrameBrushKey, frameworkElement.TryFindResource(VsBrushes.WindowFrameKey));

            dictionary.Add(SystemColors.GrayTextBrushKey, frameworkElement.TryFindResource(VsBrushes.GrayTextKey));

            dictionary.Add(SystemColors.HighlightBrushKey, frameworkElement.TryFindResource(VsBrushes.HighlightKey));
            dictionary.Add(SystemColors.HighlightTextBrushKey, frameworkElement.TryFindResource(VsBrushes.HighlightTextKey));

            dictionary.Add(SystemColors.InactiveBorderBrushKey, frameworkElement.TryFindResource(VsBrushes.InactiveBorderKey));
            dictionary.Add(SystemColors.InactiveCaptionBrushKey, frameworkElement.TryFindResource(VsBrushes.InactiveCaptionKey));
            dictionary.Add(SystemColors.InactiveCaptionTextBrushKey, frameworkElement.TryFindResource(VsBrushes.InactiveCaptionTextKey));

            dictionary.Add(SystemColors.InfoBrushKey, frameworkElement.TryFindResource(VsBrushes.InfoBackgroundKey));
            dictionary.Add(SystemColors.InfoTextBrushKey, frameworkElement.TryFindResource(VsBrushes.InfoTextKey));
        }

        private void RemoveDictionaries([NotNull] ResourceDictionary resources)
        {
            Debug.ArgumentNotNull(resources, nameof(resources));

            var mergedDictionaries = resources.MergedDictionaries;

            for (var index = mergedDictionaries.Count - 1; index >= 0; index--)
            {
                var dictionary = mergedDictionaries[index] as RocksResourceDictionary;
                if (dictionary != null)
                {
                    mergedDictionaries.Remove(dictionary);
                }
            }
        }

        public class RocksResourceDictionary : ResourceDictionary
        {
        }
    }
}
