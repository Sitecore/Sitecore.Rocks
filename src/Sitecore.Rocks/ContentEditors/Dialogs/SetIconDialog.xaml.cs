// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentEditors.Dialogs
{
    public partial class SetIconDialog
    {
        private readonly Dictionary<string, List<IconDescriptor>> iconDescriptors = new Dictionary<string, List<IconDescriptor>>();

        private readonly Dictionary<string, Image> images = new Dictionary<string, Image>();

        private readonly List<string> recentIcons;

        public SetIconDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            LoadIcons();

            recentIcons = LoadRecentIcons();

            RenderIcons(ApplicationsImage, "Applications");
            RenderIcons(BusinessImage, "Business");
            RenderIcons(ControlsImage, "Control");
            RenderIcons(Core1Image, "Core");
            RenderIcons(Core2Image, "Core2");
            RenderIcons(Core3Image, "Core3");
            RenderIcons(DatabaseImage, "Database");
            RenderIcons(FlagsImage, "Flags");
            RenderIcons(ImagingImage, "Imaging");
            RenderIcons(MultimediaImage, "Multimedia");
            RenderIcons(NetworkImage, "Network");
            RenderIcons(OtherImage, "Other");
            RenderIcons(PeopleImage, "People");
            RenderIcons(SoftwareImage, "Software");
            RenderIcons(WordProcessingImage, "WordProcessing");
            RenderIcons(OfficeImage, "Office");
            RenderIcons(OfficeWhiteImage, "OfficeWhite");
            RenderIcons(AppsImage, "Apps");
            RenderIcons(ApplicationsV2Image, "ApplicationsV2");
            RenderIcons(BusinessV2Image, "BusinessV2");
            RenderIcons(FlagsV2Image, "FlagsV2");
            RenderIcons(NetworkV2Image, "NetworkV2");
            RenderIcons(PeopleV2Image, "PeopleV2");
            RenderIcons(SoftwareV2Image, "SoftwareV2");

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string FileName
        {
            get { return Path.Text ?? string.Empty; }
        }

        [NotNull]
        protected Site Site { get; set; }

        public void Initialize([NotNull] Site site, [NotNull] string path)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(path, nameof(path));

            Site = site;
            Path.Text = path;

            RenderRecentIcons();

            var n = path.IndexOf('/');
            if (n < 0)
            {
                return;
            }

            var map = path.Left(n).ToLowerInvariant();

            switch (map)
            {
                case "applications":
                    ApplicationsTab.IsSelected = true;
                    break;
                case "business":
                    BusinessTab.IsSelected = true;
                    break;
                case "control":
                    ControlsTab.IsSelected = true;
                    break;
                case "core":
                    Core1Tab.IsSelected = true;
                    break;
                case "core2":
                    Core2Tab.IsSelected = true;
                    break;
                case "core3":
                    Core3Tab.IsSelected = true;
                    break;
                case "database":
                    DatabaseTab.IsSelected = true;
                    break;
                case "flags":
                    FlagsTab.IsSelected = true;
                    break;
                case "imaging":
                    ImagingTab.IsSelected = true;
                    break;
                case "multimedia":
                    MultimediaTab.IsSelected = true;
                    break;
                case "network":
                    NetworkTab.IsSelected = true;
                    break;
                case "other":
                    OtherTab.IsSelected = true;
                    break;
                case "people":
                    PeopleTab.IsSelected = true;
                    break;
                case "software":
                    SoftwareTab.IsSelected = true;
                    break;
                case "workprocessing":
                    WordProcessingTab.IsSelected = true;
                    break;
                case "office":
                    OfficeTab.IsSelected = true;
                    break;
                case "officewhite":
                    OfficeWhiteTab.IsSelected = true;
                    break;
                case "apps":
                    AppsTab.IsSelected = true;
                    break;
                case "applicationsv2":
                    ApplicationsV2Tab.IsSelected = true;
                    break;
                case "businessv2":
                    BusinessV2Tab.IsSelected = true;
                    break;
                case "flagsv2":
                    FlagsV2Tab.IsSelected = true;
                    break;
                case "networkv2":
                    NetworkV2Tab.IsSelected = true;
                    break;
                case "peoplev2":
                    PeopleV2Tab.IsSelected = true;
                    break;
                case "softwarev2":
                    SoftwareV2Tab.IsSelected = true;
                    break;
            }
        }

        public void ReloadImage()
        {
            Icon icon;

            var path = Path.Text;

            if (string.IsNullOrEmpty(path))
            {
                icon = new Icon(Data.Icon.MakePath("Resources/128x128/selection.png"));
            }
            else
            {
                path = Data.Icon.MakePath(path.Replace(@"16x16", @"32x32"));
                icon = new Icon(Site, path);
            }

            Image.Source = icon.GetSource();
        }

        private void CancelEnter([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            OfficeTab.Visibility = Site.SitecoreVersion >= Constants.Versions.Version80 ? Visibility.Visible : Visibility.Collapsed;
            AppsTab.Visibility = OfficeTab.Visibility;
            OfficeWhiteTab.Visibility = OfficeTab.Visibility;

            Keyboard.Focus(Path);
        }

        private void FilterChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            e.Handled = true;
            FilterComboBoxItem.IsSelected = true;
        }

        [NotNull]
        private static string GetFileName()
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "icons.xml");
        }

        [CanBeNull]
        private IconDescriptor GetIconDescriptor([NotNull] string mapName, Point position)
        {
            Debug.ArgumentNotNull(mapName, nameof(mapName));

            var list = iconDescriptors[mapName];
            if (list == null)
            {
                return null;
            }

            foreach (var iconDescriptor in list)
            {
                if (position.X >= iconDescriptor.X && position.X < iconDescriptor.X + 40 && position.Y >= iconDescriptor.Y && position.Y < iconDescriptor.Y + 40)
                {
                    return iconDescriptor;
                }
            }

            return null;
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var control = sender as FrameworkElement;
            if (control == null)
            {
                return;
            }

            var mapName = control.Tag as string ?? string.Empty;
            if (string.IsNullOrEmpty(mapName))
            {
                return;
            }

            var position = e.GetPosition((IInputElement)sender);

            var iconDescriptor = GetIconDescriptor(mapName, position);
            if (iconDescriptor == null)
            {
                return;
            }

            control.ToolTip = iconDescriptor.Name;
        }

        private void HandleMouseUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var control = sender as FrameworkElement;
            if (control == null)
            {
                return;
            }

            var mapName = control.Tag as string ?? string.Empty;
            if (string.IsNullOrEmpty(mapName))
            {
                return;
            }

            var position = e.GetPosition((IInputElement)sender);

            var iconDescriptor = GetIconDescriptor(mapName, position);
            if (iconDescriptor == null)
            {
                return;
            }

            Path.Text = string.Format(@"{0}/16x16/{1}.png", mapName, iconDescriptor.FileName);
        }

        private void LoadIcons()
        {
            iconDescriptors.Clear();

            var doc = XDocument.Load(GetFileName());

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            foreach (var map in root.Elements())
            {
                var mapName = map.GetAttributeValue("name");

                var list = new List<IconDescriptor>();

                foreach (var element in map.Elements())
                {
                    var name = element.GetAttributeValue("n");
                    var fileName = element.GetAttributeValue("p");

                    var c = element.GetAttributeValue("c");
                    var parts = c.Split(',');

                    int x;
                    int y;

                    int.TryParse(parts[0], out x);
                    int.TryParse(parts[1], out y);

                    var iconDescriptor = new IconDescriptor
                    {
                        Name = name,
                        FileName = fileName,
                        MapName = mapName,
                        X = x,
                        Y = y
                    };

                    list.Add(iconDescriptor);
                }

                iconDescriptors[mapName] = list;

                var checkBox = new CheckBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Content = mapName,
                    Tag = mapName
                };

                checkBox.Checked += Search;
                checkBox.Unchecked += Search;
                checkBox.PreviewMouseDown += Search;

                SearchFilterComboBox.Items.Add(checkBox);
            }
        }

        [NotNull]
        private List<string> LoadRecentIcons()
        {
            var result = new List<string>();

            for (var n = 0; n < 20; n++)
            {
                var icon = AppHost.Settings.Get(@"Icons\Recent", "Icon" + n, string.Empty) as string ?? string.Empty;
                if (string.IsNullOrEmpty(icon))
                {
                    break;
                }

                result.Add(icon);
            }

            return result;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            recentIcons.Remove(Path.Text);

            recentIcons.Insert(0, Path.Text);
            while (recentIcons.Count > 20)
            {
                recentIcons.RemoveAt(recentIcons.Count - 1);
            }

            SaveRecentIcons();

            this.Close(true);
        }

        private void RenderIcons([NotNull] Image image, [Localizable(false), NotNull] string imageName)
        {
            Debug.ArgumentNotNull(image, nameof(image));
            Debug.ArgumentNotNull(imageName, nameof(imageName));

            var path = $"https://raw.githubusercontent.com/Sitecore/Sitecore.Rocks/master/icons/icons_{imageName}.png";

            var policy = new RequestCachePolicy(RequestCacheLevel.Default);

            try
            {
                image.Source = new BitmapImage(new Uri(path), policy);
            }
            catch (OutOfMemoryException)
            {
                AppHost.MessageBox("Ouch, the image is too big to fit inside your computer.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                Image.Source = Data.Icon.Empty.GetSource();
            }

            images[imageName] = image;
        }

        private void RenderRecentIcons()
        {
            foreach (var recentIcon in recentIcons)
            {
                var path = recentIcon;

                if (Site.SitecoreVersion < Constants.Versions.Version80)
                {
                    if (path.IndexOf("/Apps/", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    if (path.IndexOf("/Office/", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    if (path.IndexOf("/OfficeWhite/", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        continue;
                    }
                }

                var image = new Image
                {
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(4)
                };

                image.MouseUp += (sender, args) => Path.Text = path;

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

                var icon = new Icon(Site, Data.Icon.MakePath(recentIcon.Replace(@"16x16", @"32x32")));

                image.Source = icon.GetSource();

                RecentIcons.Children.Add(image);
            }
        }

        private void SaveRecentIcons()
        {
            Storage.Delete(@"Icons\Recent");

            var index = 0;

            foreach (var recentIcon in recentIcons)
            {
                AppHost.Settings.Set(@"Icons\Recent", "Icon" + index, recentIcon);
                index++;
            }
        }

        private void Search([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (iconDescriptors == null)
            {
                return;
            }

            SearchWrapPanel.Children.Clear();

            var searchText = SearchTextBox.Text;
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            if (searchText.Length < 2)
            {
                return;
            }

            var filter = SearchFilterComboBox.Items.OfType<CheckBox>().Where(c => c.IsChecked == true).Select(c => c.Content as string).ToList();

            foreach (var descriptor in iconDescriptors)
            {
                if (filter.Count > 0 && !filter.Contains(descriptor.Key))
                {
                    continue;
                }

                if (Site.SitecoreVersion < Constants.Versions.Version80)
                {
                    if (descriptor.Key == "Apps" || descriptor.Key == "Office" || descriptor.Key == "OfficeWhite")
                    {
                        continue;
                    }
                }

                var image = images[descriptor.Key];
                var icons = descriptor.Value;

                foreach (var icon in icons)
                {
                    if (!icon.Name.IsFilterMatch(searchText))
                    {
                        continue;
                    }

                    var brush = new ImageBrush(image.Source)
                    {
                        Viewbox = new Rect(icon.X, icon.Y, 32, 32),
                        TileMode = TileMode.None,
                        Stretch = Stretch.None,
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewport = new Rect(0, 0, 32, 32),
                        ViewportUnits = BrushMappingMode.Absolute
                    };

                    var border = new Border
                    {
                        Background = brush,
                        Width = 32,
                        Height = 32,
                        Margin = new Thickness(4),
                        Tag = icon,
                        ToolTip = icon.Name
                    };

                    border.MouseUp += SelectIcon;

                    SearchWrapPanel.Children.Add(border);
                }
            }
        }

        private void SelectIcon([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var border = sender as Border;
            if (border == null)
            {
                return;
            }

            var iconDescriptor = border.Tag as IconDescriptor;
            if (iconDescriptor == null)
            {
                return;
            }

            Path.Text = string.Format(@"{0}/16x16/{1}.png", iconDescriptor.MapName, iconDescriptor.FileName);
        }

        private void TextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ReloadImage();
        }

        public class IconDescriptor
        {
            [NotNull]
            public string FileName { get; set; }

            [NotNull]
            public string MapName { get; set; }

            [NotNull]
            public string Name { get; set; }

            public int X { get; set; }

            public int Y { get; set; }
        }
    }
}
