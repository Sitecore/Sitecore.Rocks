// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.WindowExtensions
{
    public static class WindowExtensions
    {
        public const string WindowsStorageKey = "Windows";

        public static void Close([NotNull] this Window window, bool dialogResult)
        {
            Assert.ArgumentNotNull(window, nameof(window));

            if (window.DialogResult != null)
            {
                return;
            }

            try
            {
                window.DialogResult = dialogResult;
                window.Close();
            }
            catch
            {
                window.Close();
            }
        }

        public static void InitializeDialog([NotNull] this Window window)
        {
            Assert.ArgumentNotNull(window, nameof(window));

            AppHost.Shell.InitializeDialog(window);
        }

        public static void LoadPosition([NotNull] this Window window, [NotNull] string key)
        {
            Assert.ArgumentNotNull(window, nameof(window));
            Assert.ArgumentNotNull(key, nameof(key));

            var left = AppHost.Settings.GetInt(WindowsStorageKey, key + "Left", int.MinValue);
            if (left == int.MinValue)
            {
                return;
            }

            window.Left = left;
            window.Top = AppHost.Settings.GetInt(WindowsStorageKey, key + "Top", 0);
            window.Width = AppHost.Settings.GetInt(WindowsStorageKey, key + "Width", 1024);
            window.Height = AppHost.Settings.GetInt(WindowsStorageKey, key + "Height", 768);

            var isMaximized = AppHost.Settings.GetBool(WindowsStorageKey, key + "Maximized", false);
            if (isMaximized)
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        public static void SavePosition([NotNull] this Window window, [NotNull] string key)
        {
            Assert.ArgumentNotNull(window, nameof(window));
            Assert.ArgumentNotNull(key, nameof(key));

            var isMaximized = window.WindowState == WindowState.Maximized;

            AppHost.Settings.SetBool(WindowsStorageKey, key + "Maximized", isMaximized);

            if (isMaximized && (int)window.RestoreBounds.Left != int.MinValue)
            {
                AppHost.Settings.Set(WindowsStorageKey, key + "Left", ((int)window.RestoreBounds.Left).ToString());
                AppHost.Settings.Set(WindowsStorageKey, key + "Top", ((int)window.RestoreBounds.Top).ToString());
                AppHost.Settings.Set(WindowsStorageKey, key + "Height", ((int)window.RestoreBounds.Height).ToString());
                AppHost.Settings.Set(WindowsStorageKey, key + "Width", ((int)window.RestoreBounds.Width).ToString());
                return;
            }

            AppHost.Settings.Set(WindowsStorageKey, key + "Left", ((int)window.Left).ToString());
            AppHost.Settings.Set(WindowsStorageKey, key + "Top", ((int)window.Top).ToString());
            AppHost.Settings.Set(WindowsStorageKey, key + "Height", ((int)window.Height).ToString());
            AppHost.Settings.Set(WindowsStorageKey, key + "Width", ((int)window.Width).ToString());
        }
    }
}
