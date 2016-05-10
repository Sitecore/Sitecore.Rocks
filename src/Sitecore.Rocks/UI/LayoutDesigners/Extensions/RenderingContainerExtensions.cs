// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Extensions
{
    public static class RenderingContainerExtensions
    {
        public static void GetRenderingContainerDataBindingValues([NotNull] this IRenderingContainer container, [NotNull] RenderingItem renderingItem, [NotNull] DynamicProperty dynamicProperty, [NotNull] List<string> values)
        {
            Assert.ArgumentNotNull(container, nameof(container));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));
            Assert.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));
            Assert.ArgumentNotNull(values, nameof(values));

            var primary = new List<string>();
            var secondary = new List<string>();

            foreach (var rendering in container.Renderings)
            {
                if (rendering == renderingItem)
                {
                    continue;
                }

                var idProperty = rendering.DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, @"id", StringComparison.InvariantCultureIgnoreCase) == 0);
                if (idProperty == null)
                {
                    continue;
                }

                var name = idProperty.Value as string;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                foreach (var property in rendering.DynamicProperties)
                {
                    if (property == idProperty)
                    {
                        continue;
                    }

                    if (!property.CanRead())
                    {
                        continue;
                    }

                    if (string.Compare(dynamicProperty.TypeName, "Checkbox", StringComparison.InvariantCultureIgnoreCase) == 0 && string.Compare(property.TypeName, "Checkbox", StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        continue;
                    }

                    var subtype0 = dynamicProperty.Attributes["subtype"] as string ?? string.Empty;
                    var subtype1 = property.Attributes["subtype"] as string ?? string.Empty;
                    if (!string.IsNullOrEmpty(subtype0) && string.Compare(subtype0, subtype1, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        continue;
                    }

                    var text = string.Format("{{Binding {0}.{1}}}", name, property.Name);

                    if (string.Compare(property.Name, dynamicProperty.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        primary.Add(text);
                    }
                    else
                    {
                        secondary.Add(text);
                    }
                }
            }

            primary.Sort();
            secondary.Sort();

            values.AddRange(primary);
            values.AddRange(secondary);
        }
    }
}
