// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.UI.Macros;

namespace Sitecore.Rocks.ContentEditors.Pipelines.SaveItem
{
    [Pipeline(typeof(SaveItemPipeline), 5500)]
    public class PostMacro : PipelineProcessor<SaveItemPipeline>
    {
        protected override void Process(SaveItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.PostMacro)
            {
                return;
            }

            var macroName = AppHost.Settings.GetString(@"Content Editor", "Post Save Macro", string.Empty);
            if (string.IsNullOrEmpty(macroName))
            {
                return;
            }

            var macro = MacroManager.Macros.FirstOrDefault(m => m.Text == macroName);
            if (macro != null)
            {
                macro.Run(pipeline.Editor.GetContext());
            }
        }
    }
}
