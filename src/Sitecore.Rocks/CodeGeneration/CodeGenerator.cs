// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.CodeGeneration
{
    public abstract class CodeGenerator : ICodeGenerator
    {
        public IEditorPane Pane { get; set; }

        public abstract Control GetConfigurationUserControl();

        public abstract string GetOutput(string fileName, Site site);

        public abstract void Load(XElement root);

        public abstract void Save(XmlTextWriter output);

        public virtual void SetModifiedFlag(bool isModified)
        {
            if (Pane != null)
            {
                Pane.SetModifiedFlag(isModified);
            }
        }
    }
}
