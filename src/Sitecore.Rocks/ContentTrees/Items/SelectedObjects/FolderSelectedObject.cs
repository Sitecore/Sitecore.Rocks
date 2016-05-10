// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items.SelectedObjects
{
    public class FolderSelectedObject : BaseSelectedObject
    {
        public FolderSelectedObject([NotNull] string folder)
        {
            Assert.ArgumentNotNull(folder, nameof(folder));

            Folder = folder;
        }

        [NotNull, Description("The folder location."), DisplayName("Location"), Category("File")]
        public string Folder { get; private set; }
    }
}
