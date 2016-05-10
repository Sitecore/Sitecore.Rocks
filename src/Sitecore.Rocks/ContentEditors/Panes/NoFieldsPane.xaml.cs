// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Commands.Views;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panes
{
    public partial class NoFieldsPane
    {
        public NoFieldsPane([NotNull] ContentEditor contentEditor)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            InitializeComponent();
            ContentEditor = contentEditor;
        }

        public ContentEditor ContentEditor { get; }

        private void ShowStandardFields([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = new ContentEditorContext(ContentEditor);

            var command = new StandardFields();

            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }
    }
}
