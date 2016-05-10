// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.ContentEditors
{
    public class LoadItemsOptions
    {
        private static readonly LoadItemsOptions defaultOptions = new LoadItemsOptions(true);

        public LoadItemsOptions(bool addToJournal)
        {
            AddToJournal = addToJournal;
        }

        public bool AddToJournal { get; private set; }

        [NotNull]
        public static LoadItemsOptions Default
        {
            get { return defaultOptions; }
        }

        public bool ForceReload { get; set; }

        public bool NewTab { get; set; }
    }
}
