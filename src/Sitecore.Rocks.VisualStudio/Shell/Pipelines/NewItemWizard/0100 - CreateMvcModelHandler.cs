// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    [Pipeline(typeof(NewItemWizardPipeline), 100)]
    public class CreateMvcModelHandler : NewItemAndFieldsProcessorBase
    {
        public CreateMvcModelHandler()
        {
            FileType = "MvcModel";
            TemplateName = "Model";
            TemplateId = new ItemId(new Guid("{FED6A14F-0D05-4E18-B160-17C0588A2005}"));
        }

        protected override void UpdateFields(NewItemWizardPipeline pipeline, ProjectItem projectItem, ItemUri itemUri)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var modelName = GetTypeName(pipeline, projectItem);
            ItemModifier.Edit(itemUri, new FieldId(new Guid("{EE9E23D2-181D-4172-A929-0E0B8D10313C}")), modelName);
        }
    }
}
