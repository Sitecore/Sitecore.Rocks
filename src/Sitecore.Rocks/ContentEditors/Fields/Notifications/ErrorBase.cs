// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;

namespace Sitecore.Rocks.ContentEditors.Fields.Notifications
{
    public abstract class ErrorBase : NotificationBase
    {
        private Exception exception;

        public void ShowExceptionMessageBox()
        {
            var clipboard = string.Format(@"{0}{3}{1}{3}{3}Sitecore Rocks {2}", exception.Message, exception.StackTrace, Assembly.GetExecutingAssembly().GetFileVersion(), Environment.NewLine);

            var text = string.Format(Rocks.Resources.SitecorePackage_HandleException_, clipboard);

            if (AppHost.MessageBox(text, Rocks.Resources.SitecorePackage_HandleException_Sitecore_Unhandled_Exception, MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
            {
                return;
            }

            Clipboard.SetData(DataFormats.Text, clipboard);

            AppHost.Browsers.Navigate(@"http://sdn.sitecore.net/forum/ShowForum.aspx?ForumID=36");
        }

        protected void Initialize([NotNull] string text, [NotNull] Exception exception, bool hidden)
        {
            Debug.ArgumentNotNull(text, nameof(text));
            Debug.ArgumentNotNull(exception, nameof(exception));

            this.exception = exception;

            InitializeStyle();
            InitializeText(text);

            if (hidden)
            {
                Visibility = Visibility.Hidden;
            }
        }

        private void HyperlinkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ShowExceptionMessageBox();
        }

        private new void InitializeText([NotNull] string text)
        {
            Debug.ArgumentNotNull(text, nameof(text));

            var m = Regex.Match(text, @"^(.*?)\[(.*?)\](.*?)$");
            if (!m.Success)
            {
                base.InitializeText(text);
                return;
            }

            var foreRun = new Run(m.Groups[1].ToString());

            var hyperRun = new Run(m.Groups[2].ToString());

            var hyperlink = new Hyperlink(hyperRun);
            hyperlink.Click += HyperlinkClick;

            var afterRun = new Run(m.Groups[3].ToString());

            var errorBlock = new TextBlock();
            errorBlock.Inlines.Add(foreRun);
            errorBlock.Inlines.Add(hyperlink);
            errorBlock.Inlines.Add(afterRun);

            Child = errorBlock;
        }
    }
}
