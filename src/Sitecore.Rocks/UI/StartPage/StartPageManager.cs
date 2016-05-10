// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.Repositories;
using Sitecore.Rocks.UI.StartPage.Externals;

namespace Sitecore.Rocks.UI.StartPage
{
    public static class StartPageManager
    {
        private static readonly List<StartPageCommandDescriptor> commands = new List<StartPageCommandDescriptor>();

        private static readonly List<StartPageControlDescriptor> controls = new List<StartPageControlDescriptor>();

        [NotNull]
        public static IEnumerable<StartPageCommandDescriptor> Commands
        {
            get
            {
                if (!IsLoaded)
                {
                    Load();
                }

                return commands;
            }
        }

        [NotNull]
        public static IEnumerable<StartPageControlDescriptor> Controls
        {
            get
            {
                if (!IsLoaded)
                {
                    Load();
                }

                return controls;
            }
        }

        public static bool IsLoaded { get; set; }

        public static void LoadType([NotNull] Type type, [NotNull] StartPageControlAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var descriptor = new StartPageControlDescriptor(type, attribute);

            controls.Add(descriptor);
        }

        public static void Reload()
        {
            controls.RemoveAll(i => i is StartPageGroupDescriptor || i is StartPageListDescriptor || i is StartPageTabDescriptor);
            commands.Clear();

            IsLoaded = false;
        }

        private static void Load()
        {
            IsLoaded = true;

            var repository = RepositoryManager.GetRepository(RepositoryManager.StartPages);
            foreach (var fileName in repository.GetFiles("*.xml"))
            {
                LoadFile(fileName);
            }
        }

        private static void LoadFile([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            if (!File.Exists(fileName))
            {
                return;
            }

            var data = AppHost.Files.ReadAllText(fileName);

            var root = data.ToXElement();
            if (root == null)
            {
                return;
            }

            ParseElements(root, string.Empty);
        }

        private static void ParseCommand([NotNull] XElement element, [NotNull] string parentName)
        {
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(parentName, nameof(parentName));

            var text = element.Value;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var priority = element.GetAttributeDouble("priority", 1000);
            var attribute = new StartPageCommandAttribute(text, parentName, priority);

            var command = new ExternaStartPagelCommand(element.GetAttributeValue("href"));

            var descriptor = new StartPageCommandDescriptor(command, attribute);

            commands.Add(descriptor);
        }

        private static void ParseElement([NotNull] XElement element, [NotNull] string elementName)
        {
            Debug.ArgumentNotNull(element, nameof(element));
            Debug.ArgumentNotNull(elementName, nameof(elementName));

            var name = element.GetAttributeValue("name");
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var text = element.GetAttributeValue("header");
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var parentName = elementName + (string.IsNullOrEmpty(elementName) ? string.Empty : @".") + name;

            if (controls.All(c => c.Attribute.ParentName != parentName))
            {
                var priority = element.GetAttributeDouble("priority", 1000);

                StartPageControlDescriptor descriptor = null;

                var attribute = new StartPageControlAttribute(elementName, priority);

                switch (element.Name.ToString())
                {
                    case "tab":
                        descriptor = new StartPageTabDescriptor(typeof(ExternalStartPageTab), attribute, parentName, text);
                        break;
                    case "page":
                        descriptor = new StartPageListDescriptor(typeof(ExternalStartPageTab), attribute, parentName, text);
                        break;
                    case "group":
                        descriptor = new StartPageGroupDescriptor(typeof(ExternalStartPageGroup), attribute, parentName, text, element.GetAttributeValue("text"));
                        break;
                }

                if (descriptor != null)
                {
                    controls.Add(descriptor);
                }
            }

            ParseElements(element, parentName);
        }

        private static void ParseElements([NotNull] XElement parentElement, [NotNull] string parentName)
        {
            Debug.ArgumentNotNull(parentElement, nameof(parentElement));
            Debug.ArgumentNotNull(parentName, nameof(parentName));

            foreach (var element in parentElement.Elements())
            {
                if (element.Name == @"command")
                {
                    ParseCommand(element, parentName);
                }
                else
                {
                    ParseElement(element, parentName);
                }
            }
        }

        public class StartPageCommandDescriptor
        {
            public StartPageCommandDescriptor([NotNull] IStartPageCommand command, [NotNull] StartPageCommandAttribute attribute)
            {
                Assert.ArgumentNotNull(command, nameof(command));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                Command = command;
                Attribute = attribute;
            }

            [NotNull]
            public StartPageCommandAttribute Attribute { get; private set; }

            [NotNull]
            public IStartPageCommand Command { get; private set; }
        }

        public class StartPageControlDescriptor
        {
            public StartPageControlDescriptor([NotNull] Type type, [NotNull] StartPageControlAttribute attribute)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                Type = type;
                Attribute = attribute;
            }

            [NotNull]
            public StartPageControlAttribute Attribute { get; }

            [CanBeNull]
            public string Text { get; set; }

            [NotNull]
            public Type Type { get; }

            [CanBeNull]
            public virtual IStartPageControl GetInstance([NotNull] StartPageViewer startPage)
            {
                Assert.ArgumentNotNull(startPage, nameof(startPage));

                var result = Activator.CreateInstance(Type, startPage) as IStartPageControl;

                return result;
            }
        }

