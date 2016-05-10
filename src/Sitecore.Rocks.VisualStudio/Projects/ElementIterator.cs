// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects
{
    public class ElementIterator<T>
    {
        public delegate void ElementIteratorDelegate(T element);

        private readonly IEnumerable<T> items;

        private int current;

        public ElementIterator([NotNull] IEnumerable<T> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            this.items = items;
        }

        [CanBeNull]
        public EventHandler Finish { get; set; }

        [NotNull]
        public ElementIteratorDelegate Process { get; set; }

        public void Next()
        {
            if (current >= items.Count())
            {
                var finish = Finish;
                if (finish != null)
                {
                    finish(this, EventArgs.Empty);
                }

                return;
            }

            var element = items.ElementAt(current);

            current++;

            Process(element);
        }

        public void Start()
        {
            current = 0;

            Next();
        }
    }
}
