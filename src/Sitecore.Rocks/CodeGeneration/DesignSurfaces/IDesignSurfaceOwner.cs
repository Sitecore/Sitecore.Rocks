// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces
{
    public interface IDesignSurfaceOwner
    {
        void DragOver([NotNull] object sender, [NotNull] DragEventArgs e);

        void Drop([NotNull] object sender, [NotNull] DragEventArgs e);

        [CanBeNull]
        object GetContext();

        void LoadState([NotNull] XElement state);

        void SaveState([NotNull] XmlTextWriter output);

        void SetModifiedFlag(bool isModified);
    }
}
