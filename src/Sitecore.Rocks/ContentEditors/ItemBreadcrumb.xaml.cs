// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public partial class ItemBreadcrumb
    {
        private Icon icon;

        static ItemBreadcrumb()
        {
            NavigateBreadcrumb = new RoutedCommand(@"NavigateCommand", typeof(ItemBreadcrumb));
        }

        public ItemBreadcrumb()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public Icon Icon
        {
            get { return icon; }

            set
            {
                Assert.IsNotNull(value, "value");

                icon = value;
                BreadcrumbIcon.Source = value.GetSource();
            }
        }

        [NotNull]
        public static RoutedCommand NavigateBreadcrumb { get; set; }

        [NotNull]
        protected ContentEditor ContentEditor { get; private set; }

        [CanBeNull]
        protected ContentModel ContentModel { get; set; }

        protected bool IsUpdating { get; set; }

        public void EndLoading()
        {
        }

        public void Initialize([NotNull] ContentEditor contentEditor)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            ContentEditor = contentEditor;
        }

        public void StartLoading()
        {
        }

        public void Update([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            ContentModel = contentModel;

            if (!contentModel.IsSingle)
            {
                Breadcrumb.Text = string.Empty;
                BreadcrumbIcon.Source = new Icon("Resources/16x16/cube_blue.png").GetSource();
                return;
            }

            BreadcrumbIcon.Source = contentModel.FirstItem.Icon.GetSource();

            Breadcrumb.Text = contentModel.FirstItem.GetPath();
        }

        private void CanNavigate([NotNull] object sender, [NotNull] CanExecuteRoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (ContentModel == null)
            {
                e.CanExecute = true;
                return;
            }

            e.CanExecute = !ContentModel.IsEmpty;
        }
    }
}
