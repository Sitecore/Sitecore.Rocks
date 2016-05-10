// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    [Pipeline(typeof(NewItemWizardPipeline), 101)]
    public class CreateMvcControllerHandler : NewItemAndFieldsProcessorBase
    {
        public CreateMvcControllerHandler()
        {
            FileType = "MvcController";
            TemplateName = "Controller";
            TemplateId = new ItemId(new Guid("{473F22C2-3C94-4D1D-B9AE-3D1FC3D3D3F2}"));
        }

        protected override void UpdateFields(NewItemWizardPipeline pipeline, ProjectItem projectItem, ItemUri itemUri)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var modelName = GetTypeName(pipeline, projectItem);

            var fieldValues = new List<Tuple<FieldId, string>>();
            fieldValues.Add(new Tuple<FieldId, string>(new FieldId(new Guid("{1A0AE537-291C-4CC8-B53F-7CA307A0FEF5}")), modelName));
            fieldValues.Add(new Tuple<FieldId, string>(new FieldId(new Guid("{298F7E71-8AEB-488B-BC93-CE6F2B09E130}")), "Index"));

            ItemModifier.Edit(itemUri, fieldValues);
        }
    }
}
