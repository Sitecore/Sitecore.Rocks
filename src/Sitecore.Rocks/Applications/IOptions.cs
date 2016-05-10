// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Applications
{
    public enum AppTheme
    {
        Automatic,

        System,

        Dark
    }

    public interface IOptions
    {
        AppTheme AppTheme { get; set; }

        bool DisableIisIntegration { get; set; }

        bool EnableSitecoreRocksExtensions { get; set; }

        [CanBeNull]
        string FileTypes { get; set; }

        [UsedImplicitly]
        bool HideContentEditorHelp { get; set; }

        [UsedImplicitly]
        bool HideContentTreeHelp { get; set; }

        [UsedImplicitly]
        bool HideDragToUploadHelp { get; set; }

        bool HideGuidSearch { get; set; }

        [UsedImplicitly]
        bool HideImageFieldHelp { get; set; }

        bool HidePublishingDialog { get; set; }

        bool HideQuickInfo { get; set; }

        [UsedImplicitly]
        bool HideScriptMacroHelp { get; set; }

        bool HideUpdateDialog { get; set; }

        bool IsLogEnabled { get; set; }

        [NotNull]
        string MacrosFileName { get; set; }

        [NotNull]
        string PluginRespositoryUrl { get; set; }

        bool ReuseWindow { get; set; }

        [CanBeNull]
        string SharedFolder { get; set; }

        bool ShowFieldDisplayTitles { get; set; }

        bool ShowFieldInformation { get; set; }

        bool ShowGroupAndSortingValue { get; set; }

        bool ShowJobViewer { get; set; }

        bool ShowRawValues { get; set; }

        bool ShowStandardFields { get; set; }

        bool ShowStartPageOnStartup { get; set; }

        [CanBeNull]
        string Skin { get; set; }

        void Load();

        void Save();
    }
}
