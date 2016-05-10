// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.XmlFields
{
    public abstract class XmlOperation : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var control = context.Field.Control;
            if (control == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(control.GetValue()))
            {
                return false;
            }

            // TODO: test for field type control, not actual control
            return context.Field.Control is ISupportsXmlOperations;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var control = context.Field.Control;
            if (control == null)
            {
                return;
            }

            var value = control.GetValue();

            value = ProcessXml(value);

            control.SetValue(value);
        }

        [CanBeNull]
        protected XElement Parse([NotNull] string text)
        {
            Debug.ArgumentNotNull(text, nameof(text));

            try
            {
                var doc = XDocument.Parse(text);

                if (doc.Root != null)
                {
                    return doc.Root;
                }

                AppHost.MessageBox(@"Hmm... No root element.", Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppHost.MessageBox(string.Format(@"Nope, sorry, got an exception while parsing.\n\n{0}", ex.Message), Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return null;
        }

        [NotNull]
        protected abstract string ProcessXml([NotNull] string xml);
    }
}
