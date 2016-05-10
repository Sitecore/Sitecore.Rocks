// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Dialogs.Prompts;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Chunks
{
    [Command(Submenu = ChunksSubmenu.Name)]
    public class ExportChunk : CommandBase
    {
        public ExportChunk()
        {
            Text = "Export...";
            Group = "Chunk";
            SortingValue = 9000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (!context.SelectedItems.Any())
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var name = string.Empty;

            var rendering = context.SelectedItems.OfType<RenderingItem>().FirstOrDefault();
            if (rendering != null)
            {
                name = rendering.GetControlId();
                if (string.IsNullOrEmpty(name))
                {
                    name = rendering.Name;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "Chunk";
            }

            name = Prompt.Show("Enter the name of the rendering chunk:", "Rendering Chunk", name);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            output.WriteStartElement("r");
            output.WriteStartElement("d");

            foreach (var layoutDesignerItem in context.SelectedItems)
            {
                layoutDesignerItem.Write(output, false);
            }

            output.WriteEndElement();
            output.WriteEndElement();

            var folder = Path.Combine(AppHost.User.SharedFolder, "RenderingChunks");
            var fileName = Path.Combine(folder, name + ".chunk.xml");

            AppHost.Files.CreateDirectory(folder);
            AppHost.Files.WriteAllText(fileName, writer.ToString(), Encoding.UTF8);
        }
    }
}
