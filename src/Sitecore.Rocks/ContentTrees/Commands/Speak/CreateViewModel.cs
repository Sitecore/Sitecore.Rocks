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
    [Command(Submenu = SpeakSubmenu.Name)]
    public class CreateViewModel : CommandBase
    {
        private static readonly char[] Backslash =
        {
            '\\'
        };

        public static readonly FieldId ModelFieldId = new FieldId(new Guid("{DDAB263E-B42E-419A-AA6F-081B7393CA38}"));

        public static readonly ItemId ViewRenderingId = new ItemId(new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}"));

        public CreateViewModel()
        {
            Text = "Create View Model";
            Group = "Renderings";
            SortingValue = 51;
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

            if (!item.ItemUri.Site.IsSitecoreVersion(SitecoreVersion.Version70))
            {
                return false;
            }

            if (item.TemplateId != ViewRenderingId)
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

                string modelName = null;
                string nameSpace = null;
                string className = null;
                string assemblyName = null;

                var modelField = item.Fields.FirstOrDefault(f => f != null && f.Name == "Model");
                if (modelField != null)
                {
                    modelName = modelField.Value;
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        var n = modelName.IndexOf(',');
                        if (n >= 0)
                        {
                            className = modelName.Left(n);
                            assemblyName = modelName.Mid(n + 1);

                            n = className.LastIndexOf('.');
                            if (n >= 0)
                            {
                                nameSpace = className.Left(n);
                                className = className.Mid(n + 1);
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(nameSpace))
                {
                    nameSpace = string.Empty;
                    var folder = Path.GetDirectoryName(path) ?? string.Empty;
                    var parts = folder.Split(Backslash, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (!string.IsNullOrEmpty(nameSpace))
                        {
                            nameSpace += ".";
                        }

                        nameSpace += part.GetSafeCodeIdentifier().Capitalize();
                    }
                }

                if (string.IsNullOrEmpty(nameSpace))
                {
                    nameSpace = "Sitecore.Models";
                }

                if (string.IsNullOrEmpty(className))
                {
                    className = selectedItem.Item.Name.GetSafeCodeIdentifier() + "RenderingModel";
                }

                if (string.IsNullOrEmpty(assemblyName))
                {
                    var outputFileName = project.OutputFileName;
                    assemblyName = Path.GetFileNameWithoutExtension(outputFileName);
                }

                GetValueCompleted<XDocument> createFile = delegate(XDocument value)
                {
                    var root = value.Root;
                    if (root == null)
                    {
                        return;
                    }

                    var output = new StringWriter();

                    output.WriteLine("//------------------------------------------------------------------------------");
                    output.WriteLine("// <auto-generated>");
                    output.WriteLine("//     This code was generated by a tool.");
                    output.WriteLine("//");
                    output.WriteLine("//     Changes to this file may cause incorrect behavior and will be lost if");
                    output.WriteLine("//     the code is regenerated.");
                    output.WriteLine("// </auto-generated>");
                    output.WriteLine("//------------------------------------------------------------------------------");
                    output.WriteLine();
                    output.WriteLine("#pragma warning disable 1591");
                    output.WriteLine();

                    output.Write("namespace ");
                    output.WriteLine(nameSpace);
                    output.WriteLine("{");

                    output.WriteLine();

                    output.WriteLine("  [System.Diagnostics.DebuggerStepThroughAttribute()]");
                    output.WriteLine("  [System.CodeDom.Compiler.GeneratedCodeAttribute(\"SitecoreRocks\", \"1.0.0.0\")]");
                    output.WriteLine("  public partial class {0} : Sitecore.Mvc.Presentation.SpeakRenderingModel", className);
                    output.WriteLine("  {");

                    var first = true;
                    var elements = root.XPathSelectElements("//field").Where(f => f.GetAttributeValue("system") == "0").OrderBy(f => f.GetAttributeValue("name")).ToList();
                    foreach (var element in elements)
                    {
                        var defaultValue = string.Empty;
                        var source = element.GetAttributeValue("source");
                        if (!string.IsNullOrEmpty(source))
                        {
                            var urlString = new UrlString(source);
                            defaultValue = urlString.Parameters["defaultvalue"] ?? string.Empty;
                        }

                        var type = "string";
                        var call = "GetString";
                        switch (element.GetAttributeValue("type").ToLowerInvariant())
                        {
                            case "checkbox":
                                type = "bool";
                                call = "GetBool";
                                defaultValue = defaultValue == "1" || string.Compare(defaultValue, "true", StringComparison.InvariantCultureIgnoreCase) == 0 ? "true" : string.Empty;
                                break;

                            case "integer":
                                type = "int";
                                call = "GetInt";
                                if (defaultValue == "0")
                                {
                                    defaultValue = string.Empty;
                                }

                                break;

                            default:
                                if (!string.IsNullOrEmpty(defaultValue))
                                {
                                    defaultValue = "\"" + defaultValue + "\"";
                                }

                                break;
                        }

                        var name = element.GetAttributeValue("name");

                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            output.WriteLine();
                        }

                        output.WriteLine("    [System.CodeDom.Compiler.GeneratedCodeAttribute(\"SitecoreRocks\", \"1.0.0.0\")]");
                        output.Write("    public {0} {1}", type, name.GetSafeCodeIdentifier());
                        output.Write(" { get { return this.");
                        output.Write(call);

                        if (!string.IsNullOrEmpty(defaultValue))
                        {
                            output.Write("(\"{0}\", {1});", name, defaultValue);
                        }
                        else
                        {
                            output.Write("(\"{0}\");", name);
                        }

                        output.WriteLine(" } }");
                    }

                    output.WriteLine("  }");
                    output.WriteLine();

                    output.WriteLine("}");
                    output.WriteLine();
                    output.WriteLine("#pragma warning restore 1591");

                    var fileName = project.FolderName;
                    if (path.StartsWith("/"))
                    {
                        path = path.Mid(1);
                    }

                    fileName = Path.Combine(fileName, path);
                    var parentFileName = fileName;
                    var modelFileName = Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, className + ".cs");

                    AppHost.Files.CreateDirectory(Path.GetDirectoryName(modelFileName));
                    AppHost.Files.WriteAllText(modelFileName, output.ToString(), Encoding.UTF8);

                    var newModelName = nameSpace + "." + className;
                    if (!string.IsNullOrEmpty(assemblyName))
                    {
                        newModelName += "," + assemblyName;
                    }

                    if (newModelName != modelName)
                    {
                        AppHost.Server.UpdateItem(selectedItem.ItemUri, ModelFieldId, newModelName);
                    }

                    UpdateSourceCode(fileName, newModelName);

                    AppHost.Projects.AddFileToProject(parentFileName, modelFileName, true);
                    AppHost.Files.OpenFile(modelFileName);
                };

                item.ItemUri.Site.DataService.GetTemplateXml(new ItemUri(item.ItemUri.DatabaseUri, new ItemId(new Guid(parametersTemplate))), true, createFile);
            };

            selectedItem.ItemUri.Site.DataService.GetItemFieldsAsync(new ItemVersionUri(selectedItem.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), completed);
        }

        private void UpdateSourceCode([NotNull] string fileName, [NotNull] string newModelName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(newModelName, nameof(newModelName));

            if (!File.Exists(fileName))
            {
                return;
            }

            var n = newModelName.IndexOf(',');
            if (n >= 0)
            {
                newModelName = newModelName.Left(n);
            }

            var content = File.ReadAllLines(fileName);

            for (var index = 0; index < content.Length; index++)
            {
                var line = content[index];
                var l = line.Trim();

                if (!l.StartsWith("@model", StringComparison.Ordinal))
                {
                    continue;
                }

                var newLine = "@model " + newModelName;

                if (l != newLine)
                {
                    content[index] = newLine;
                    File.WriteAllLines(fileName, content, Encoding.UTF8);
                }

                return;
            }
        }
    }
}
