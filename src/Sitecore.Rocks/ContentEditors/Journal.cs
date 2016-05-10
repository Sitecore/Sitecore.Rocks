// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public class Journal<T> where T : class
    {
        private readonly List<T> journal;

        private int current;

        public Journal()
        {
            journal = new List<T>();
            current = -1;
        }

        public bool CanGoBack
        {
            get { return current > 0; }
        }

        public bool CanGoForward
        {
            get { return current < journal.Count - 1; }
        }

        [NotNull]
        public IEnumerable<T> Entries
        {
            get { return journal; }
        }

        public int Max { get; set; }

        public bool RemoveDuplicates { get; set; }

        public void Clear()
        {
            journal.Clear();
        }

        [NotNull]
        public IEnumerable<T> GetHistory()
        {
            for (var n = current; n >= 0; n--)
            {
                yield return journal[n];
            }
        }

        [CanBeNull]
        public T GoBack()
        {
            if (current == 0)
            {
                return null;
            }

            current--;

            return journal[current];
        }

        [NotNull]
        public T GoForward()
        {
            current++;

            return journal[current];
        }

        [CanBeNull]
        public T Peek()
        {
            return current >= 0 ? journal[current] : default(T);
        }

        public void Push([NotNull] T element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            if (RemoveDuplicates)
            {
                if (journal.Contains(element))
                {
                    journal.Remove(element);
                }
            }

            while (journal.Count - 1 > current)
            {
                journal.RemoveAt(journal.Count - 1);
            }

            journal.Add(element);

            if (Max > 0)
            {
                while (journal.Count > Max)
                {
                    journal.RemoveAt(0);
                }
            }

            current = journal.Count - 1;
        }
    }
}
