// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.SiteViewers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class SiteDefinition
    {
        [Description("Must be true to be able to debug the site."), Category("Debugging"), DisplayName("Allow Debug")]
        public bool AllowDebug { get; set; }

        [Description("The title of the browser window when browsing the site."), Category("Information"), DisplayName("Browser Title")]
        public string BrowserTitle { get; set; }

        [Description("If true, HTML caching will be enabled. If false, no HTML will be cached for any rendering. Default value: false."), Category("Caching"), DisplayName("Cache Html")]
        public bool CacheHtml { get; set; }

        [Description("Database containing items to be edited."), Category("Content"), DisplayName("Content Database")]
        public string ContentDatabase { get; set; }

        [Description("The default editing language in the client."), Category("Content"), DisplayName("Content Language")]
        public string ContentLanguage { get; set; }

        [Description("The path to the start item when using the client."), Category("Content"), DisplayName("Content Start Item")]
        public string ContentStartItem { get; set; }

        [Description("Database containing items to be used for rendering the site."), Category("Content")]
        public string Database { get; set; }

        [Description("The device to use if no specific device matches the request. This setting takes precedence over the default device as it is set by Default checkbox for Device item."), Category("Devices"), DisplayName("Default Device")]
        public string DefaultDevice { get; set; }

        [Description("The name of the device to use for the site. If not specified, the device resolver will find a 'best match device'."), Category("Devices")]
        public string Device { get; set; }

        [Description("If set to true, the use of ClientDataStore will be disabled for the site."), Category("Configuration"), DisplayName("Disable Client Data")]
        public bool DisableClientData { get; set; }

        [Description("If set to true, loading Xml Controls as pages will be disabled."), Category("Configuration"), DisplayName("Disable XML Controls")]
        public bool DisableXmlControls { get; set; }

        [Description("The security domain of the site."), Category("Security")]
        public string Domain { get; set; }

        [Description("Indicates if analytics is enabled on the site. Typically this is only the website. Default value: true."), Category("Configuration"), DisplayName("Enable Analytics")]
        public bool EnableAnalytics { get; set; }

        [Description("Indicates if the debugger is enabled on the site. Typically this is only the website."), Category("Debugging"), DisplayName("Enable Debugger")]
        public bool EnableDebugger { get; set; }

        [Description("Indicates if preview is enabled on the site. Typically this is only the website."), Category("Configuration"), DisplayName("Enable Preview")]
        public bool EnablePreview { get; set; }

        [Description("Indicates if WebEdit is enabled on the site. Typically this is only the website."), Category("Configuration"), DisplayName("Enable WebEdit")]
        public bool EnableWebEdit { get; set; }

        [Description("Must be true to enable workflows for the site."), Category("Configuration"), DisplayName("Enable Workflow")]
        public bool EnableWorkflow { get; set; }

        [Description("The size of the cache used to store filtered items. Specify the value in bytes or append the value with KB, MB or GB"), Category("Filtering"), DisplayName("Filtered Items Cache Size")]
        public string FilteredItemsCacheSize { get; set; }

        [Description("If true, the site will always show the current version of an item (without publishing)"), Category("Filtering"), DisplayName("Filter Items")]
        public bool FilterItems { get; set; }

        [Description("The host name of the incoming url. May include wildcards (ex. www.site.net, *.site.net, *.net, pda.*, print.*.net). It's possible to set more than one mask by using '|' symbol as a separator (ex. pda.*|print.*.net)"), Category("Web"), DisplayName("Host Name")]
        public string HostName { get; set; }

        [Description("The size of the html cache. Specify the value in bytes or append the value with KB, MB or GB"), Category("Caching"), DisplayName("Html Cache Size")]
        public string HtmlCacheSize { get; set; }

        [Browsable(false)]
        public int Index { get; set; }

        [Description("Indicates that the attributes should be inherited from another site. To enable inheritance, you must specify the name of the source site. Attributes that are explicitly specified overwrite the attributes that are inherited from the source site."), Category("Inheritance"), DisplayName("Inherits")]
        public string Inherits { get; set; }

        [Description("Default language for the site."), Category("Content")]
        public string Language { get; set; }

        [Description("The path to the login page to use. Must point to a physical file or a page in a site that does NOT require login."), Category("Security"), DisplayName("Login Page")]
        public string LoginPage { get; set; }

        [Description("The database containing the data to be shown in preview and web edit modes."), Category("Content"), DisplayName("Master Database")]
        public string MasterDatabase { get; set; }

        [Description("The size of the html cache. Specify the value in bytes or append the value with KB, MB or GB"), Category("Caching"), DisplayName("Media Cache Path")]
        public string MediaCachePath { get; set; }

        [Description("[on|off]. If set to 'off', the site will be disabled. Default value: 'on'."), Category("Configuration")]
        public bool Mode { get; set; }

        [Description(" Name of the site."), Category("Configuration")]
        public string Name { get; set; }

        [Description("The physical location of files for the site. If the site is based on physical files, this is the path to the folder holding the files. For non-physical sites, this is the place where Sitecore looks for a default.aspx file (to start the pipelines)."), Category("Configuration"), DisplayName("Physical Folder")]
        public string PhysicalFolder { get; set; }

        [Description("The port number of the incoming url. Default value: 80"), Category("Web")]
        public string Port { get; set; }

        [Description("The size of the registry cache. Specify the value in bytes or append the value with KB, MB or GB"), Category("Caching"), DisplayName("Registry Cache Size")]
        public string RegistryCacheSize { get; set; }

        [Description("The path to the root item of the site. The item path specified in the URL will be appended to this value to find the item."), Category("Web"), DisplayName("Root Path")]
        public string RootPath { get; set; }

        [Description("The path to the start item of the site. This is used as the default item path if no path is specified in the URL. It will be combined with rootPath to find the default item of the site."), Category("Content"), DisplayName("Start Item")]
        public string StartItem { get; set; }

        [Description("The size of the view state cache. Specify the value in bytes or append the value with KB, MB or GB"), Category("Caching"), DisplayName("View State Cache Size")]
        public string ViewStateCacheSize { get; set; }

        [Description("The prefix to match for incoming URL's. This value will be removed from the URL and the remainder will be treated as the item path."), Category("Web"), DisplayName("Virtual Folder")]
        public string VirtualFolder { get; set; }

        [Description("The page that renders Xml Controls."), Category("Web"), DisplayName("XML Control Page")]
        public string XmlControlPage { get; set; }

        [Description("The size of the Xsl cache. Specify the value in bytes or append the value with KB, MB or GB"), Category("Caching"), DisplayName("XSL Cache Size")]
        public string XslCacheSize { get; set; }

        public void Parse([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Name = element.GetAttributeValue("name");
            HostName = element.GetAttributeValue("hostName");
            Database = element.GetAttributeValue("database");
            Domain = element.GetAttributeValue("domain");
            Mode = element.GetAttributeValue("mode") == "on";
            Port = element.GetAttributeValue("port");
            VirtualFolder = element.GetAttributeValue("virtualFolder");
            PhysicalFolder = element.GetAttributeValue("physicalFolder");
            RootPath = element.GetAttributeValue("rootPath");
            StartItem = element.GetAttributeValue("startItem");
            Language = element.GetAttributeValue("language");
            Database = element.GetAttributeValue("database");
            ContentDatabase = element.GetAttributeValue("content");
            ContentLanguage = element.GetAttributeValue("contentLanguage");
            ContentStartItem = element.GetAttributeValue("contentStartItem");
            MasterDatabase = element.GetAttributeValue("masterDatabase");
            Device = element.GetAttributeValue("device");
            FilterItems = element.GetAttributeValue("filterItems") == "true";
            FilteredItemsCacheSize = element.GetAttributeValue("filteredItemsCacheSize");
            CacheHtml = element.GetAttributeValue("cacheHtml") == "true";
            HtmlCacheSize = element.GetAttributeValue("htmlCacheSize");
            MediaCachePath = element.GetAttributeValue("mediaCachePath");
            LoginPage = element.GetAttributeValue("loginPage");
            EnableDebugger = element.GetAttributeValue("enableDebugger") == "true";
            EnablePreview = element.GetAttributeValue("enablePreview") == "true";
            EnableWebEdit = element.GetAttributeValue("enableWebEdit") == "true";
            EnableWorkflow = element.GetAttributeValue("enableWorkflow") == "true";
            EnableAnalytics = element.GetAttributeValue("enableAnalytics") == "true";
            AllowDebug = element.GetAttributeValue("allowDebug") == "true";
            BrowserTitle = element.GetAttributeValue("browserTitle");
            DisableClientData = element.GetAttributeValue("disableClientData") == "true";
            DisableXmlControls = element.GetAttributeValue("disableXmlControls") == "true";
            DefaultDevice = element.GetAttributeValue("defaultDevice");
            XmlControlPage = element.GetAttributeValue("xmlControlPage");
            RegistryCacheSize = element.GetAttributeValue("registryCacheSize");
            ViewStateCacheSize = element.GetAttributeValue("viewStateCacheSize");
            XslCacheSize = element.GetAttributeValue("xslCacheSize");
            Inherits = element.GetAttributeValue("inherits");
        }
    }
}
