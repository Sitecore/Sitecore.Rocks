// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Shell.Pipelines.SetResources;

namespace Sitecore.Rocks.UI.Controls
{
    public class ThemeHandler : Control
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(@"Target", typeof(DependencyObject), typeof(ThemeHandler));

        public ThemeHandler()
        {
            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public DependencyObject Target
        {
            get { return (DependencyObject)GetValue(TargetProperty); }

            set { SetValue(TargetProperty, value); }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            if (Target == null)
            {
                Target = this.GetAncestor<IContextProvider>() as DependencyObject;
            }

            if (Target == null)
            {
                Target = this.GetAncestor<UserControl>();
            }

            if (Target == null)
            {
                Target = this.GetAncestor<Window>();
            }

            if (Target == null)
            {
                return;
            }

            var frameworkElement = Target as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            SetResourcePipeline.Run().WithParameters(frameworkElement);
        }
    }
}
