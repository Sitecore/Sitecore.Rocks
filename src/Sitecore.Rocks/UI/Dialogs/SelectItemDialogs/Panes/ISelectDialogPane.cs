// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes
{
    public interface ISelectDialogPane
    {
        [NotNull]
        string Header { get; }

        void Close();

        void Initialize([NotNull] SelectItemDialog selectItemDialog);

        void SetActive();
    }
}
