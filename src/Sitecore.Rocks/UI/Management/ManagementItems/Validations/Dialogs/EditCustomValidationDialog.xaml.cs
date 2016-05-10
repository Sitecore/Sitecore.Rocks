// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls.QueryBuilders;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs
{
    public partial class EditCustomValidationDialog
    {
        public EditCustomValidationDialog([NotNull] CustomValidation customValidation)
        {
            Assert.ArgumentNotNull(customValidation, nameof(customValidation));

            InitializeComponent();
            this.InitializeDialog();

            CustomValidation = customValidation;

            switch (CustomValidation.Type)
            {
                case CustomValidationType.XPath:
                    XPath.IsSelected = true;
                    break;
                case CustomValidationType.Query:
                    Query.IsSelected = true;
                    break;
                case CustomValidationType.WebConfig:
                    WebConfig.IsSelected = true;
                    break;
                case CustomValidationType.ExpandedWebConfig:
                    ExpandedWebConfig.IsSelected = true;
                    break;
                case CustomValidationType.WebFileSystem:
                    WebFileSystem.IsSelected = true;
                    break;
                case CustomValidationType.DataFileSystem:
                    DataFileSystem.IsSelected = true;
                    break;
            }

            switch (CustomValidation.Severity)
            {
                case SeverityLevel.Error:
                    Error.IsSelected = true;
                    break;
                case SeverityLevel.Warning:
                    Warning.IsSelected = true;
                    break;
                case SeverityLevel.Suggestion:
                    Suggestion.IsSelected = true;
                    break;
                case SeverityLevel.Hint:
                    Hint.IsSelected = true;
                    break;
            }

            Editor.Text = customValidation.Code;
            Category.Text = customValidation.Category;
            TitleField.Text = customValidation.Title;
            Problem.Text = customValidation.Problem;
            Solution.Text = customValidation.Solution;
            FixEditor.Text = customValidation.Fix;

            if (customValidation.WhenExists)
            {
                WhenExists.IsSelected = true;
            }
            else
            {
                WhenNotExists.IsSelected = true;
            }

            EnableButtons();
        }

        [NotNull]
        public CustomValidation CustomValidation { get; set; }

        private void BuildQuery([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var query = Editor.Text;
            var type = GetCustomValidationType();

            var dialog = new BuildQueryDialog(query, type);
            if (AppHost.Shell.ShowDialog(dialog) == true)
            {
                Editor.Text = dialog.Text;
            }
        }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void EnableButtons()
        {
            OkButton.IsEnabled = !string.IsNullOrEmpty(Editor.Text) && !string.IsNullOrEmpty(TitleField.Text) && !string.IsNullOrEmpty(Category.Text) && !string.IsNullOrEmpty(Problem.Text) && !string.IsNullOrEmpty(Solution.Text);
        }

        private void EnableButtons([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private CustomValidationType GetCustomValidationType()
        {
            if (XPath.IsSelected)
            {
                return CustomValidationType.XPath;
            }

            if (Query.IsSelected)
            {
                return CustomValidationType.Query;
            }

            if (WebConfig.IsSelected)
            {
                return CustomValidationType.WebConfig;
            }

            if (ExpandedWebConfig.IsSelected)
            {
                return CustomValidationType.ExpandedWebConfig;
            }

            if (WebFileSystem.IsSelected)
            {
                return CustomValidationType.WebFileSystem;
            }

            return CustomValidationType.DataFileSystem;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            CustomValidation.Type = GetCustomValidationType();

            if (Error.IsSelected)
            {
                CustomValidation.Severity = SeverityLevel.Error;
            }
            else if (Warning.IsSelected)
            {
                CustomValidation.Severity = SeverityLevel.Warning;
            }
            else if (Suggestion.IsSelected)
            {
                CustomValidation.Severity = SeverityLevel.Suggestion;
            }
            else if (Hint.IsSelected)
            {
                CustomValidation.Severity = SeverityLevel.Hint;
            }

            CustomValidation.Code = Editor.Text;
            CustomValidation.Category = Category.Text ?? string.Empty;
            CustomValidation.Title = TitleField.Text ?? string.Empty;
            CustomValidation.Problem = Problem.Text ?? string.Empty;
            CustomValidation.Solution = Solution.Text ?? string.Empty;
            CustomValidation.Fix = FixEditor.Text;
            CustomValidation.WhenExists = WhenExists.IsSelected;

            this.Close(true);
        }

        private void SetType([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (FixEditor == null)
            {
                return;
            }

            var isEnabled = ValidationType.SelectedIndex == 0 || ValidationType.SelectedIndex == 1;
            FixEditor.IsEnabled = isEnabled;
            FixLabel.IsEnabled = isEnabled;

            if (!isEnabled)
            {
                FixEditor.Visibility = Visibility.Collapsed;
                NoFix.Visibility = Visibility.Visible;
            }
            else
            {
                FixEditor.Visibility = Visibility.Visible;
                NoFix.Visibility = Visibility.Collapsed;
            }
        }
    }
}
