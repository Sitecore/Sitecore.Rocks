// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Repositories.Dialogs;

namespace Sitecore.Rocks.UI.Repositories
{
    public class Repository
    {
        private readonly List<RepositoryEntry> entries = new List<RepositoryEntry>();

        public Repository([Localizable(false), NotNull] string repositoryName)
        {
            Assert.ArgumentNotNull(repositoryName, nameof(repositoryName));

            RepositoryName = repositoryName;

            Load();
        }

        [NotNull]
        public ICollection<RepositoryEntry> Entries
        {
            get { return entries; }
        }

        [NotNull]
        public string RepositoryName { get; }

        [NotNull]
        public string AddFile([NotNull] string title)
        {
            Assert.ArgumentNotNull(title, nameof(title));

            var dialog = new AddFileDialog(RepositoryName)
            {
                Title = title
            };

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return string.Empty;
            }

            return dialog.SelectedFileName;
        }

        public bool Edit([NotNull] string title)
        {
            Assert.ArgumentNotNull(title, nameof(title));

            var dialog = new EditRepositoriesDialog(this)
            {
                Title = title
            };

            return AppHost.Shell.ShowDialog(dialog) == true;
        }

        [NotNull]
        public IEnumerable<string> GetFiles()
        {
            return GetFiles("*.*");
        }

        [NotNull]
        public IEnumerable<string> GetFiles([Localizable(false), NotNull] string pattern)
        {
            Assert.ArgumentNotNull(pattern, nameof(pattern));

            return Entries.SelectMany(entry => entry.GetFiles(pattern));
        }

        public void Load()
        {
            entries.Clear();

            var value = AppHost.Settings.GetString("Repositories", RepositoryName, string.Empty);

            foreach (var pair in value.Split('|'))
            {
                if (string.IsNullOrEmpty(pair))
                {
                    continue;
                }

                var parts = pair.Split('^');
                if (parts.Length != 2)
                {
                    continue;
                }

                var repository = new RepositoryEntry(parts[0], parts[1]);

                entries.Add(repository);
            }
        }

        public void Save()
        {
            var sb = new StringBuilder();

            foreach (var repository in entries)
            {
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append(repository.Name);
                sb.Append('^');
                sb.Append(repository.Location);
            }

            AppHost.Settings.Set("Repositories", RepositoryName, sb.ToString());
        }
    }
}
