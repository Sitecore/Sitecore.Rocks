// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.KeyEventArgsExtentions
{
    public static class KeyEventArgsExtentions
    {
        [NotNull]
        public static string GetGestureString([NotNull] this KeyEventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));

            var result = string.Empty;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                result += Resources.KeyEventArgsExtentions_GetGestureString_Ctrl_;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                result += Resources.KeyEventArgsExtentions_GetGestureString_Shift_;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                result += Resources.KeyEventArgsExtentions_GetGestureString_Alt_;
            }

            result += e.Key.ToString();

            return result;
        }
    }
}
