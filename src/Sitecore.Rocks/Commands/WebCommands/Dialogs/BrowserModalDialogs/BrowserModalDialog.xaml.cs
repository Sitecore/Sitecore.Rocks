// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.Commands.WebCommands.Dialogs.BrowserModalDialogs
{
    public partial class BrowserModalDialog
    {
        public BrowserModalDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Browser.MessageHook += MessageHook;
        }

        public string Url
        {
            set { Browser.Navigate(value); }
        }

        private IntPtr MessageHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0010:
                    handled = true;
                    this.Close(true);
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
