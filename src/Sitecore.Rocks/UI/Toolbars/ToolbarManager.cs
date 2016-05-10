// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility.Managers;

namespace Sitecore.Rocks.UI.Toolbars
{
    public class ToolbarManager : ComposableManagerBase<IToolbarElement>
    {
        [NotNull]
        public IEnumerable<ToolbarElement> GetElements([CanBeNull] object parameter)
        {
            if (parameter == null)
            {
                yield break;
            }

            var contextType = parameter.GetType();

            foreach (var element in Items)
            {
                var attributes = element.GetType().GetCustomAttributes(typeof(ToolbarElementAttribute), true);
                if (attributes.Length == 0)
                {
                    continue;
                }

                foreach (var attribute in attributes.OfType<ToolbarElementAttribute>())
                {
                    if (attribute.ContextType.IsInterface)
                    {
                        if (!attribute.ContextType.IsAssignableFrom(contextType))
                        {
                            continue;
                        }
                    }
                    else if (attribute.ContextType != contextType && !contextType.IsSubclassOf(attribute.ContextType))
                    {
                        continue;
                    }

                    var dynamicToolbarElement = element as IDynamicToolbarElement;
                    if (dynamicToolbarElement != null)
                    {
                        if (!dynamicToolbarElement.CanRender(parameter))
                        {
                            continue;
                        }
                    }

                    var sortOrder = attribute.SortOrder;
                    var chunk = attribute.Chunk;

                    var command = element as ICommand;
                    if (command != null)
                    {
                        if (sortOrder == 0)
                        {
                            sortOrder = command.SortingValue;
                        }

                        if (string.IsNullOrEmpty(chunk))
                        {
                            chunk = command.Group;
                        }
                    }

                    yield return new ToolbarElement(element, sortOrder, attribute.Strip, chunk, attribute.Icon, attribute.Text, attribute.ContextType, attribute.ElementType);
                }
            }
        }
    }
}
