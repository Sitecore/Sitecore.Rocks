// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.Shell.Pipelines.Initialization;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.LayoutDesigners
{
    [Pipeline(typeof(InitializationPipeline), 500)]
    public class FileSavedHandler : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.IsStartUp)
            {
                Notifications.FileSaved += SaveXmlLayout;
            }
        }

        private void SaveXmlLayout([NotNull] string fileName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            if (!fileName.EndsWith(".layout.xml", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var xmlLayout = AppHost.Files.ReadAllText(fileName);
            if (string.IsNullOrEmpty(xmlLayout))
            {
                return;
            }

            var rootElement = xmlLayout.ToXElement(LoadOptions.SetLineInfo);
            if (rootElement == null)
            {
                AppHost.Tasks.Add("Sitecore Layouts", new Task("Xml is not valid", TaskCategory.Error, fileName, 0, 0));
                AppHost.Tasks.Show("Sitecore Layouts");
                return;
            }

            var xmlns = rootElement.GetAttributeValue("xmlns");
            if (string.IsNullOrEmpty(xmlns))
            {
                AppHost.Tasks.Add("Sitecore Layouts", new Task("Missing xmlns attribute", TaskCategory.Error, fileName, 1, 1));
                AppHost.Tasks.Show("Sitecore Layouts");
                return;
            }

            if (!xmlns.StartsWith("http://www.sitecore.net/Sitecore-Speak-Intellisense/"))
            {
                AppHost.Tasks.Add("Sitecore Layouts", new Task("xmlns attribute must start with 'http://www.sitecore.net/Sitecore-Speak-Intellisense/'", TaskCategory.Error, fileName, 1, 1));
                AppHost.Tasks.Show("Sitecore Layouts");
                return;
            }

            var itemId = rootElement.GetAttributeValue("ItemId");
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            var parts = xmlns.Mid(52).Split('/');
            if (parts.Length != 2)
            {
                AppHost.Tasks.Add("Sitecore Layouts", new Task("The xmlns attribute is missing site and database information", TaskCategory.Error, fileName, 1, 1));
                AppHost.Tasks.Show("Sitecore Layouts");
                return;
            }

            var site = SiteManager.GetSite(parts[0]);
            if (site == null)
            {
                site = SiteManager.Sites.FirstOrDefault(s => s.Name.GetSafeCodeIdentifier() == parts[0]);

                if (site == null)
                {
                    AppHost.Tasks.Add("Sitecore Layouts", new Task(string.Format("Site \"{0}\" not found", parts[0]), TaskCategory.Error, fileName, 1, 1));
                    AppHost.Tasks.Show("Sitecore Layouts");
                    return;
                }
            }

            var itemUri = new ItemUri(new DatabaseUri(site, new DatabaseName(parts[1])), new ItemId(new Guid(itemId)));

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var tasks = new List<Task>();

                foreach (var error in root.Elements(@"e"))
                {
                    tasks.Add(new Task(error.Value, TaskCategory.Error, fileName, error.GetAttributeInt("line", 0), error.GetAttributeInt("column", 0)));
                }

                foreach (var warning in root.Elements(@"w"))
                {
                    tasks.Add(new Task(warning.Value, TaskCategory.Warning, fileName, warning.GetAttributeInt("line", 0), warning.GetAttributeInt("column", 0)));
                }

                foreach (var warning in root.Elements(@"m"))
                {
                    tasks.Add(new Task(warning.Value, TaskCategory.Message, fileName, warning.GetAttributeInt("line", 0), warning.GetAttributeInt("column", 0)));
                }

                AppHost.Tasks.Add("Sitecore Layouts", tasks);
                AppHost.Tasks.Show("Sitecore Layouts");
            };

            AppHost.Tasks.Clear("Sitecore Layouts", fileName);
            itemUri.Site.DataService.ExecuteAsync("XmlLayouts.SaveXmlLayout", completed, itemUri.DatabaseUri.DatabaseName.ToString(), itemUri.ItemId.ToString(), xmlLayout);
        }
    }
}
