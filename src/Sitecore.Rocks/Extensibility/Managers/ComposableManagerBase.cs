// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.Extensibility.Managers
{
    public abstract class ComposableManagerBase<T> : ManagerBase, IEnumerable<T> where T : class
    {
        protected ComposableManagerBase()
        {
            Items = new List<T>();
            Populators = new List<IComposableManagerPopulator<T>>();
        }

        public virtual int Count => Items.Count;

        [NotNull, ImportMany]
        protected IList<T> Items { get; }

        [NotNull, ImportMany]
        protected IList<IComposableManagerPopulator<T>> Populators { get; }

        public virtual void Add([NotNull] T item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Items.Add(item);
        }

        public virtual void Clear()
        {
            Items.Clear();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public virtual void Refresh()
        {
            Clear();

            AppHost.Extensibility.ComposeParts(this);

            foreach (var populator in Populators)
            {
                foreach (var item in populator.Populate())
                {
                    Items.Add(item);
                }
            }
        }

        public virtual void Remove([NotNull] T item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Items.Remove(item);
        }

        protected override void Initialize()
        {
            Refresh();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Items).GetEnumerator();
        }
    }
}
