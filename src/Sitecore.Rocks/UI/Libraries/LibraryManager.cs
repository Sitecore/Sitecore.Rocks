// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Repositories;

namespace Sitecore.Rocks.UI.Libraries
{
    public static class LibraryManager
    {
        private static readonly List<ILibraryHandler> handlers = new List<ILibraryHandler>();

        private static readonly ObservableCollection<LibraryBase> libraries = new ObservableCollection<LibraryBase>();

        private static bool loaded;

        [NotNull]
        public static IEnumerable<LibraryBase> Libraries
        {
            get
            {
                if (!loaded)
                {
                    Load();
                }

                return libraries;
            }
        }

        public static void Add([NotNull] LibraryBase library)
        {
            Assert.ArgumentNotNull(library, nameof(library));

            libraries.Add(library);
        }

        [CanBeNull]
        public static LibraryBase AddNew([NotNull] Func<string, string, LibraryBase> create)
        {
            Assert.ArgumentNotNull(create, nameof(create));

            var repository = RepositoryManager.GetRepository(RepositoryManager.Folders);
            var library = repository.Entries.FirstOrDefault();
            if (library == null)
            {
                return null;
            }

            var name = "Library";
            do
            {
                name = AppHost.Prompt("Enter the name of the new library:", "New Library", name);
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }

                if (Libraries.Any(w => string.Compare(w.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    AppHost.MessageBox(string.Format("A library with the name '{0}' already exists.\n\nPlease choose another name.", name), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    break;
                }
            }
            while (true);

            var fileName = IO.File.GetSafeFileName(name + ".xml");
            fileName = Path.Combine(library.Path, fileName);

            var f = create(fileName, name);

            Add(f);

            return f;
        }

        public static void Delete([NotNull] LibraryBase library)
        {
            Assert.ArgumentNotNull(library, nameof(library));

            if (File.Exists(library.FileName))
            {
                File.Delete(library.FileName);
            }

            libraries.Remove(library);
        }

        public static void LoadType([NotNull] Type type, [NotNull] LibraryHandlerAttribute libraryHandlerAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(libraryHandlerAttribute, nameof(libraryHandlerAttribute));

            var instance = Activator.CreateInstance(type) as ILibraryHandler;
            if (instance == null)
            {
                return;
            }

            handlers.Add(instance);
        }

        public static void Rename([NotNull] LibraryBase library, [NotNull] string newName)
        {
            Assert.ArgumentNotNull(library, nameof(library));
            Assert.ArgumentNotNull(newName, nameof(newName));

            library.Name = newName;

            library.Save();
        }

        [CanBeNull]
        private static LibraryBase GetLibrary([NotNull] string fileName, [NotNull] XElement root)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(root, nameof(root));

            foreach (var handler in handlers)
            {
                if (handler.CanHandle(fileName, root))
                {
                    return handler.Handle(fileName, root);
                }
            }

            return null;
        }

        private static void Load()
        {
            loaded = true;

            var repository = RepositoryManager.GetRepository(RepositoryManager.Folders);

            var list = new List<LibraryBase>();

            foreach (var fileName in repository.GetFiles("*.xml"))
            {
                var file = AppHost.Files.ReadAllText(fileName);

                var root = file.ToXElement();
                if (root == null)
                {
                    continue;
                }

                var library = GetLibrary(fileName, root);
                if (library != null)
                {
                    list.Add(library);
                }
            }

            foreach (var item in list.OrderBy(i => i.Name))
            {
                Add(item);
            }
        }
    }
}
