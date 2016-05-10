// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces
{
    public interface IShape
    {
        bool IsSelected { get; set; }

        [NotNull]
        IShapeContent ShapeContent { get; }

        Point GetPosition();

        event MouseButtonEventHandler HeaderClick;

        void Load([NotNull] XElement element);

        void Save([NotNull] XmlTextWriter output);

        void SetPosition(Point position);
    }
}
