// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Projects.ProjectItems;

namespace Sitecore.Rocks.Projects
{
    public class ProjectLoader
    {
        [CanBeNull]
        public delegate ProjectItemBase ProjectElementHandlerDelegate(ProjectBase project, XElement element);

        private static readonly Dictionary<string, ProjectElementHandlerDelegate> ProjectElementHandlers = new Dictionary<string, ProjectElementHandlerDelegate>();

        static ProjectLoader()
        {
            RegisterProjectElementHandler("Site", LoadProjectSiteElement);
        }

        public static void ClearProjectItemHandlers()
        {
            ProjectElementHandlers.Clear();
        }

        public void Load([NotNull] ProjectBase project, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var doc = XDocument.Load(fileName);

            lock (project.SyncRoot)
            {
                project.Clear();
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            Load(project, root);
        }

        public static void RegisterProjectElementHandler([NotNull] string elementName, [NotNull] ProjectElementHandlerDelegate handler)
        {
            Assert.ArgumentNotNull(elementName, nameof(elementName));
            Assert.ArgumentNotNull(handler, nameof(handler));

            ProjectElementHandlers[elementName] = handler;
        }

        public static void UnregisterProjectItemHandler([NotNull] string elementName)
        {
            Assert.ArgumentNotNull(elementName, nameof(elementName));

            ProjectElementHandlers.Remove(elementName);
        }

        [CanBeNull]
        private static ProjectElementHandlerDelegate GetProjectElementHandler([NotNull] string localName)
        {
            Debug.ArgumentNotNull(localName, nameof(localName));

            ProjectElementHandlerDelegate result;
            return ProjectElementHandlers.TryGetValue(localName, out result) ? result : null;
        }

        private void Load([NotNull] ProjectBase project, [NotNull] XElement parentElement)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(parentElement, nameof(parentElement));

            foreach (var element in parentElement.Elements())
            {
                if (element.Name.LocalName == "PropertyGroup" || element.Name.LocalName == "ItemGroup")
                {
                    Load(project, element);
                    continue;
                }

                var handler = GetProjectElementHandler(element.Name.LocalName);
                if (handler != null)
                {
                    var projectItem = handler(project, element);

                    if (projectItem != null)
                    {
                        project.Add(projectItem);
                    }
                }
            }
        }

        [CanBeNull]
        private static ProjectSiteItem LoadProjectSiteElement([NotNull] ProjectBase project, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(element, nameof(element));

            project.ProjectSiteItem.Load(element);
            return null;
        }
    }
}
