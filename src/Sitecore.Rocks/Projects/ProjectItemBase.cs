// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.IO;

namespace Sitecore.Rocks.Projects
{
    public abstract class ProjectItemBase
    {
        protected ProjectItemBase([NotNull] ProjectBase project)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            Project = project;
        }

        [NotNull]
        public ProjectBase Project { get; private set; }

        public abstract void Load([NotNull] XElement projectElement);

        public abstract void Save([NotNull] OutputWriter output);
    }
}
