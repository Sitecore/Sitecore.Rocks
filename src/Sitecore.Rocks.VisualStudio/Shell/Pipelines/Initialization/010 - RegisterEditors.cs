// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.CodeGeneration;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.UI.EditorWindowHosts;
using Sitecore.Rocks.UI.Packages.PackageBuilders;
using Sitecore.Rocks.UI.TemplateDesigner;
using Sitecore.Rocks.UI.TemplateFieldSorter;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 1000)]
    public class RegisterEditors : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsStartUp)
            {
                return;
            }

            SitecorePackage.Instance.RegisterEditor(new ContentEditorFactory());
            SitecorePackage.Instance.RegisterEditor(new TemplateDesignerFactory());
            SitecorePackage.Instance.RegisterEditor(new TemplateFieldSorterFactory());
            SitecorePackage.Instance.RegisterEditor(new EditorWindowFactory());
            SitecorePackage.Instance.RegisterEditor(new CodeGenerationFactory());
            SitecorePackage.Instance.RegisterEditor(new PackageBuilderFactory());
        }
    }
}
