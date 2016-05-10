// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command]
    public class CreatePageCode : CommandBase
    {
        public CreatePageCode()
        {
            Text = "Create SPEAK PageCode TypeScript File";
            Group = "PageCode";
            SortingValue = 1;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var renderingItem = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (renderingItem == null)
            {
                return false;
            }

            if (renderingItem.ItemId != "{DAFAFFB8-74AF-4141-A96A-70B16834CEC6}")
            {
                return false;
            }

            if (string.IsNullOrEmpty(renderingItem.ItemUri.Site.WebRootPath))
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

            var renderingItem = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            var rootFolder = renderingItem.ItemUri.Site.WebRootPath;
            var project = AppHost.Projects.FirstOrDefault(p => p.Site == renderingItem.ItemUri.Site);
            if (project != null)
            {
                rootFolder = project.FolderName;
            }

            var isNew = false;
            string fileName;
            if (!renderingItem.ParameterDictionary.Parameters.TryGetValue("PageCodeScriptFileName", out fileName))
            {
                var dialog = new SaveFileDialog
                {
                    Title = "PageCode TypeScript",
                    CheckPathExists = true,
                    OverwritePrompt = true,
                    InitialDirectory = rootFolder,
                    FileName = "PageCode.ts",
                    DefaultExt = "ts",
                    Filter = @"TypeScript (*.ts)|*.ts|All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                fileName = dialog.FileName;
                isNew = true;

                var relativeFileName = fileName;
                if (relativeFileName.StartsWith(rootFolder, StringComparison.InvariantCultureIgnoreCase))
                {
                    relativeFileName = relativeFileName.Mid(rootFolder.Length);

                    relativeFileName = relativeFileName.Replace("\\", "/");
                    if (!relativeFileName.StartsWith("/"))
                    {
                        relativeFileName = "/" + relativeFileName;
                    }
                }

                renderingItem.SetParameterValue("PageCodeScriptFileName", relativeFileName);
            }

            var className = Path.GetFileNameWithoutExtension(fileName).GetSafeCodeIdentifier();

            var output = new StringWriter();
            output.WriteLine("  // #region Public Properties");

            if (renderingItem.RenderingContainer != null)
            {
                foreach (var rendering in renderingItem.RenderingContainer.Renderings)
                {
                    var id = rendering.GetControlId();
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    output.WriteLine("  public {0}: any;", id);
                }
            }

            output.WriteLine("  // #endregion");

            if (File.Exists(fileName))
            {
                UpdateScriptFile(fileName, className, output.ToString());
            }
            else
            {
                CreateScriptFile(fileName, className, output.ToString());
            }

            if (isNew && fileName.StartsWith(rootFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                var parentFileName = Path.GetDirectoryName(fileName);
                AppHost.Projects.AddFileToProject(parentFileName, fileName, false);
            }

            AppHost.Files.OpenFile(fileName);
        }

        private void CreateScriptFile([NotNull] string scriptFileName, [NotNull] string className, [NotNull] string propertiesBlock)
        {
            Debug.ArgumentNotNull(scriptFileName, nameof(scriptFileName));
            Debug.ArgumentNotNull(className, nameof(className));
            Debug.ArgumentNotNull(propertiesBlock, nameof(propertiesBlock));

            var output = new StringWriter();

            output.WriteLine("import $ = require(\"jquery\");");
            output.WriteLine("import Speak = require(\"sitecore/shell/client/Bootstrap3/Assets/lib/typescript/SitecoreSpeak\");");
            output.WriteLine();
            output.WriteLine("class " + className + " extends Speak.PageCode {");
            output.WriteLine(propertiesBlock);
            output.WriteLine("  initialize() {");
            output.WriteLine("  }");
            output.WriteLine("}");
            output.WriteLine();
            output.WriteLine("Sitecore.pageCode([], new " + className + "());");

            AppHost.Files.WriteAllText(scriptFileName, output.ToString(), Encoding.UTF8);
        }

        private void UpdateScriptFile([NotNull] string scriptFileName, [NotNull] string className, [NotNull] string propertiesBlock)
        {
            Debug.ArgumentNotNull(scriptFileName, nameof(scriptFileName));
            Debug.ArgumentNotNull(className, nameof(className));
            Debug.ArgumentNotNull(propertiesBlock, nameof(propertiesBlock));

            var lines = File.ReadAllLines(scriptFileName).ToList();

            var startLine = -1;
            var endLine = -1;

            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                if (line == "// #region Public Properties")
                {
                    startLine = index;
                }
                else if (line == "// #endregion" && startLine >= 0)
                {
                    endLine = index;
                    break;
                }
            }

            if (startLine < 0 || endLine < 0)
            {
                if (AppHost.MessageBox("The TypeScript file already exists, but the Public Properties region is missing.\n\nDo you want to overwrite the existing file?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    CreateScriptFile(scriptFileName, className, propertiesBlock);
                }

                return;
            }

            for (var index = endLine; index >= startLine; index--)
            {
                lines.RemoveAt(index);
            }

            lines.Insert(startLine, propertiesBlock);

            File.WriteAllLines(scriptFileName, lines, Encoding.UTF8);
        }
    }
}
