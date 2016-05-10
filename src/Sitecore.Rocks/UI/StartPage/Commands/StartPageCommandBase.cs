// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Commands
{
    public abstract class StartPageCommandBase : CommandBase, IStartPageCommand
    {
        public override bool CanExecute(object parameter)
        {
            return false;
        }

        public virtual bool CanExecute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return true;
        }

        public override void Execute(object parameter)
        {
            Execute();
        }

        public virtual void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Execute();
        }

        protected virtual void Execute()
        {
        }
    }
}
