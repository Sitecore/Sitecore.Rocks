// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Options
{
    [Localizable(false)]
    public class VisualStudioOptions : DialogPage, IOptions
    {
        private string _fileTypes;

        private string _macrosFileName;

        private string _pluginRespositoryUrl;

        private string _sharedFolder;

        public VisualStudioOptions()
        {
            ShowStartPageOnStartup = true;

            try
            {
                var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
                if (key != null)
                {
                    var value = key.GetValue("CurrentVersion") as string;
                    if (value == "6.2" || value == "6.3")
                    {
                        DisableIisIntegration = true;
                    }
                }
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }
        }

        [Category("Appearance"), DisplayName(@"Color Theme"), Description("Sets the color theme.")]
        public AppTheme AppTheme { get; set; }

        [Category("System"), DisplayName(@"Disable IIS Integration"), Description("Disables Local IIS Sites and drop down in New Connection dialog.")]
        public bool DisableIisIntegration { get; set; }

        [Category("Extensions"), DisplayName(@"Enable Sitecore Rocks Extensions Menu Items"), Description("Enable Sitecore Rocks Extensions menu items in context menus.")]
        public bool EnableSitecoreRocksExtensions { get; set; }

        [Category("Files"), DisplayName(@"File Types"), Description("File types.")]
        public string FileTypes
        {
            get
            {
                if (!string.IsNullOrEmpty(_fileTypes))
                {
                    return _fileTypes;
                }

                return ".cs|csharp|.htm|html|.html|html|.xhtml|html|.xsl|xslt|.xslt|xslt|.xml|xml|.config|xml|.js|javascript|.css|css";
            }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _fileTypes = value;
            }
        }

        [Category("Help"), DisplayName(@"Hide the Item Editor help"), Description("Hide the Item Editor help.")]
        public bool HideContentEditorHelp { get; set; }

        [Category("Help"), DisplayName(@"Hide the Sitecore Explorer help"), Description("Hide the Sitecore Explorer help.")]
        public bool HideContentTreeHelp { get; set; }

        [Category("Help"), DisplayName(@"Hide the Drag to Upload help text"), Description("Hide the Drag to Upload help text.")]
        public bool HideDragToUploadHelp { get; set; }

        [Category("Help"), DisplayName(@"Hide GUID search warning"), Description("Hide warning that older versions of Sitecore does not support searching for GUIDs.")]
        public bool HideGuidSearch { get; set; }

        [Category("Help"), DisplayName(@"Hide Image Field help"), Description("Hide help regarding how to select images in the Image field.")]
        public bool HideImageFieldHelp { get; set; }

        [Category("Publishing"), DisplayName(@"Hide dialog when publishing"), Description("Hide dialog when publishing.")]
        public bool HidePublishingDialog { get; set; }

        [Category("Item Editor"), DisplayName(@"Show Quick Information section"), Description("Show Quick Information section.")]
        public bool HideQuickInfo { get; set; }

        [Category("Help"), DisplayName(@"Hide the Create Script Macro help"), Description("Hide the Create Script Macro help.")]
        public bool HideScriptMacroHelp { get; set; }

        [Category("Server Components"), DisplayName(@"Hide Update warning dialog"), Description("Hide Update warning dialog.")]
        public bool HideUpdateDialog { get; set; }

        [Category("Debug"), DisplayName(@"Is Logging Enabled"), Description("If enabled, all Sitecore Rocks commands and server requests are logged in the Output window.")]
        public bool IsLogEnabled { get; set; }

        [Category("Macros"), DisplayName(@"Macro File Location"), Description("The file name of the macros.")]
        public string MacrosFileName
        {
            get
            {
                if (!string.IsNullOrEmpty(_macrosFileName))
                {
                    return _macrosFileName;
                }

                return Path.Combine(AppHost.User.UserFolder, "Macros\\Macros.xml");
            }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _macrosFileName = value;
            }
        }

        [NotNull, Category("Plugins"), DisplayName(@"Respository URL"), Description("The URL of the plugin repository.")]
        public string PluginRespositoryUrl
        {
            get { return _pluginRespositoryUrl ?? @"vsplugins.sitecore.net"; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _pluginRespositoryUrl = value;
            }
        }

        [Category("Sitecore Explorer"), DisplayName(@"Reuse Item Editor window, if saved."), Description("Open items in the same Item Editor window, if there are no changes.")]
        public bool ReuseWindow { get; set; }

        [NotNull, Category("Sharing"), DisplayName(@"Shared Folder"), Description("Folder for sharing information between a team.")]
        public string SharedFolder
        {
            get { return _sharedFolder ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _sharedFolder = value;
            }
        }

        [Category("Item Editor"), DisplayName(@"Show Field Display Titles"), Description("Show field display titles instead of field names.")]
        public bool ShowFieldDisplayTitles { get; set; }

        [Category("Item Editor"), DisplayName(@"Show Field Information"), Description("Show field information like Shared, Unversioned or Standard Value.")]
        public bool ShowFieldInformation { get; set; }

        [Category("Plugin Developers"), DisplayName(@"Show Groups and Sorting Values"), Description("Show Groups and Sorting Values")]
        public bool ShowGroupAndSortingValue { get; set; }

        [Category("Publishing"), DisplayName(@"Show Job Viewer"), Description("Show Job Viewer when the dialog closes.")]
        public bool ShowJobViewer { get; set; }

        [Category("Item Editor"), DisplayName(@"Show raw values"), Description("Show raw values.")]
        public bool ShowRawValues { get; set; }

        [Category("Item Editor"), DisplayName(@"Show standard fields"), Description("Show standard fields.")]
        public bool ShowStandardFields { get; set; }

        [Category("Start Up"), DisplayName(@"Show the Start Page"), Description("Show the Start Page on start up.")]
        public bool ShowStartPageOnStartup { get; set; }

        [Category("Item Editor"), DisplayName(@"Skin"), Description("Item Editor Skin.")]
        public string Skin { get; set; }

        public void Load()
        {
        }

        public void Save()
        {
            SaveSettingsToStorage();
        }
    }
}
