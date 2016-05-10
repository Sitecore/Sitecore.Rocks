// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.RemoveServerComponents
{
    [Pipeline(typeof(RemoveServerComponentsPipeline), 2000)]
    public class RemoveFiles : PipelineProcessor<RemoveServerComponentsPipeline>
    {
        protected override void Process([NotNull] RemoveServerComponentsPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            var output = new StringWriter();

            RemoveAssemblies(output, pipeline.WebRootPath);
            RunRemovers(output, pipeline.WebRootPath);

            var text = output.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                AppHost.MessageBox(string.Format("There were errors while removing server components:\n") + text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [NotNull]
        private void RemoveAssemblies([NotNull] TextWriter output, [NotNull] string webRootPath)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var binFolder = Path.Combine(webRootPath, @"bin");
            if (!Directory.Exists(binFolder))
            {
                return;
            }

            foreach (var fileName in AppHost.Files.GetFiles(binFolder, "*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
                if (!AppHost.Plugins.IsServerComponent(name))
                {
                    continue;
                }

                try
                {
                    File.Delete(fileName);
                }
                catch (Exception ex)
                {
                    output.WriteLine("/bin/" + Path.GetFileName(fileName) + " : " + ex.Message);
                }
            }
        }

        private void RunRemovers([NotNull] TextWriter output, [NotNull] string webRootPath)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var options = new RemoveServerComponentOptions(output, webRootPath);

            foreach (var remover in UpdateServerComponentsManager.Removers)
            {
                if (!remover.CanRemove(options))
                {
                    continue;
                }

                remover.Remove(options);
            }
        }
    }
}
