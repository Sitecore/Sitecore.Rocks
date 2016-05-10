// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.GuidExtensions;

namespace Sitecore.Rocks.CodeGeneration.CodeFirstTemplates
{
    public class FileCodeModel
    {
        public FileCodeModel([NotNull] FileCodeModel2 fileCodeModel)
        {
            Assert.ArgumentNotNull(fileCodeModel, nameof(fileCodeModel));

            Model = fileCodeModel;
        }

        [NotNull]
        public virtual IEnumerable<CodeElement2> CodeElements
        {
            get
            {
                var rootNode = Model;

                foreach (var child in rootNode.CodeElements)
                {
                    var codeElement = child as CodeElement2;
                    if (codeElement == null)
                    {
                        continue;
                    }

                    yield return codeElement;

                    foreach (var c in GetDescendents(codeElement))
                    {
                        yield return c;
                    }
                }
            }
        }

        [NotNull]
        protected FileCodeModel2 Model { get; set; }

        public Guid GetHash([NotNull] CodeElements attributes, [NotNull] string defaultName)
        {
            Assert.ArgumentNotNull(attributes, nameof(attributes));
            Assert.ArgumentNotNull(defaultName, nameof(defaultName));

            foreach (var attribute in attributes.OfType<CodeAttribute2>())
            {
                if (attribute.Name == "SitecoreClass" || attribute.Name == "SitecoreClassAttribute")
                {
                    var templateId = attribute.Arguments.OfType<CodeAttributeArgument>().FirstOrDefault(a => a.Name == "TemplateId");
                    if (templateId != null)
                    {
                        Guid guid;
                        if (Guid.TryParse(templateId.Value, out guid))
                        {
                            return guid;
                        }
                    }
                }

                if (attribute.Name == "Guid" || attribute.Name == "GuidAttribute")
                {
                    var templateId = attribute.Arguments.OfType<CodeAttributeArgument>().FirstOrDefault();
                    if (templateId != null)
                    {
                        Guid guid;
                        if (Guid.TryParse(templateId.Value, out guid))
                        {
                            return guid;
                        }
                    }
                }
            }

            return GuidExtensions.Hash(defaultName);
        }

        [NotNull]
        private IEnumerable<CodeElement2> GetDescendents([NotNull] CodeElement2 node)
        {
            Debug.ArgumentNotNull(node, nameof(node));

            foreach (var child in node.Children)
            {
                var codeElement = child as CodeElement2;
                if (codeElement == null)
                {
                    continue;
                }

                yield return codeElement;

                foreach (var descendent in GetDescendents(codeElement))
                {
                    yield return descendent;
                }
            }
        }
    }
}
