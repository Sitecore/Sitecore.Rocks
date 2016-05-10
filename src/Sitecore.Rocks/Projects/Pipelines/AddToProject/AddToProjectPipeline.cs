// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Projects.Pipelines.AddToProject
{
    public class AddToProjectPipeline : Pipeline<AddToProjectPipeline>
    {
        public bool DependOnParent { get; private set; }

        [NotNull]
        public string NewFileName { get; private set; }

        [NotNull]
        public string ParentFileName { get; private set; }

        [CanBeNull]
        public ProjectBase Project { get; set; }

        [NotNull]
        public AddToProjectPipeline WithParameters([NotNull] string parentFileName, [NotNull] string newFileName, bool dependOnParent)
        {
            Assert.ArgumentNotNull(parentFileName, nameof(parentFileName));
            Assert.ArgumentNotNull(newFileName, nameof(newFileName));

            ParentFileName = parentFileName;
            NewFileName = newFileName;
            DependOnParent = dependOnParent;

            Start();

            return this;
        }
    }
}
