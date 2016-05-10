// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public delegate void ValueModifiedEventHandler();

    public interface IFieldControl
    {
        [NotNull]
        Control GetControl();

        [CanBeNull]
        Control GetFocusableControl();

        [NotNull]
        string GetValue();

        bool IsSupported([NotNull] Field sourceField);

        void SetField([NotNull] Field sourceField);

        void SetValue([NotNull] string value);

        event ValueModifiedEventHandler ValueModified;
    }
}
