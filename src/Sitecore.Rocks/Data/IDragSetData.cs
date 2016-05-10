// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface IDragSetData
    {
        void SetData([NotNull] DataObject dataObject);
    }
}
