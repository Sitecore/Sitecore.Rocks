// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Management.ManagementItems.SiteViewers.Dialogs;

namespace Sitecore.Rocks.UI.Management.ManagementItems.SiteViewers.Commands
{
    [Command]
    public class CreateNewSite : CommandBase
    {
        public CreateNewSite()
        {
            Text = Resources.CreateNewSite_CreateNewSite_Create_New_Site___;
            Group = "Add";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as SiteViewerContext;
            if (context == null)
            {
                return false;
            }

            var viewer = context.SiteViewer;

            if (viewer.SiteList.SelectedItem == null)
            {
                return false;
            }

            return true;
        }

        [Localizable(false)]
        public override void Execute(object parameter)
        {
            var context = parameter as SiteViewerContext;
            if (context == null)
            {
                return;
            }

            var viewer = context.SiteViewer;
            var siteDefinition = new SiteDefinition();

            var d = new EditSiteDefinitionDialog(viewer.Sites, siteDefinition);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var output = new StringBuilder("<configuration xmlns:patch=\"http://www.sitecore.net/xmlconfig/\">\n" + "  <sitecore>\n" + "    <sites>\n" + "      <site");

            // <site name="shell" virtualFolder="/sitecore/shell" physicalFolder="/sitecore/shell" rootPath="/sitecore/content" 
            // startItem="/home" language="en" database="core" domain="sitecore" loginPage="/sitecore/login" content="master" 
            // contentStartItem="/Home" enableWorkflow="true" enableAnalytics="false" 
            // xmlControlPage="/sitecore/shell/default.aspx" browserTitle="Sitecore" htmlCacheSize="2MB" 
            // registryCacheSize="3MB" viewStateCacheSize="200KB" xslCacheSize="5MB"/>
            var selectedItem = viewer.SiteList.SelectedItem as SiteDefinition;
            if (selectedItem != null)
            {
                output.Append(string.Format("\n        patch:before=\"site[@name='{0}']\"", selectedItem.Name));
            }

            Write(output, "inherits", siteDefinition.Inherits, string.Empty);

            Write(output, "mode", siteDefinition.Mode ? "on" : "off", "off");

            Write(output, "name", siteDefinition.Name, string.Empty);

            Write(output, "hostName", siteDefinition.HostName, string.Empty);
            Write(output, "port", siteDefinition.Port, string.Empty);

            Write(output, "virtualFolder", siteDefinition.VirtualFolder, string.Empty);
            Write(output, "physicalFolder", siteDefinition.PhysicalFolder, string.Empty);
            Write(output, "rootPath", siteDefinition.RootPath, string.Empty);
            Write(output, "startItem", siteDefinition.RootPath, string.Empty);
            Write(output, "language", siteDefinition.Language, string.Empty);
            Write(output, "database", siteDefinition.Database, string.Empty);
            Write(output, "domain", siteDefinition.Domain, string.Empty);
            Write(output, "loginPage", siteDefinition.LoginPage, string.Empty);
            Write(output, "content", siteDefinition.ContentDatabase, string.Empty);
            Write(output, "contentStartItem", siteDefinition.ContentStartItem, string.Empty);
            Write(output, "contentLanguage", siteDefinition.ContentLanguage, string.Empty);

            Write(output, "defaultDevice", siteDefinition.DefaultDevice, string.Empty);
            Write(output, "device", siteDefinition.Device, string.Empty);

            Write(output, "allowDebug", siteDefinition.AllowDebug, false);
            Write(output, "enableWorkflow", siteDefinition.EnableWorkflow, false);
            Write(output, "enableAnalytics", siteDefinition.EnableAnalytics, false);
            Write(output, "enableDebugger", siteDefinition.EnableDebugger, false);
            Write(output, "enablePreview", siteDefinition.EnablePreview, false);
            Write(output, "enableWebEdit", siteDefinition.EnableWebEdit, false);
            Write(output, "disableClientData", siteDefinition.DisableClientData, false);

            Write(output, "xmlControlPage", siteDefinition.XmlControlPage, string.Empty);
            Write(output, "disableXmlControls", siteDefinition.DisableXmlControls, false);

            Write(output, "browserTitle", siteDefinition.BrowserTitle, string.Empty);

            Write(output, "filteredItemsCacheSize", siteDefinition.FilteredItemsCacheSize, string.Empty);
            Write(output, "filterItems", siteDefinition.FilterItems, false);

            Write(output, "mediaCachePath", siteDefinition.MediaCachePath, string.Empty);

            Write(output, "cacheHtml", siteDefinition.CacheHtml, false);
            Write(output, "htmlCacheSize", siteDefinition.HtmlCacheSize, string.Empty);
            Write(output, "registryCacheSize", siteDefinition.RegistryCacheSize, string.Empty);
            Write(output, "viewStateCacheSize", siteDefinition.ViewStateCacheSize, string.Empty);
            Write(output, "xslCacheSize", siteDefinition.XslCacheSize, string.Empty);

            output.Append(" />\n    </sites>\n  </sitecore>\n</configuration>");

            string folder;
            if (!string.IsNullOrEmpty(viewer.Context.Site.WebRootPath))
            {
                folder = Path.Combine(viewer.Context.Site.WebRootPath, "App_Config\\Include");
            }
            else
            {
                folder = AppHost.Settings.Get("SiteViewer", "LastFolder", string.Empty) as string ?? string.Empty;
            }

            var fileName = Path.Combine(folder, string.Format("{0}.config", siteDefinition.Name));

            var dialog = new SaveFileDialog
            {
                Title = Resources.MediaManager_DownloadAttachment_Download,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = @"Include files|*.config"
            };

            if (!string.IsNullOrEmpty(fileName))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(fileName);
            }

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            AppHost.Settings.Set("SiteViewer", "LastFolder", Path.GetDirectoryName(dialog.FileName));

            AppHost.Files.WriteAllText(dialog.FileName, output.ToString(), Encoding.UTF8);

            AppHost.Files.OpenFile(dialog.FileName);

            viewer.Refresh();
        }

        private void Write([NotNull] StringBuilder output, [NotNull] string name, bool value, bool empty)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(name, nameof(name));

            Write(output, name, value ? "true" : "false", empty ? "true" : "false");
        }

        private void Write([NotNull] StringBuilder output, [NotNull] string name, [CanBeNull] string value, [NotNull] string empty)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(name, nameof(name));
            Debug.ArgumentNotNull(empty, nameof(empty));

            if (value == null || value == empty)
            {
                return;
            }

            output.Append("\n        ");
            output.Append(name);
            output.Append("=\"");
            output.Append(value);
            output.Append("\"");
        }
    }
}
