// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectDatabaseDialogs;

namespace Sitecore.Rocks.UI.StartPage
{
    public static class StartPageCommandExtensions
    {
        [NotNull]
        public static DatabaseUri GetDatabaseUri([NotNull] this IStartPageCommand command, [NotNull] StartPageContext context)
        {
            Assert.ArgumentNotNull(command, nameof(command));
            Assert.ArgumentNotNull(context, nameof(context));

            var result = context.StartPageViewer.DatabaseUri;

            if (result != DatabaseUri.Empty)
            {
                return result;
            }

            if (result == DatabaseUri.Empty)
            {
                var dialog = new SelectDatabaseDialog
                {
                    SelectedDatabaseUri = AppHost.Settings.ActiveDatabaseUri
                };

                if (dialog.ShowDialog())
                {
                    result = dialog.SelectedDatabaseUri;
                    if (result != DatabaseUri.Empty)
                    {
                        AppHost.Settings.ActiveDatabaseUri = result;

                        context.StartPageViewer.DatabaseUri = result;
                        context.StartPageViewer.UpdateLinks();

                        return result;
                    }
                }
            }

            return DatabaseUri.Empty;
        }

        public static bool HasDatabaseUri([NotNull] this IStartPageCommand command, [NotNull] StartPageContext context)
        {
            Assert.ArgumentNotNull(command, nameof(command));
            Assert.ArgumentNotNull(context, nameof(context));

            var result = context.StartPageViewer.DatabaseUri;

            return result != DatabaseUri.Empty;
        }
    }
}
