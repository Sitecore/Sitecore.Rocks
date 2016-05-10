// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    [Pipeline(typeof(NewItemWizardPipeline), 102)]
    public class CreateMvcLayoutHandler : NewItemAndFieldsProcessorBase
    {
        public CreateMvcLayoutHandler()
        {
            FileType = "MvcLayout";
            TemplateName = "Layout";
            TemplateId = IdManager.GetItemId("/sitecore/templates/System/Layout/Layout");
        }

        protected override void UpdateFields(NewItemWizardPipeline pipeline, ProjectItem projectItem, ItemUri itemUri)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var fileName = GetFileName(pipeline, projectItem);

            var pathFieldId = IdManager.GetFieldId("/sitecore/templates/System/Layout/Layout/Data/Path");

            var fieldValues = new List<Tuple<FieldId, string>>
            {
                new Tuple<FieldId, string>(pathFieldId, fileName),
            };

            ItemModifier.Edit(itemUri, fieldValues);

            projectItem.Project.LinkItemAndFile(itemUri, fileName);
        }
    }
}
