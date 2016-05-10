// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.UI.StartPage
{
    public partial class StartPageGroup
    {
        public StartPageGroup([NotNull] StartPageViewer startPage, [NotNull] string parentName, [NotNull] string title, [NotNull] string description)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));
            Assert.ArgumentNotNull(parentName, nameof(parentName));
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(description, nameof(description));

            InitializeComponent();

            StartPage = startPage;
            ParentName = parentName;
            Title = title;
            Description = description;

            RenderGroup();
        }

        [NotNull]
        public string Description { get; }

        [NotNull]
        public string ParentName { get; }

        [NotNull]
        public StartPageViewer StartPage { get; }

        [NotNull]
        public string Title { get; }

        private void Execute([NotNull] CommandDescriptor command)
        {
            Debug.ArgumentNotNull(command, nameof(command));

            var startPage = this.GetAncestor<StartPageViewer>();
            if (startPage == null)
            {
                return;
            }

            var context = new StartPageContext(startPage);

            command.Command.Execute(context);
        }

        private void RenderGroup()
        {
            TitleTextBlock.Text = Title;
            DescriptionTextBlock.Text = Description;

            var descriptors = new List<CommandDescriptor>();

            foreach (var command in StartPageManager.Commands)
            {
                if (command.Attribute.ParentName != ParentName)
                {
                    continue;
                }

                descriptors.Add(new CommandDescriptor(command.Command, command.Attribute));
            }

            foreach (var cmd in CommandManager.Commands)
            {
                var command = cmd as IStartPageCommand;
                if (command == null)
                {
                    continue;
                }

                var attributes = command.GetType().GetCustomAttributes(typeof(StartPageCommandAttribute), false);
                if (attributes.Length == 0)
                {
                    continue;
                }

                foreach (var attribute in attributes.OfType<StartPageCommandAttribute>())
                {
                    if (attribute.ParentName != ParentName)
                    {
                        continue;
                    }

                    descriptors.Add(new CommandDescriptor(command, attribute));
                }
            }

            if (descriptors.Count == 0)
            {
                return;
            }

            descriptors.Sort((d1, d2) => d1.Attribute.Priority == d2.Attribute.Priority ? string.Compare(d1.Attribute.Text, d2.Attribute.Text, StringComparison.InvariantCultureIgnoreCase) : d1.Attribute.Priority >= d2.Attribute.Priority ? 1 : -1);

            foreach (var descriptor in descriptors)
            {
                var textBlock = new TextBlock
                {
                    FontSize = 12,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(descriptor.Attribute.Text);
                textBlock.Inlines.Add(hyperlink);

                var d = descriptor;
                hyperlink.Click += (sender, args) => Execute(d);

                // hyperlink.Foreground = descriptor.Command.CanExecute(context) ? StartPageViewer.EnabledLink : StartPageViewer.DisabledLink;
                List.Children.Add(textBlock);

                StartPage.AddHyperlink(descriptor.Command, hyperlink);
            }
        }

        public class CommandDescriptor
        {
            public CommandDescriptor([NotNull] IStartPageCommand command, [NotNull] StartPageCommandAttribute attribute)
            {
                Assert.ArgumentNotNull(command, nameof(command));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                Command = command;
                Attribute = attribute;
            }

            [NotNull]
            public StartPageCommandAttribute Attribute { get; }

            [NotNull]
            public IStartPageCommand Command { get; }
        }
    }
}
