// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Sitecore.VisualStudio.Annotations;

namespace Sitecore.VisualStudio.Shell
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideXmlEditorChooserDesignerViewAttribute : RegistrationAttribute
    {
        private const string XmlChooserEditorExtensionsKeyPath = @"Editors\{32CC8DFA-2D70-49b2-94CD-22D57349B778}\Extensions";

        private const string XmlChooserFactory = "XmlChooserFactory";

        private const string XmlEditorFactoryGuid = "{FA3CD31E-987B-443A-9B81-186104E8DAC1}";

        private readonly string extension;

        private readonly string name;

        private readonly int xmlChooserPriority;

        private Guid defaultLogicalView;

        [Localizable(false)]
        public ProvideXmlEditorChooserDesignerViewAttribute([CanBeNull] string name, [CanBeNull] string extension, [CanBeNull] object defaultLogicalViewEditorFactory, int xmlChooserPriority)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(@"Editor description cannot be null or empty.", @"editorDescription");
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException(@"Extension cannot be null or empty.", @"extension");
            }

            if (defaultLogicalViewEditorFactory == null)
            {
                throw new ArgumentNullException(@"defaultLogicalViewEditorFactory");
            }

            this.name = name;
            this.extension = extension;
            defaultLogicalView = TryGetGuidFromObject(defaultLogicalViewEditorFactory);
            this.xmlChooserPriority = xmlChooserPriority;

            CodeLogicalViewEditor = XmlEditorFactoryGuid;
            DebuggingLogicalViewEditor = XmlEditorFactoryGuid;
            DesignerLogicalViewEditor = XmlEditorFactoryGuid;
            TextLogicalViewEditor = XmlEditorFactoryGuid;
        }

        public object CodeLogicalViewEditor { get; set; }

        public object DebuggingLogicalViewEditor { get; set; }

        public object DesignerLogicalViewEditor { get; set; }

        public bool? IsDataSet { get; set; }

        public bool MatchExtensionAndNamespace { get; set; }

        public string Namespace { get; set; }

        public object TextLogicalViewEditor { get; set; }

        public override void Register([CanBeNull] RegistrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(@"context");
            }

            using (var xmlChooserExtensions = context.CreateKey(XmlChooserEditorExtensionsKeyPath))
            {
                xmlChooserExtensions.SetValue(extension, xmlChooserPriority);
            }

            using (var key = context.CreateKey(GetKeyName()))
            {
                key.SetValue(@"DefaultLogicalView", defaultLogicalView.ToString(@"B").ToUpperInvariant());
                key.SetValue(@"Extension", extension);

                if (!string.IsNullOrWhiteSpace(Namespace))
                {
                    key.SetValue(@"Namespace", Namespace);
                }

                if (MatchExtensionAndNamespace)
                {
                    key.SetValue(@"Match", @"both");
                }

                if (IsDataSet.HasValue)
                {
                    key.SetValue(@"IsDataSet", Convert.ToInt32(IsDataSet.Value));
                }

                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_Debugging, DebuggingLogicalViewEditor);
                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_Code, CodeLogicalViewEditor);
                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_Designer, DesignerLogicalViewEditor);
                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_TextView, TextLogicalViewEditor);
            }
        }

        public override void Unregister([CanBeNull] RegistrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(@"context");
            }

            context.RemoveKey(GetKeyName());

            context.RemoveValue(XmlChooserEditorExtensionsKeyPath, extension);
            context.RemoveKeyIfEmpty(XmlChooserEditorExtensionsKeyPath);
        }

        [NotNull]
        private string GetKeyName()
        {
            return Path.Combine(XmlChooserFactory, name);
        }

        private void SetLogicalViewMapping([CanBeNull] Key key, Guid logicalView, [CanBeNull] object editorFactory)
        {
            if (key != null && editorFactory != null)
            {
                key.SetValue(logicalView.ToString(@"B").ToUpperInvariant(), TryGetGuidFromObject(editorFactory).ToString(@"B").ToUpperInvariant());
            }
        }

        private Guid TryGetGuidFromObject([CanBeNull] object guidObject)
        {
            // figure out what type of object they passed in and get the GUID from it
            if (guidObject is string)
            {
                return new Guid((string)guidObject);
            }

            if (guidObject is Type)
            {
                return ((Type)guidObject).GUID;
            }

            if (guidObject is Guid)
            {
                return (Guid)guidObject;
            }

            throw new ArgumentException(@"Could not determine Guid from supplied object.", @"guidObject");
        }
    }
}
