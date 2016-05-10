// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    public abstract class SearchCommand : CommandBase
    {
        protected SearchCommand()
        {
            Group = "Search";
        }

        [NotNull, Localizable(false)]
        protected string FieldName { get; set; }

        [NotNull, Localizable(false)]
        protected string FieldPath { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (!contentModel.IsSingle)
            {
                return false;
            }

            var site = contentModel.FirstItem.Uri.Site;
            if ((site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                return false;
            }

            var value = GetValue(contentModel);
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty || contentModel.IsMultiple)
            {
                return;
            }

            var value = GetValue(contentModel);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var searchViewer = AppHost.Windows.OpenSearchViewer(contentModel.FirstItem.Uri.Site);
            if (searchViewer == null)
            {
                return;
            }

            searchViewer.Search(FieldName, @"""" + value + @"""");
        }

        [CanBeNull]
        protected virtual string GetValue([NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            var fieldId = IdManager.GetFieldId(FieldPath);

            foreach (var field in contentModel.Fields)
            {
                var fieldControl = field.Control;
                if (fieldControl == null)
                {
                    continue;
                }

                if (field.FieldUris.First().FieldId == fieldId)
                {
                    return fieldControl.GetValue();
                }
            }

            return null;
        }
    }
}
