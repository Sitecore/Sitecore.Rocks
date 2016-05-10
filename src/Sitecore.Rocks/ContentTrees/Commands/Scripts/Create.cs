// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Scripts
{
    [Command(Submenu = "Script"), CommandId(CommandIds.SitecoreExplorer.ScriptCreate, typeof(ContentTreeContext), Text = "Script Create"), Feature(FeatureNames.Scripting)]
    public class Create : ScriptCommand
    {
        public Create()
        {
            Text = Resources.Create_Create_Create;
            Group = "Create";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            var templateId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template");
            foreach (var item in context.Items)
            {
                var templatedItem = item as ITemplatedItem;
                if (templatedItem == null)
                {
                    return false;
                }

                if (templatedItem.TemplateId != templateId)
                {
                    return false;
                }
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var items = context.Items.ToList();
            var itemList = new List<XDocument>();

            GetValueCompleted<XDocument> c = delegate(XDocument value)
            {
                itemList.Add(value);
                if (itemList.Count != items.Count)
                {
                    return;
                }

                var databaseUri = items.First().ItemUri.DatabaseUri;

                var script = GetScript(itemList);

                OpenQueryAnalyzer(databaseUri, script);
            };

            foreach (var item in items)
            {
                item.ItemUri.Site.DataService.GetTemplateXml(item.ItemUri, false, c);
            }
        }

        [NotNull]
        protected string GetScript([NotNull] List<XDocument> docs)
        {
            Debug.ArgumentNotNull(docs, nameof(docs));

            var output = new StringWriter();

            foreach (var doc in docs)
            {
                Render(output, doc);
            }

            return output.ToString();
        }

        protected override string GetScript(List<IItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            return string.Empty;
        }

        private void Render([NotNull] StringWriter output, [NotNull] XDocument doc)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(doc, nameof(doc));

            var template = doc.Root;
            if (template == null)
            {
                return;
            }

            var templateName = template.GetAttributeValue("name");
            var templatePath = template.GetAttributeValue("path");

            var n = templatePath.LastIndexOf('/');
            if (n >= 0)
            {
                templatePath = templatePath.Left(n);
            }

            output.WriteLine(@"create template #{0}# {1}", templateName, FormatPath(templatePath));
            output.WriteLine(@"(");

            var lineCount = template.Elements().Sum(sectionElement => sectionElement.Elements().Count());
            var count = 0;

            foreach (var sectionElement in template.Elements())
            {
                var sectionName = sectionElement.GetAttributeValue("name");

                var fields = sectionElement.Elements();
                foreach (var fieldElement in fields)
                {
                    var fieldName = fieldElement.GetAttributeValue("name");
                    var fieldType = fieldElement.GetAttributeValue("type");
                    var shared = fieldElement.GetAttributeValue("shared") == @"1";
                    var unversioned = fieldElement.GetAttributeValue("unversioned") == @"1";

                    output.Write(@"  #{0}# #{1}#", fieldName, fieldType);

                    if (shared)
                    {
                        output.Write(@" shared");
                    }
                    else if (unversioned)
                    {
                        output.Write(@" unversioned");
                    }

                    if (sectionName != @"Data")
                    {
                        output.Write(@" section #{0}#", sectionName);
                    }

                    if (count < lineCount - 1)
                    {
                        output.Write(@",");
                    }

                    count++;

                    output.WriteLine();
                }
            }

            output.WriteLine(@");");
        }
    }
}
