// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.UI;

// ReSharper disable once CheckNamespace

namespace Sitecore
{
    public static class AppHost
    {
        private static Container _container;

        static AppHost()
        {
            Container.Register<BrowserHost, BrowserHost>().AsSingleton();
            Container.Register<ClipboardHost, ClipboardHost>().AsSingleton();
            Container.Register<CommandHost, CommandHost>().AsSingleton();
            Container.Register<ContextMenuHost, ContextMenuHost>().AsSingleton();
            Container.Register<EnvHost, EnvHost>().AsSingleton();
            Container.Register<ExtensibilityHost, ExtensibilityHost>().AsSingleton();
            Container.Register<FeatureHost, FeatureHost>().AsSingleton();
            Container.Register<FilesHost, FilesHost>().AsSingleton();
            Container.Register<GlobalsHost, GlobalsHost>().AsSingleton();
            Container.Register<MockHost, MockHost>().AsSingleton();
            Container.Register<OutputHost, OutputHost>().AsSingleton();
            Container.Register<PipelineHost, PipelineHost>().AsSingleton();
            Container.Register<PluginHost, PluginHost>().AsSingleton();
            Container.Register<ProjectHost, ProjectHost>().AsSingleton();
            Container.Register<SelectionHost, SelectionHost>().AsSingleton();
            Container.Register<ServerHost, ServerHost>().AsSingleton();
            Container.Register<SettingsHost, SettingsHost>().AsSingleton();
            Container.Register<ShellHost, ShellHost>().AsSingleton();
            Container.Register<SitesHost, SitesHost>().AsSingleton();
            Container.Register<StatusbarHost, StatusbarHost>().AsSingleton();
            Container.Register<TasksHost, TasksHost>().AsSingleton();
            Container.Register<UsageHost, UsageHost>().AsSingleton();
            Container.Register<UserHost, UserHost>().AsSingleton();
            Container.Register<WindowsHost, WindowsHost>().AsSingleton();
        }

        [NotNull]
        public static BrowserHost Browsers => Container.Resolve<BrowserHost>();

        [NotNull]
        public static ClipboardHost Clipboard => Container.Resolve<ClipboardHost>();

        [NotNull]
        public static CommandHost Commands => Container.Resolve<CommandHost>();

        [NotNull]
        public static Container Container => _container ?? (_container = new Container());

        [NotNull]
        public static ContextMenuHost ContextMenus => Container.Resolve<ContextMenuHost>();

        [CanBeNull]
        public static ContentTree CurrentContentTree => ActiveContext.ActiveContentTree;

        [NotNull]
        public static EnvHost Env => Container.Resolve<EnvHost>();

        [NotNull]
        public static ExtensibilityHost Extensibility => Container.Resolve<ExtensibilityHost>();

        [NotNull]
        public static FeatureHost Features => Container.Resolve<FeatureHost>();

        [NotNull]
        public static FilesHost Files => Container.Resolve<FilesHost>();

        [NotNull]
        public static GlobalsHost Globals => Container.Resolve<GlobalsHost>();

        [NotNull]
        public static MockHost Mock => Container.Resolve<MockHost>();

        [NotNull]
        public static IOptions Options => Settings.Options;

        [NotNull]
        public static OutputHost Output => Container.Resolve<OutputHost>();

        [NotNull]
        public static PipelineHost Pipelines => Container.Resolve<PipelineHost>();

        [NotNull]
        public static PluginHost Plugins => Container.Resolve<PluginHost>();

        [NotNull]
        public static ProjectHost Projects => Container.Resolve<ProjectHost>();

        [NotNull]
        public static SelectionHost Selection => Container.Resolve<SelectionHost>();

        [NotNull]
        public static ServerHost Server => Container.Resolve<ServerHost>();

        [NotNull]
        public static SettingsHost Settings => Container.Resolve<SettingsHost>();

        [NotNull]
        public static ShellHost Shell => Container.Resolve<ShellHost>();

        [NotNull]
        public static SitesHost Sites => Container.Resolve<SitesHost>();

        [NotNull]
        public static StatusbarHost Statusbar => Container.Resolve<StatusbarHost>();

        [NotNull]
        public static TasksHost Tasks => Container.Resolve<TasksHost>();

        [NotNull]
        public static UsageHost Usage => Container.Resolve<UsageHost>();

        [NotNull]
        public static UserHost User => Container.Resolve<UserHost>();

        [NotNull]
        public static WindowsHost Windows => Container.Resolve<WindowsHost>();

        public static void DoEvents()
        {
            Shell.DoEvents();
        }

        public static bool DoEvents(ref bool busy, int timeoutInSeconds = 120)
        {
            return Shell.DoEvents(ref busy, timeoutInSeconds);
        }

        public static bool DoEvents(ref int busy, int timeoutInSeconds = 120)
        {
            return Shell.DoEvents(ref busy, timeoutInSeconds);
        }

        [CanBeNull]
        public static T GetToolWindow<T>([NotNull] string caption) where T : class
        {
            Assert.ArgumentNotNull(caption, nameof(caption));

            return Windows.GetToolWindow<T>(caption);
        }

        public static MessageBoxResult MessageBox([NotNull] string text, [NotNull] string title, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxIcon)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(title, nameof(title));

            return Shell.MessageBox(text, title, messageBoxButton, messageBoxIcon);
        }

        public static void OpenContentEditor([NotNull] ItemVersionUri itemVersionUri)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));

            Windows.OpenContentEditor(itemVersionUri);
        }

        public static void OpenContentEditor([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            OpenContentEditor(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest));
        }

        [CanBeNull]
        public static T OpenDocumentWindow<T>([NotNull] string documentName) where T : class, new()
        {
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            return Windows.OpenDocumentWindow<T>(documentName);
        }

        public static void OpenToolWindow<T>([NotNull] T control, [NotNull] string caption, WindowsHost.Dock dock = WindowsHost.Dock.Left) where T : class
        {
            Assert.ArgumentNotNull(control, nameof(control));
            Assert.ArgumentNotNull(caption, nameof(caption));

            Windows.OpenToolWindow(control, caption, dock);
        }

        public static void OpenUsingDefaultAction([NotNull] ItemUri itemUri, [NotNull] string itemName = "")
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(itemName, nameof(itemName));

            DefaultActionPipeline.Run().WithParameters(new ItemSelectionContext(new ItemDescriptor(itemUri, itemName)));
        }

        public static void OpenUsingDefaultAction([NotNull] ITemplatedItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            DefaultActionPipeline.Run().WithParameters(new ItemSelectionContext(item));
        }

        [CanBeNull]
        public static string Prompt([NotNull] string text, [NotNull] string title, [NotNull] string value = "")
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(value, nameof(value));

            return Shell.Prompt(text, title, value);
        }

        internal static void Internal()
        {
        }
    }
}
