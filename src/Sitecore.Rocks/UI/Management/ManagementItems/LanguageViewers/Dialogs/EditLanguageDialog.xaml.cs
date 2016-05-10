// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers.Dialogs
{
    public partial class EditLanguageDialog
    {
        public EditLanguageDialog([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            InitializeComponent();
            this.InitializeDialog();

            DatabaseUri = databaseUri;
            IconPath = "flags/16x16/flag_generic.png";

            LoadLanguages();
            RenderLanguages();
            EnableButtons();
            ReloadIcon();
        }

        public string IconPath { get; set; }

        protected DatabaseUri DatabaseUri { get; set; }

        protected XDocument LanguageDefinitions { get; set; }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new SetIconDialog();
            d.Initialize(DatabaseUri.Site, IconPath);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            IconPath = d.FileName;

            ReloadIcon();
        }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void EnableButtons()
        {
            OK.IsEnabled = !string.IsNullOrEmpty(LanguageName.Text) && !string.IsNullOrEmpty(CountryCode.Text) && !string.IsNullOrEmpty(IconPath);
        }

        private void LoadLanguages()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sitecore.Rocks.Resources.LanguageDefinitions.xml"))
            {
                if (stream == null)
                {
                    Trace.TraceError("Sitecore.Rocks.Resources.LanguageDefinitions.xml resource not found.");
                    LanguageDefinitions = new XDocument();
                    return;
                }

                LanguageDefinitions = XDocument.Load(stream);
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void PredefinedChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = PredefinedLanguage.SelectedItem as ComboBoxItem;
            if (item == null)
            {
                return;
            }

            var element = item.Tag as XElement;
            if (element == null)
            {
                return;
            }

            LanguageName.Text = element.GetAttributeValue("id");
            CountryCode.Text = element.GetAttributeValue("region");
            CustomCode.Text = string.Empty;
            CodePage.Text = element.GetAttributeValue("codepage");
            Encoding.Text = element.GetAttributeValue("encoding");
            Charset.Text = element.GetAttributeValue("charset");
            SpellcheckerPath.Text = element.GetAttributeValue("spellchecker");
            IconPath = element.GetAttributeValue("icon");

            ReloadIcon();
        }

        private void ReloadIcon()
        {
            var source = IconPath;
            if (string.IsNullOrEmpty(source))
            {
                source = "flags/16x16/flag_generic.png";
            }

            var iconPath = Data.Icon.MakePath(source.Replace("16x16", "32x32"));

            var icon = new Icon(DatabaseUri.Site, iconPath);

            Image.Source = icon.GetSource();
        }

        private void RenderLanguages()
        {
            var root = LanguageDefinitions.Root;
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                var language = element.GetAttributeValue("id");
                var region = element.GetAttributeValue("region");
                var custom = element.GetAttributeValue("custom");

                var code = language;

                if (!string.IsNullOrEmpty(region))
                {
                    code += "-" + region;
                }

                if (!string.IsNullOrEmpty(custom))
                {
                    code += "-" + custom;
                }

                string displayName;
                try
                {
                    var cultureInfo = new CultureInfo(code);

                    if ((cultureInfo.CultureTypes & CultureTypes.UserCustomCulture) == CultureTypes.UserCustomCulture)
                    {
                        displayName = string.Format("{0} : {1}", cultureInfo.Name, cultureInfo.NativeName);
                    }
                    else
                    {
                        displayName = cultureInfo.EnglishName + " : " + cultureInfo.NativeName;
                    }
                }
                catch
                {
                    displayName = language;
                }

                var comboBoxItem = new ComboBoxItem
                {
                    Content = displayName,
                    Tag = element
                };

                PredefinedLanguage.Items.Add(comboBoxItem);
            }
        }

        private void TextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }
    }
}
