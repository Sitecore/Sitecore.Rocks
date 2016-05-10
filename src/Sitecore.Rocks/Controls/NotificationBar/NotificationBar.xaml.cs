// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Reflection;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.NotificationBar
{
    public partial class NotificationBar
    {
        private string _optionsKeyName;

        public NotificationBar()
        {
            InitializeComponent();

            /*
      this.Loaded += this.ControlLoaded;
      this.content = this.Content;
      */

            Visibility = Visibility.Collapsed;
        }

        /*





    private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
    {
      Debug.ArgumentNotNull(sender, "sender");
      Debug.ArgumentNotNull(e, "e");

      this.Loaded -= this.ControlLoaded;

        this.Inner.Content = this.Content;
        this.Content = this.content;
    }
    */

        [NotNull]
        public string OptionsKeyName
        {
            get { return _optionsKeyName ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _optionsKeyName = value;

                var options = AppHost.Settings.Options;

                var propertyInfo = GetPropertyInfo(options);
                if (propertyInfo == null)
                {
                    return;
                }

                var hide = (bool)propertyInfo.GetValue(options, null);
                if (hide)
                {
                    Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Close([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            Visibility = Visibility.Collapsed;

            var options = AppHost.Settings.Options;

            var propertyInfo = GetPropertyInfo(options);
            if (propertyInfo == null)
            {
                return;
            }

            propertyInfo.SetValue(options, true, null);
            options.Save();
        }

        [CanBeNull]
        private PropertyInfo GetPropertyInfo([NotNull] IOptions options)
        {
            Assert.ArgumentNotNull(options, nameof(options));

            if (string.IsNullOrEmpty(OptionsKeyName))
            {
                return null;
            }

            return options.GetType().GetProperty(OptionsKeyName);
        }
    }
}
