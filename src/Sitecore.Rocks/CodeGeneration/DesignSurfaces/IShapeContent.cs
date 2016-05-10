// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces
{
    public interface IShapeContent
    {
        [NotNull]
        string Header { get; }

        void Initialize([NotNull] IShape shape);

        void Load([NotNull] XElement element);

        void Save([NotNull] XmlTextWriter output);
    }
}
