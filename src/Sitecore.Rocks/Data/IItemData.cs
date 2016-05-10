// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface IItemData
    {
        [CanBeNull]
        string GetData([Localizable(false), NotNull] string key);
    }
}
