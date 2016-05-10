// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;

namespace Sitecore.Rocks.ContentEditors.Validators
{
    public partial class ValidatorMarker : IContextProvider
    {
        private ValidatorResult result;

        [CanBeNull]
        private Validator validator;

        public ValidatorMarker()
        {
            InitializeComponent();
        }

        public ValidatorResult Result
        {
            get { return result; }

            set
            {
                result = value;

                var imageSource = string.Empty;
                switch (result)
                {
                    case ValidatorResult.Unknown:
                        imageSource = @"bullet_square_grey.png";
                        break;
                    case ValidatorResult.Valid:
                        imageSource = @"bullet_square_green.png";
                        break;
                    case ValidatorResult.Suggestion:
                        imageSource = @"bullet_square_green.png";
                        break;
                    case ValidatorResult.Warning:
                        imageSource = @"bullet_square_yellow.png";
                        break;
                    case ValidatorResult.Error:
                    case ValidatorResult.CriticalError:
                    case ValidatorResult.FatalError:
                        imageSource = @"bullet_square_red.png";
                        break;
                }

                if (string.IsNullOrEmpty(imageSource))
                {
                    return;
                }

                UpdateIcon(imageSource);
            }
        }

        [NotNull]
        public string Text
        {
            get { return Icon.ToolTip as string ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Icon.ToolTip = value;
            }
        }

        [NotNull]
        protected ContentEditor ContentEditor { get; set; }

        [NotNull]
        public object GetContext()
        {
            return new ValidatorContext(this, ContentEditor, validator);
        }

        public void SetValidator([NotNull] ContentEditor contentEditor, [NotNull] Validator validator)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));
            Assert.ArgumentNotNull(validator, nameof(validator));

            ContentEditor = contentEditor;
            this.validator = validator;

            Result = this.validator.Result;
            Text = this.validator.Text;
        }

        private void Clicked([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var v = validator;
            if (v == null)
            {
                return;
            }

            if (e.ClickCount != 1)
            {
                return;
            }

            e.Handled = true;

            var field = ContentEditor.ContentModel.Fields.FirstOrDefault(f => f.FieldUris.Any(uri => v.FieldId == uri.FieldId.ToGuid()));
            if (field == null)
            {
                return;
            }

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return;
            }

            var control = fieldControl.GetFocusableControl();
            if (control == null)
            {
                return;
            }

            Keyboard.Focus(control);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            if (validator == null)
            {
                return;
            }

            var context = GetContext();

            var commands = Rocks.Commands.CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        private void UpdateIcon([NotNull] string imageSource)
        {
            Assert.ArgumentNotNull(imageSource, nameof(imageSource));

            var source = new BitmapImage();

            source.BeginInit();
            source.UriSource = new Uri("pack://application:,,,/Sitecore.Rocks;component/Resources/16x16/" + imageSource);
            source.EndInit();

            Icon.Source = source;
        }
    }
}
