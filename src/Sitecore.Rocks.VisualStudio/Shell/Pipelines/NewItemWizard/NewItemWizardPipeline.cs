// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    public class NewItemWizardPipeline : Pipeline<NewItemWizardPipeline>
    {
        [CanBeNull]
        public DatabaseName DatabaseName { get; set; }

        [CanBeNull]
        public string FileType { get; set; }

        public bool HasAskedForConnection { get; set; }

        [NotNull]
        public EnvDTE.ProjectItem Item { get; set; }

        [CanBeNull]
        public ProjectFileItem ProjectItem { get; set; }

        [NotNull]
        public string Template { get; set; }

        [NotNull]
        public Dictionary<string, string> Tokens { get; set; }

        [NotNull]
        public NewItemWizardPipeline WithParameters([NotNull] EnvDTE.ProjectItem item, [NotNull] Dictionary<string, string> tokens, [NotNull] string template)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(tokens, nameof(tokens));
            Assert.ArgumentNotNull(template, nameof(template));

            Item = item;
            Tokens = tokens;
            Template = template;

            Start();

            return this;
        }
    }
}
