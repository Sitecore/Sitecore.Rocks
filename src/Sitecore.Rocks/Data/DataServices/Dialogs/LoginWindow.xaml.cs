// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Net.Pipelines.Troubleshooter;
using Sitecore.Rocks.NewWebService;

namespace Sitecore.Rocks.Data.DataServices.Dialogs
{
    public partial class LoginWindow
    {
        private const int GWL_STYLE = -16;

        private const int WS_SYSMENU = 0x80000;

        public LoginWindow()
        {
            InitializeComponent();

            AppHost.DoEvents();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public Credentials Credentials { get; set; }

        [NotNull]
        public HardRockWebService DataService { get; set; }

        public bool DisableLoginErrorMessage { get; set; }

        [NotNull]
        public SitecoreWebService2SoapClient WebService { get; set; }

        protected bool IsComplete { get; set; }

        private void CloseWindow()
        {
            IsComplete = true;
            this.Close(true);
        }

        private void ControlClosing([NotNull] object sender, [NotNull] CancelEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!IsComplete)
            {
                e.Cancel = true;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            LoginText.Text = string.Format(Rocks.Resources.LoginWindow_ControlLoaded_Connecting_to___0______, DataService.Connection.HostName);

            WebService.LoginCompleted += LoginCompleted;
            WebService.LoginAsync(Credentials);

            AppHost.DoEvents();
        }

        [DllImport(@"user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        private void LoginCompleted([NotNull] object sender, [NotNull] LoginCompletedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Error != null)
            {
                if (DisableLoginErrorMessage)
                {
                    IsComplete = true;
                    this.Close(false);
                    return;
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    var pipeline = TroubleshooterPipeline.Run().WithParameters(DataService, true, e.Error, WebService.Endpoint);

                    if (pipeline.Retry)
                    {
                        WebService.LoginAsync(Credentials);
                        return;
                    }

                    IsComplete = true;
                    this.Close(false);
                }));

                return;
            }

            if (e.Result == @"Invalid user or password.")
            {
                Dispatcher.Invoke(new Action(ShowInvalidUserNameOrPassword));
                return;
            }

            WebService.LoginCompleted -= LoginCompleted;

            DataService.WebServiceVersion = e.Result;

            Dispatcher.Invoke(new Action(CloseWindow));
        }

        [DllImport(@"user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newLong);

        private void ShowInvalidUserNameOrPassword()
        {
            AppHost.MessageBox(Rocks.Resources.LoginWindow_ShowInvalidUserNameOrPassword_Invalid_user_name_or_password_, Rocks.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

            IsComplete = true;
            this.Close(false);
        }
    }
}
