// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 500)]
    public class SetTheme : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsStartUp)
            {
                return;
            }

			ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				AppHost.Shell.VisualStudioTheme = GetVisualStudioTheme();
			});
        }

        private AppTheme GetVisualStudioTheme()
        {
            var appTheme = AppHost.Options.AppTheme;

            if (appTheme == AppTheme.Automatic)
            {
                var isDark = VSColorPaint() == 3157293;

                var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\VisualStudio\\" + SitecorePackage.Instance.Dte.Version + "\\General");
                if (key != null)
                {
                    var themeId = key.GetValue("CurrentTheme", string.Empty) as string ?? string.Empty;

                    if (themeId.Contains("1ded0138-47ce-435e-84ef-9ec1f439b749"))
                    {
                        isDark = true;
                    }
                }

                if (isDark)
                {
                    appTheme = AppTheme.Dark;
                }
            }

            return appTheme;
        }

        private uint VSColorPaint()
        {
			// ReSharper disable once SuspiciousTypeConversion.Global
			var uiShell2 = SitecorePackage.Instance.GetService<SVsUIShell>() as IVsUIShell2;
			if (uiShell2 == null)
            {
                return 0;
            }

            uint win32Color;
            uiShell2.GetVSSysColorEx(-53, out win32Color); // VSCOLOR_ENVIRONMENT_BACKGROUND

            return win32Color;
        }
    }
}
