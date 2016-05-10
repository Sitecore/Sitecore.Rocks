// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.CodeGeneration
{
    public interface ICodeGenerator
    {
        [CanBeNull]
        IEditorPane Pane { get; set; }

        [CanBeNull]
        Control GetConfigurationUserControl();

        [CanBeNull]
        string GetOutput([NotNull] string fileName, [CanBeNull] Site site);

        void Load([NotNull] XElement root);

        void Save([NotNull] XmlTextWriter output);
    }
}
