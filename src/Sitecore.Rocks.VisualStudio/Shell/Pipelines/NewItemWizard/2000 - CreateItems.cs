// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    [Pipeline(typeof(NewItemWizardPipeline), 2000)]
    public class CreateItems : NewItemProcessorBase
    {
        protected override void Process(NewItemWizardPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var template = pipeline.Template.Replace(@"xmlns=""http://schemas.microsoft.com/developer/vstemplate/2005""", string.Empty);

            var doc = XDocument.Parse(template);

            var itemElements = doc.XPathSelectElements(@"/VSTemplate/SitecoreItems/Item").ToList();
            if (!itemElements.Any())
            {
                itemElements = doc.XPathSelectElements(@"/VSTemplate/WizardData/SitecoreItems/Item").ToList();
            }

            if (!itemElements.Any())
            {
                return;
            }

            var project = ProjectManager.GetProject(pipeline.Item) ?? GetProject(pipeline);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            foreach (var element in itemElements)
            {
                var databaseName = Expand(pipeline, element.GetAttributeValue("DatabaseName"));
                var path = Expand(pipeline, element.GetAttributeValue("Path"));
                var name = Expand(pipeline, element.GetAttributeValue("Name"));
                var templateName = Expand(pipeline, element.GetAttributeValue("TemplateName"));

                var fields = new StringBuilder();
                var first = true;

                foreach (var field in element.Elements())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        fields.Append('|');
                    }

                    fields.Append(Expand(pipeline, field.GetAttributeValue("Name")));
                    fields.Append('|');
                    fields.Append(Expand(pipeline, field.GetAttributeValue("Value")));
                }

                ExecuteCompleted completed = delegate(string response, ExecuteResult result)
                {
                    if (!DataService.HandleExecute(response, result))
                    {
                        return;
                    }

                    var parts = response.Split('|');
                    if (parts.Length != 2)
                    {
                        return;
                    }

                    Guid itemId;
                    if (!Guid.TryParse(parts[0], out itemId))
                    {
                        return;
                    }

                    Guid parentId;
                    if (!Guid.TryParse(parts[1], out parentId))
                    {
                        return;
                    }

                    var databaseUri = new DatabaseUri(site, new DatabaseName(databaseName));
                    var itemUri = new ItemUri(databaseUri, new ItemId(itemId));
                    var parentUri = new ItemUri(itemUri.DatabaseUri, new ItemId(parentId));

                    var fileName = project.GetProjectItemFileName(pipeline.Item);
                    var projectItem = ProjectFileItem.Load(project, fileName);
                    project.Add(projectItem);
                    projectItem.Items.Add(itemUri);

                    project.Save();

                    Notifications.RaiseItemAdded(this, new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), parentUri);
                };

                site.DataService.ExecuteAsync("Items.CreateItem", completed, databaseName, path, name, templateName, fields.ToString());
            }
        }

        [NotNull]
        private string Expand([NotNull] NewItemWizardPipeline pipeline, [NotNull] string value)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(value, nameof(value));

            return pipeline.Tokens.Aggregate(value, (current, token) => current.Replace(token.Key, token.Value));
        }
    }
}
