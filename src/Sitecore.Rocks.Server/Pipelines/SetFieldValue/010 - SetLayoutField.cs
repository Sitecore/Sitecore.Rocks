// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.SetFieldValue
{
    [Pipeline(typeof(SetFieldValuePipeline), 1000)]
    public class SetLayoutField : PipelineProcessor<SetFieldValuePipeline>
    {
        protected override void Process(SetFieldValuePipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (string.Compare(pipeline.Field.Type, "Layout", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(pipeline.Value))
            {
                return;
            }

            pipeline.Value = PatchUniqueId(pipeline.Value);

            try
            {
                SetField(pipeline);
            }
            catch (MissingMethodException)
            {
                return;
            }

            pipeline.Abort();
        }

        private string PatchUniqueId(string value)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(value);
            }
            catch
            {
                return value;
            }

            var root = doc.Root;
            if (root == null)
            {
                return value;
            }

            foreach (var deviceElement in root.Elements())
            {
                var renderingElements = deviceElement.Elements().ToList();
                foreach (var renderingElement in renderingElements)
                {
                    // set the unique id, if not set
                    var uid = renderingElement.GetAttributeValue("uid");
                    if (string.IsNullOrEmpty(uid))
                    {
                        renderingElement.SetAttributeValue("uid", Guid.NewGuid().ToString("B").ToUpperInvariant());
                        continue;
                    }

                    // find duplicate uniqueids and patch them
                    var duplicates = renderingElements.Where(r => r.GetAttributeValue("uid") == uid && r != renderingElement);
                    foreach (var duplicate in duplicates)
                    {
                        duplicate.SetAttributeValue("uid", Guid.NewGuid().ToString("B").ToUpperInvariant());
                    }
                }
            }

            return doc.ToString();
        }

        private void SetField([NotNull] SetFieldValuePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            LayoutField.SetFieldValue(pipeline.Field, pipeline.Value);
        }
    }
}
