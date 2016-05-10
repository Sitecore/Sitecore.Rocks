// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Templates;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates.Pipelines.ProcessCodeFirstTemplates
{
    public class ProcessCodeFirstTemplatesPipeline : Pipeline<ProcessCodeFirstTemplatesPipeline>
    {
        [NotNull]
        private readonly List<Template> templates = new List<Template>();

        [NotNull]
        public string DesignerFile { get; set; }

        [NotNull]
        public FileCodeModel FileCodeModel { get; private set; }

        [NotNull]
        public Site Site { get; private set; }

        [NotNull]
        public ICollection<Template> Templates
        {
            get { return templates; }
        }

        [NotNull]
        public ProcessCodeFirstTemplatesPipeline WithParameters([NotNull] Site site, [NotNull] FileCodeModel model)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(model, nameof(model));

            Site = site;
            FileCodeModel = model;
            DesignerFile = string.Empty;

            Start();

            return this;
        }
    }
}
