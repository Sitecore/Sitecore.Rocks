// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.ShellLoaded
{
    [Pipeline(typeof(ShellLoadedPipeline), 6000)]
    public class PreloadAssemblies : PipelineProcessor<ShellLoadedPipeline>
    {
        protected override void Process(ShellLoadedPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            LoadAssemblies(folder, "ActiproSoftware.*.dll");
            LoadAssemblies(folder, "TaskDialog.dll");
        }

        private void LoadAssemblies([NotNull] string folder, [NotNull] string searchPattern)
        {
            Debug.ArgumentNotNull(folder, nameof(folder));
            Debug.ArgumentNotNull(searchPattern, nameof(searchPattern));

            foreach (var fileName in AppHost.Files.GetFiles(folder, searchPattern))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(fileName);

#pragma warning disable 168
                    var type = assembly.GetTypes().FirstOrDefault();
#pragma warning restore 168
                }
                catch (Exception ex)
                {
                    AppHost.Output.Log(string.Format("Failed to load assembly {0}: {1}", fileName, ex.Message));
                }
            }
        }
    }
}
