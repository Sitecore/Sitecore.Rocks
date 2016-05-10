// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.AnalyzePackage
{
    public class AnalyzePackageDialogContext : IItemSelectionContext
    {
        private List<AnalyzePackageDialog.PackageItem> items;

        public AnalyzePackageDialogContext([NotNull] AnalyzePackageDialog analyzePackageDialog)
        {
            Assert.ArgumentNotNull(analyzePackageDialog, nameof(analyzePackageDialog));

            AnalyzePackageDialog = analyzePackageDialog;
        }

        [NotNull]
        public AnalyzePackageDialog AnalyzePackageDialog { get; }

        IEnumerable<IItem> IItemSelectionContext.Items
        {
            get
            {
                if (items != null)
                {
                    return items;
                }

                items = new List<AnalyzePackageDialog.PackageItem>();

                var dialog = AnalyzePackageDialog;

                if (dialog.ItemTab.IsSelected)
                {
                    foreach (var selectedItem in dialog.ItemList.SelectedItems)
                    {
                        var packageItem = selectedItem as AnalyzePackageDialog.PackageItem;

                        if (packageItem == null)
                        {
                            continue;
                        }

                        if (packageItem.Action != "Overwrite")
                        {
                            continue;
                        }

                        items.Add(packageItem);
                    }
                }

                return items;
            }
        }
    }
}
