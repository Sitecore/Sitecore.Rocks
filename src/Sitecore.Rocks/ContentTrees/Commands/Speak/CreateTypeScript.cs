// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    /// <summary>Defines the view command class.</summary>
    [Command(Submenu = SpeakSubmenu.Name)]
    public class CreateTypeScript : CommandBase
    {
        public static readonly FieldId ModelFieldId = new FieldId(new Guid("{DDAB263E-B42E-419A-AA6F-081B7393CA38}"));

        public static readonly ItemId ViewRenderingId = new ItemId(new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}"));

        public CreateTypeScript()
        {
            Text = "Create TypeScript File";
            Group = "Renderings";
            SortingValue = 52;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (item.TemplateId != ViewRenderingId)
            {
                return false;
            }

            if (!item.ItemUri.Site.IsSitecoreVersion(SitecoreVersion.Version70))
            {
                return false;
            }

            if (string.IsNullOrEmpty(item.ItemUri.Site.WebRootPath))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var selectedItem = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (selectedItem == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(selectedItem.ItemUri.Site.WebRootPath))
            {
                return;
            }

            GetValueCompleted<Item> completed = delegate(Item item)
            {
                var pathField = item.Fields.FirstOrDefault(f => f != null && f.Name == "Path");
                if (pathField == null)
                {
                    AppHost.MessageBox("Rendering does not have a Path field.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var path = pathField.Value;
                if (string.IsNullOrEmpty(path))
                {
                    AppHost.MessageBox("Rendering does not have a value in the Path field.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var project = AppHost.Projects.GetProjectContainingFileName(selectedItem.Site, path);
                if (project == null)
                {
                    AppHost.MessageBox("Could not find the target file name in any Visual Studio project.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var parametersTemplateField = item.Fields.FirstOrDefault(f => f != null && f.Name == "Parameters Template");
                if (parametersTemplateField == null)
                {
                    AppHost.MessageBox("Rendering does not have a Parameters template field.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var parametersTemplate = parametersTemplateField.Value;
                if (string.IsNullOrEmpty(parametersTemplate))
                {
                    AppHost.MessageBox("Rendering does not have a Parameters template.\n\nYou must first create or assign a Parameters Template.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                GetValueCompleted<XDocument> createFile = delegate(XDocument value)
                {
                    var root = value.Root;
                    if (root == null)
                    {
                        return;
                    }

                    var output = new StringWriter();

                    output.WriteLine("  // #region Public Properties");
                    output.WriteLine("  // This region was generated by a tool. Changes to this region may cause incorrect behavior and will be lost if the code is regenerated.");

                    var elements = root.XPathSelectElements("//field").Where(f => f.GetAttributeValue("system") == "0").OrderBy(f => f.GetAttributeValue("name")).ToList();
                    foreach (var element in elements)
                    {
                        var source = element.GetAttributeValue("source");
                        if (!string.IsNullOrEmpty(source))
                        {
                            var urlString = new UrlString(source);

                            var bindMode = urlString.Parameters["bindmode"] ?? string.Empty;
                            if (string.Compare(bindMode, "server", StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                continue;
                            }
                        }

                        var type = "string";
                        switch (element.GetAttributeValue("type").ToLowerInvariant())
                        {
                            case "checkbox":
                                type = "boolean";
                                break;

                            case "integer":
                                type = "number";
                                break;
                        }

                        var name = element.GetAttributeValue("name");
                        output.WriteLine("  public {0}: {1};", name.GetSafeCodeIdentifier(), type);
                    }

                    output.WriteLine("  // #endregion");

                    path = path.Replace("/", "\\");
                    if (path.StartsWith("\\"))
                    {
                        path = path.Mid(1);
                    }

                    var parentFileName = Path.Combine(project.FolderName, path);
                    var scriptFileName = Path.ChangeExtension(parentFileName, "ts");

                    if (File.Exists(scriptFileName))
                    {
                        UpdateScriptFile(scriptFileName, output.ToString());
                    }
                    else
                    {
                        CreateScriptFile(scriptFileName, output.ToString());
                    }

                    AppHost.Projects.AddFileToProject(parentFileName, scriptFileName, false);
                    AppHost.Files.OpenFile(scriptFileName);
                };

                item.ItemUri.Site.DataService.GetTemplateXml(new ItemUri(item.ItemUri.DatabaseUri, new ItemId(new Guid(parametersTemplate))), true, createFile);
            };

            selectedItem.ItemUri.Site.DataService.GetItemFieldsAsync(new ItemVersionUri(selectedItem.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), completed);
        }

        private void CreateScriptFile([NotNull] string scriptFileName, [NotNull] string propertiesBlock)
        {
            Debug.ArgumentNotNull(scriptFileName, nameof(scriptFileName));
            Debug.ArgumentNotNull(propertiesBlock, nameof(propertiesBlock));

            var componentName = Path.GetFileNameWithoutExtension(scriptFileName);
            var className = componentName.GetSafeCodeIdentifier();

            var output = new StringWriter();

            output.WriteLine("import Speak = require(\"sitecore/shell/client/Speak/Assets/lib/core/1.2/SitecoreSpeak\");");
            output.WriteLine();
            output.WriteLine("class {0} extends Speak.ControlBase {{", className);

            output.WriteLine(propertiesBlock);

            output.WriteLine("  initialize(options: ComponentOptions, app: Application, el: Element, sitecore: SitecoreSpeak) {");
            output.WriteLine("  }");

            output.WriteLine("}");
            output.WriteLine();

            if (className != componentName)
            {
                output.WriteLine("Sitecore.component({0});", className);
            }
            else
            {
                output.WriteLine("Sitecore.component({0}, \"{1}\");", className, componentName);
            }

            AppHost.Files.WriteAllText(scriptFileName, output.ToString(), Encoding.UTF8);
        }

        private void UpdateScriptFile([NotNull] string scriptFileName, [NotNull] string propertiesBlock)
        {
            Debug.ArgumentNotNull(scriptFileName, nameof(scriptFileName));
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
                    CreateScriptFile(scriptFileName, propertiesBlock);
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
