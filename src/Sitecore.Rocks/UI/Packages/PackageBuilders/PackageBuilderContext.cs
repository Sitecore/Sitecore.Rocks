// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders
{
    public class PackageBuilderContext : ICommandContext, IItemSelectionContext, IFileSelectionContext
    {
        public PackageBuilderContext([NotNull] PackageBuilder packageBuilder, [CanBeNull] object sender)
        {
            Assert.ArgumentNotNull(packageBuilder, nameof(packageBuilder));

            PackageBuilder = packageBuilder;
            Sender = sender;
        }

        [NotNull]
        public PackageBuilder PackageBuilder { get; }

        [CanBeNull]
        public object Sender { get; }

        IEnumerable<IHasFileUri> IFileSelectionContext.Files
        {
            get
            {
                if (Sender != PackageBuilder.FileList)
                {
                    yield break;
                }

                foreach (var item in PackageBuilder.FileList.SelectedItems)
                {
                    var packageFile = item as PackageFile;
                    if (packageFile == null)
                    {
                        continue;
                    }

                    yield return packageFile;
                }
            }
        }

        IEnumerable<IItem> IItemSelectionContext.Items
        {
            get
            {
                if (Sender != PackageBuilder.ItemList)
                {
                    yield break;
                }

                foreach (var item in PackageBuilder.ItemList.SelectedItems)
                {
                    var packageItem = item as PackageItem;
                    if (packageItem == null)
                    {
                        continue;
                    }

                    yield return packageItem;
                }
            }
        }
    }
}
