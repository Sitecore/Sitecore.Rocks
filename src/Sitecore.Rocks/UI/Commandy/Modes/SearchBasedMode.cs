// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    public abstract class SearchBasedMode : ModeBase
    {
        protected SearchBasedMode([NotNull] Commandy commandy) : base(commandy)
        {
            Debug.ArgumentNotNull(commandy, nameof(commandy));

            IsReady = true;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ISiteSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.Site != Site.Empty;
        }

        public override void Execute(Hit hit, object parameter)
        {
            Assert.ArgumentNotNull(hit, nameof(hit));
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var itemHeader = hit.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            Execute(itemHeader);
        }

        protected abstract void Execute([NotNull] ItemHeader itemHeader);
    }
}
