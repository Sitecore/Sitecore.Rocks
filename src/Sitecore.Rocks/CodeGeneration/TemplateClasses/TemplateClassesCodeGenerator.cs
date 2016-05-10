// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.TemplateClasses.Models;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses
{
    [CodeGenerator("Template Class")]
    public class TemplateClassesCodeGenerator : DesignSurfaceCodeGenerator, IShapeCreator
    {
        public TemplateClassesCodeGenerator()
        {
            EmptyText = "The generator allows you to auto-generate code classes for Sitecore templates.\n\nCreate code classes by dragging templates from the Sitecore Explorer onto the design surface.";
        }

        public virtual Template CreateTemplate(IShapeCreator shapeCreator, ItemUri templateUri)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            return new Template(shapeCreator, templateUri);
        }

        public virtual TemplateField CreateTemplateField(IShapeCreator shapeCreator)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));

            return new TemplateField();
        }

        public virtual TemplateSection CreateTemplateSection(IShapeCreator shapeCreator)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));

            return new TemplateSection(shapeCreator);
        }

        public override void DragOver(object sender, DragEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            var items = GetItems(e).ToList();
            if (!items.Any())
            {
                return;
            }

            foreach (var item in items)
            {
                var templatedItem = item as ITemplatedItem;
                if (templatedItem == null)
                {
                    return;
                }

                if (IdManager.GetTemplateType(templatedItem.TemplateId) != "template")
                {
                    return;
                }

                if (DesignSurface.Shapes.Select(s => s.ShapeContent).Cast<TemplateShapeContent>().Any(i => i.Template.TemplateUri.ItemId == item.ItemUri.ItemId))
                {
                    return;
                }
            }

            e.Effects = DragDropEffects.Copy;
        }

        public override void Drop(object sender, DragEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            var items = GetItems(e).ToList();
            if (!items.Any())
            {
                return;
            }

            var position = e.GetPosition(DesignSurface);

            foreach (var item in items)
            {
                var result = DesignSurface.CreateShape(new TemplateShapeContent(this, item.ItemUri, item.Name));

                result.SetPosition(position);

                position.Offset(16, 16);
            }

            SetModifiedFlag(true);
        }

        [NotNull]
        public override object GetContext()
        {
            return new TemplateClassesContext(DesignSurface);
        }

        public override string GetOutput(string fileName, Site site)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var xslt = Path.ChangeExtension(fileName, @".scx.xslt");
            if (!File.Exists(xslt))
            {
                return string.Format("// The code transformation file \"{0}\" was not found", xslt);
            }

            var fileContents = AppHost.Files.ReadAllText(xslt);

            var xmlReader = new XmlTextReader(fileName);
            var xpathDocument = new XPathDocument(xmlReader);

            var xsltReader = new XmlTextReader(new StringReader(fileContents));

            var transform = new XslCompiledTransform();
            transform.Load(xsltReader);

            var writer = new StringBuilder();
            TextWriter output = new StringWriter(writer);

            transform.Transform(xpathDocument, null, output);

            return writer.ToString();
        }

        public override void LoadState(XElement state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            foreach (var element in state.Elements())
            {
                DesignSurface.LoadShape(element, new TemplateShapeContent(this));
            }
        }

        public override void SaveState(XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            foreach (var shape in DesignSurface.Shapes)
            {
                shape.Save(output);
            }
        }
    }
}
