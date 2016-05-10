// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.ObjectModel;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.Projects.Dialogs
{
    public partial class ProjectLogWindow
    {
        public delegate void ProcessCallback();

        private readonly ObservableCollection<LogLine> lines = new ObservableCollection<LogLine>();

        private readonly ListViewSorter listViewSorter;

        public ProjectLogWindow()
        {
            InitializeComponent();
            this.InitializeDialog();

            ListView.ItemsSource = lines;

            Loaded += ControlLoaded;

            listViewSorter = new ListViewSorter(ListView);
        }

        [CanBeNull]
        public ProcessCallback AutoStart { get; set; }

        public long Maximum
        {
            get { return (long)Progress.Maximum; }

            set
            {
                Progress.Maximum = value;
                ProgressPanel.Visibility = value != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public long Value
        {
            get { return (long)Progress.Value; }

            set { Progress.Value = value; }
        }

        public void Finish()
        {
            CloseButton.Content = Rocks.Resources.Close;
        }

        public void Increment()
        {
            Value++;
        }

        public void Write([NotNull] string path, [NotNull] string text, [NotNull] string comment)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(comment, nameof(comment));

            lines.Add(new LogLine(path, text, comment));
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            if (AutoStart != null)
            {
                AutoStart();
            }
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        internal class LogLine
        {
            public LogLine()
            {
            }

            public LogLine([NotNull] string path, [NotNull] string text, [NotNull] string comment)
            {
                Assert.ArgumentNotNull(path, nameof(path));
                Assert.ArgumentNotNull(text, nameof(text));
                Assert.ArgumentNotNull(comment, nameof(comment));

                Path = path;
                Text = text;
                Comment = comment;
            }

            [NotNull]
            public string Comment { get; private set; }

            [NotNull]
            public string Path { get; private set; }

            [NotNull]
            public string Text { get; private set; }
        }
    }
}
