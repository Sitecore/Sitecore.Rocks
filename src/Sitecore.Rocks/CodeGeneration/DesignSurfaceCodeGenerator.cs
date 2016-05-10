// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.DesignSurfaces;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration
{
    public abstract class DesignSurfaceCodeGenerator : CodeGenerator, IDesignSurfaceOwner
    {
        private DesignSurface designSurface;

        protected DesignSurfaceCodeGenerator()
        {
            EmptyText = string.Empty;
        }

        [NotNull]
        protected DesignSurface DesignSurface
        {
            get
            {
                if (designSurface == null)
                {
                    designSurface = new DesignSurface(this, EmptyText);
                }

                return designSurface;
            }
        }

        [NotNull]
        protected string EmptyText { get; set; }

        public abstract void DragOver(object sender, DragEventArgs e);

        public abstract void Drop(object sender, DragEventArgs e);

        public override Control GetConfigurationUserControl()
        {
            return DesignSurface;
        }

        public virtual object GetContext()
        {
            return new DesignSurfaceContext(DesignSurface);
        }

        public override void Load(XElement root)
        {
            Assert.ArgumentNotNull(root, nameof(root));

            DesignSurface.Clear();

            LoadState(root);

            SetModifiedFlag(false);
            DesignSurface.ClearJournal();
            DesignSurface.AddToJournal();
        }

        public abstract void LoadState(XElement state);

        public override void Save(XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            SaveState(output);

            SetModifiedFlag(false);
        }

        public abstract void SaveState(XmlTextWriter output);

        public override void SetModifiedFlag(bool isModified)
        {
            base.SetModifiedFlag(isModified);

            if (isModified)
            {
                DesignSurface.AddToJournal();
            }
        }

        [NotNull]
        protected IEnumerable<IItem> GetItems([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return Enumerable.Empty<IItem>();
            }

            var result = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (result == null)
            {
                return Enumerable.Empty<IItem>();
            }

            return result;
        }
    }
}
