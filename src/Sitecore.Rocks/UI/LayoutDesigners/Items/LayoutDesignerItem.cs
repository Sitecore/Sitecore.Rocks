// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell.ComponentModel;

namespace Sitecore.Rocks.UI.LayoutDesigners.Items
{
    public abstract class LayoutDesignerItem : DynamicTypeDescriptor
    {
        public abstract void Commit();

        public event EventHandler Modified;

        public void RaiseModified()
        {
            var modified = Modified;
            if (modified != null)
            {
                modified(this, EventArgs.Empty);
            }
        }

        public abstract void Write([NotNull] XmlTextWriter output, bool isCopy);
    }
}
