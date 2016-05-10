// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;

namespace Sitecore.Rocks.UI.About
{
    public partial class AboutBox
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        public string Company => GetValue<AssemblyCompanyAttribute>("Company");

        public string Copyright => GetValue<AssemblyCopyrightAttribute>("Copyright");

        public string Description => GetValue<AssemblyDescriptionAttribute>("Description");

        public string Product => GetValue<AssemblyProductAttribute>("Product");

        public string ProductTitle => GetValue<AssemblyTitleAttribute>("Title");

        public string Version => Assembly.GetExecutingAssembly().GetFileVersion().ToString();

        private string GetValue<T>([NotNull] string propertyName)
        {
            var result = string.Empty;

            var attributes = typeof(Notifications).Assembly.GetCustomAttributes(typeof(T), false);
            if (attributes.Length <= 0)
            {
                return result;
            }

            var attribute = (T)attributes[0];
            var property = attribute.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                result = property.GetValue(attributes[0], null) as string;
            }

            return result;
        }
    }
}