        public class StartPageGroupDescriptor : StartPageControlDescriptor
        {
            public StartPageGroupDescriptor([NotNull] Type type, [NotNull] StartPageControlAttribute attribute, [NotNull] string parentName, [NotNull] string text, [NotNull] string description) : base(type, attribute)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(attribute, nameof(attribute));
                Assert.ArgumentNotNull(parentName, nameof(parentName));
                Assert.ArgumentNotNull(text, nameof(text));
                Assert.ArgumentNotNull(description, nameof(description));

                ParentName = parentName;
                Text = text;
                Description = description;
            }

            [NotNull]
            public string Description { get; set; }

            [NotNull]
            public string ParentName { get; }

            public override IStartPageControl GetInstance(StartPageViewer startPage)
            {
                Assert.ArgumentNotNull(startPage, nameof(startPage));

                return new ExternalStartPageGroup(startPage, ParentName)
                {
                    Text = Text ?? string.Empty,
                    Description = Description
                };
            }
        }

        public class StartPageListDescriptor : StartPageControlDescriptor
        {
            public StartPageListDescriptor([NotNull] Type type, [NotNull] StartPageControlAttribute attribute, [NotNull] string parentName, [NotNull] string text) : base(type, attribute)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(attribute, nameof(attribute));
                Assert.ArgumentNotNull(parentName, nameof(parentName));
                Assert.ArgumentNotNull(text, nameof(text));

                ParentName = parentName;
                Text = text;
            }

            [NotNull]
            public string ParentName { get; }

            public override IStartPageControl GetInstance(StartPageViewer startPage)
            {
                Assert.ArgumentNotNull(startPage, nameof(startPage));

                return new ExternalStartPageList(startPage, ParentName)
                {
                    Text = Text ?? string.Empty
                };
            }
        }

        public class StartPageTabDescriptor : StartPageControlDescriptor
        {
            public StartPageTabDescriptor([NotNull] Type type, [NotNull] StartPageControlAttribute attribute, [NotNull] string parentName, [NotNull] string text) : base(type, attribute)
            {
                Assert.ArgumentNotNull(type, nameof(type));
                Assert.ArgumentNotNull(attribute, nameof(attribute));
                Assert.ArgumentNotNull(parentName, nameof(parentName));
                Assert.ArgumentNotNull(text, nameof(text));

                ParentName = parentName;
                Text = text;
            }

            [NotNull]
            public string ParentName { get; }

            public override IStartPageControl GetInstance(StartPageViewer startPage)
            {
                Assert.ArgumentNotNull(startPage, nameof(startPage));

                return new ExternalStartPageTab(startPage, ParentName)
                {
                    Text = Text ?? string.Empty,
                    TabStyle = TabStyle.Page
                };
            }
        }
    }
}
