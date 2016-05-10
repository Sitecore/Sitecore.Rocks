// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioWindowsHost : WindowsHost
    {
        public override WindowFactory Factory
        {
            get { return AppHost.Container.Resolve<VisualStudioWindowFactory>(); }
        }

        public override void Activate<T>(string caption)
        {
            var factory = Factory as VisualStudioWindowFactory;
            if (factory == null)
            {
                return;
            }

            factory.ActivateToolWindow<T>(caption);
        }
    }
}
