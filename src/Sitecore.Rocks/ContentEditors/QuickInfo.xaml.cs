// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Commands.Languages;
using Sitecore.Rocks.ContentEditors.Commands.Navigating;
using Sitecore.Rocks.ContentEditors.Commands.Versions;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.ImageExtensions;

namespace Sitecore.Rocks.ContentEditors
{
    public partial class QuickInfo
    {
        private ContentEditor contentEditor;

        public QuickInfo()
        {
            InitializeComponent();

            var options = AppHost.Settings.Options;
            if (options.HideQuickInfo)
            {
                Visibility = Visibility.Collapsed;
            }
        }

        [NotNull]
        public ContentEditor ContentEditor
        {
            get { return contentEditor; }
        }

        [NotNull]
        public ContentModel ContentModel
        {
            get { return ContentEditor.ContentModel; }
        }

        public void Load([NotNull] ContentEditor editor)
        {
            Assert.ArgumentNotNull(editor, nameof(editor));

            contentEditor = editor;
            RenderInfo();
        }

        private void ChangeTemplate([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = new ContentEditorContext(ContentEditor);

            var command = new ChangeTemplate();
            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }

        private void CopyIdToClipboard([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Clipboard.SetText(ItemId.Text);
        }

        private void CopyPathToClipboard([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = ContentModel.FirstItem;

            var path = string.Empty;

            for (var n = item.Path.Count - 1; n >= 0; n--)
            {
                path += @"/" + item.Path[n].Name;
            }

            AppHost.Clipboard.SetText(path);
        }

        private void CopyTemplateIdToClipboard([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Clipboard.SetText(TemplateId.Text);
        }

        private void GoToItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var hyperlink = sender as Hyperlink;
            if (hyperlink == null)
            {
                return;
            }

            var itemUri = hyperlink.Tag as ItemUri;
            if (itemUri == null)
            {
                return;
            }

            var uri = new ItemVersionUri(itemUri, ContentModel.FirstItem.Uri.Language, Version.Latest);

            AppHost.OpenContentEditor(uri);
        }

        private void GoToTemplate([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = ContentModel.FirstItem;

            var templateId = item.TemplateId;
            if (templateId == Data.ItemId.Empty)
            {
                return;
            }

            var uri = new ItemVersionUri(new ItemUri(new DatabaseUri(item.Uri.ItemUri.Site, item.Uri.ItemUri.DatabaseName), templateId), item.Uri.Language, Version.Latest);

            AppHost.OpenContentEditor(uri);
        }

        private void IconMouseDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var context = new ContentEditorContext(ContentEditor);

            var command = new SetIcon();
            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }

        private void LanguagesClick([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            var context = new ContentEditorContext(ContentEditor);

            var languages = new LanguagesSubmenu();
            var commands = languages.GetSubmenuCommands(context);

            ShowContextMenu(sender, commands, context);
        }

        private void RenderEmpty()
        {
            ItemName.Text = Rocks.Resources.QuickInfo_RenderEmpty_No_Item;
            TemplateName.Text = Rocks.Resources.N_A;
            ItemId.Text = Rocks.Resources.N_A;

            QuickInfoIcon.SetImage("Resources/32x32/cube_blue.png");
        }

        private void RenderInfo()
        {
            if (ContentModel.IsEmpty)
            {
                RenderEmpty();
                return;
            }

            if (ContentModel.IsMultiple)
            {
                RenderMultipleItems();
                return;
            }

            RenderItem();
        }

        private void RenderItem()
        {
            var item = ContentModel.FirstItem;

            ItemName.Text = item.Name;
            ItemId.Text = item.Uri.ItemUri.ItemId.ToString();

            TemplateName.Text = string.IsNullOrWhiteSpace(item.TemplateName) ? Rocks.Resources.QuickInfo_RenderItem_Unknown : item.TemplateName;
            TemplateId.Text = item.TemplateId == Data.ItemId.Empty ? Rocks.Resources.QuickInfo_RenderItem_Unknown : item.TemplateId.ToString();

            QuickInfoIcon.Source = item.Icon.GetSource();

            RenderPath(item);
            RenderVersions(item);
            RenderLanguages(item);
        }

        [NotNull]
        private HyperlinkBox RenderLanguage([NotNull] ContentEditorContext context, [NotNull] string name)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(name, nameof(name));

            CultureInfo cultureInfo;
            try
            {
                cultureInfo = new CultureInfo(name);
            }
            catch
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }

            var language = new Language(name);

            var command = new SetLanguage(language);

            var hyperlink = new HyperlinkBox
            {
                Text = name,
                CommandParameter = context,
                Command = command,
                ToolTip = cultureInfo.NativeName + @" / " + cultureInfo.EnglishName
            };

            Languages.Inlines.Add(hyperlink);

            return hyperlink;
        }

        private void RenderLanguages([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            Languages.Inlines.Clear();

            var context = new ContentEditorContext(ContentEditor);
            var languages = item.Languages;

            if (languages.Count > 0)
            {
                var language = RenderLanguage(context, item.Uri.Language.Name);
                language.IsSelected = true;

                var n = 0;
                foreach (var name in languages)
                {
                    if (name == item.Uri.Language.Name)
                    {
                        continue;
                    }

                    RenderLanguage(context, name);

                    n++;
                    if (n >= 2)
                    {
                        break;
                    }
                }
            }

            var more = new Hyperlink(new Run(@"..."))
            {
                Style = FindResource(@"HyperlinkValue") as Style
            };

            more.Click += LanguagesClick;

            var border = new Border
            {
                Child = new TextBlock(more),
                Margin = new Thickness(2)
            };

            Languages.Inlines.Add(border);
        }

        private void RenderMultipleItems()
        {
            ItemName.Text = string.Format(Rocks.Resources.QuickInfo_RenderMultipleItems__0__items, ContentModel.Items.Count);
            TemplateName.Text = Rocks.Resources.N_A;
            ItemId.Text = Rocks.Resources.N_A;
            Languages.Text = Rocks.Resources.N_A;
            Versions.Text = Rocks.Resources.N_A;

            QuickInfoIcon.SetImage("Resources/32x32/cube_blue.png");
        }

        private void RenderPath([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            ItemPath.Inlines.Clear();

            for (var n = item.Path.Count - 1; n >= 0; n--)
            {
                var run = new Run(@"/");
                ItemPath.Inlines.Add(run);

                var hyperlink = new Hyperlink(new Run(item.Path[n].Name))
                {
                    Tag = item.Path[n].ItemUri,
                    Style = FindResource(@"HyperlinkValue") as Style,
                    ToolTip = Rocks.Resources.QuickInfo_RenderPath_Navigate_to_item
                };

                hyperlink.Click += GoToItem;

                ItemPath.Inlines.Add(hyperlink);
            }
        }

        private void RenderVersions([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            Versions.Inlines.Clear();

            var context = new ContentEditorContext(ContentEditor);
            var versions = item.Versions;

            var count = versions.Count;
            if (count > 0)
            {
                var selectedVersion = item.Uri.Version;
                if (selectedVersion == Version.Latest)
                {
                    selectedVersion = new Version(item.Versions[count - 1]);
                }

                count = count > 3 ? 3 : count;

                for (var n = 0; n < count; n++)
                {
                    var version = new Version(versions[versions.Count - n - 1]);

                    var command = new SetVersion(version);

                    var hyperlink = new HyperlinkBox
                    {
                        Text = version.ToString(),
                        IsSelected = version == selectedVersion,
                        CommandParameter = context,
                        Command = command,
                        ToolTip = Rocks.Resources.QuickInfo_RenderVersions_Version + @" " + version
                    };

                    Versions.Inlines.Add(hyperlink);
                }
            }

            var more = new Hyperlink(new Run(@"..."))
            {
                Style = FindResource(@"HyperlinkValue") as Style
            };

            more.Click += VersionsClick;

            var border = new Border
            {
                Child = new TextBlock(more),
                Margin = new Thickness(2)
            };

            Versions.Inlines.Add(border);
        }

        private void ShowBaseTemplates([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var hyperlink = sender as Hyperlink;
            if (hyperlink == null)
            {
                return;
            }

            hyperlink.ContextMenu = null;

            var contextMenu = new ContextMenu();

            var context = new ContentEditorContext(ContentEditor);

            var baseTemplates = new BaseTemplatesSubmenu();
            contextMenu.Build(baseTemplates.GetSubmenuCommands(this), context);

            var commands = baseTemplates.GetSubmenuCommands(context);

            contextMenu.Build(commands, context);
            if (contextMenu.Items.Count <= 0)
            {
                return;
            }

            hyperlink.ContextMenu = contextMenu;
            contextMenu.IsOpen = true;
        }

        private void ShowContextMenu([NotNull] object sender, [NotNull] IEnumerable<Rocks.Commands.ICommand> commands, [NotNull] ContentEditorContext context)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(commands, nameof(commands));
            Debug.ArgumentNotNull(context, nameof(context));

            var hyperlink = sender as Hyperlink;
            if (hyperlink == null)
            {
                return;
            }

            hyperlink.ContextMenu = new ContextMenu();

            hyperlink.ContextMenu.Build(commands, context);

            hyperlink.ContextMenu.IsOpen = true;
        }

        private void VersionsClick([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            var context = new ContentEditorContext(ContentEditor);

            var versions = new VersionsSubmenu();
            var commands = versions.GetSubmenuCommands(context);

            ShowContextMenu(sender, commands, context);
        }
    }
}
