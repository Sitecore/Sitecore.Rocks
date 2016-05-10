// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateFieldSorter
{
    public partial class TemplateSection
    {
        public TemplateSection()
        {
            InitializeComponent();
        }

        public string SectionId { get; set; }

        protected TemplateFieldSorter.TemplateFields TemplateFields { get; set; }

        [NotNull]
        protected TemplateFieldSorter TemplateFieldSorter { get; set; }

        public void Initialize([NotNull] TemplateFieldSorter templateFieldSorter, [NotNull] TemplateFieldSorter.TemplateFields templateFields, [NotNull] string sectionName, int sectionSortOrder, [NotNull] string sectionId)
        {
            Assert.ArgumentNotNull(templateFieldSorter, nameof(templateFieldSorter));
            Assert.ArgumentNotNull(templateFields, nameof(templateFields));
            Assert.ArgumentNotNull(sectionName, nameof(sectionName));
            Assert.ArgumentNotNull(sectionId, nameof(sectionId));

            TemplateFieldSorter = templateFieldSorter;
            TemplateFields = templateFields;
            SectionSortOrder.Text = sectionSortOrder.ToString();
            SectionName.Text = sectionName;
            SectionId = sectionId;
        }

        private void SortOrderChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (string.IsNullOrEmpty(SectionId))
            {
                return;
            }

            int value;
            int.TryParse(SectionSortOrder.Text, out value);

            TemplateFieldSorter.SetSectionSortOrder(this, SectionId, value);
        }
    }
}
