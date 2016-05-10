// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.ContentEditors.Validators
{
    public partial class ValidatorBar
    {
        public ValidatorBar()
        {
            InitializeComponent();
        }

        public ContentEditor ContentEditor { get; set; }

        public void Update([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            if (contentModel.IsEmpty)
            {
                return;
            }

            if (contentModel.IsMultiple)
            {
                return;
            }

            RenderValidating();

            var item = contentModel.FirstItem;

            item.Uri.ItemUri.Site.DataService.Validate(item.Uri.ItemUri.DatabaseUri, contentModel.Fields, ValidateCallback);
        }

        private void RenderStatus([NotNull] IEnumerable<Validator> validators)
        {
            Debug.ArgumentNotNull(validators, nameof(validators));

            var errors = 0;
            var warnings = 0;

            foreach (var validator in validators)
            {
                switch (validator.Result)
                {
                    case ValidatorResult.Warning:
                        warnings++;
                        break;
                    case ValidatorResult.Error:
                    case ValidatorResult.CriticalError:
                    case ValidatorResult.FatalError:
                        errors++;
                        break;
                }
            }

            var marker = new ValidatorMarker();

            if (warnings == 0 && errors == 0)
            {
                marker.Text = Rocks.Resources.ValidatorBar_RenderStatus_No_errors_or_warnings_;
                marker.Result = ValidatorResult.Valid;
            }
            else if (errors == 0)
            {
                marker.Text = warnings + Rocks.Resources.ValidatorBar_RenderStatus__warning_s__;
                marker.Result = ValidatorResult.Warning;
            }
            else if (warnings == 0)
            {
                marker.Text = errors + Rocks.Resources.ValidatorBar_RenderStatus__error_s__;
                marker.Result = ValidatorResult.Error;
            }
            else
            {
                marker.Text = string.Format(Rocks.Resources.ValidatorBar_RenderStatus__0__error_s__and__1__warnings_, errors, warnings);
                marker.Result = ValidatorResult.Error;
            }

            marker.MouseDoubleClick += Revalidate;

            Bar.Children.Add(marker);
        }

        private void RenderValidating()
        {
            Bar.Children.Clear();

            var marker = new ValidatorMarker
            {
                Text = Rocks.Resources.ValidatorBar_RenderValidating_Validating___,
                Result = ValidatorResult.Unknown
            };

            Bar.Children.Add(marker);
        }

        private void RenderValidatorBar([NotNull] IEnumerable<Validator> validators)
        {
            Debug.ArgumentNotNull(validators, nameof(validators));

            Bar.Children.Clear();

            RenderStatus(validators);
            RenderValidators(validators);
        }

        private void RenderValidators([NotNull] IEnumerable<Validator> validators)
        {
            Debug.ArgumentNotNull(validators, nameof(validators));

            foreach (var validator in validators)
            {
                var marker = new ValidatorMarker();
                marker.SetValidator(ContentEditor, validator);

                Bar.Children.Add(marker);
            }
        }

        private void Revalidate([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Update(ContentEditor.ContentModel);
        }

        private void ValidateCallback([NotNull] IEnumerable<Validator> validators)
        {
            Debug.ArgumentNotNull(validators, nameof(validators));

            Dispatcher.Invoke(new Action(() => RenderValidatorBar(validators)));
        }
    }
}
