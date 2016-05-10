// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateFieldSorter
{
    public partial class TemplateField
    {
        public TemplateField()
        {
            InitializeComponent();
        }

        protected TemplateFieldSorter.Field Field { get; set; }

        [NotNull]
        protected TemplateFieldSorter TemplateFieldSorter { get; set; }

        public void Initialize([NotNull] TemplateFieldSorter templateFieldSorter, [NotNull] TemplateFieldSorter.Field field)
        {
            Assert.ArgumentNotNull(templateFieldSorter, nameof(templateFieldSorter));
            Assert.ArgumentNotNull(field, nameof(field));

            TemplateFieldSorter = templateFieldSorter;
            Field = field;
            FieldName.Content = Field.Name;
            SortOrder.Text = field.SortOrder.ToString();
            FieldType.Text = field.Type;
            TemplateName.Text = field.TemplateName;
            Icon.Source = field.TemplateIcon.GetSource();

            if (field.IsInherited)
            {
                Details.Foreground = SystemColors.GrayTextBrush;
            }
        }

        private void SortOrderChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            int value;
            int.TryParse(SortOrder.Text, out value);

            TemplateFieldSorter.SetSortOrder(Field, Field.TemplateFieldId, value);
        }
    }
}
