// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Controls.SyntaxEditors
{
    public enum Syntax
    {
        Text,

        Xslt,

        XHtml,

        Xml,

        Csharp,

        Css,

        JavaScript,

        PowerShell,

        Query,

        XPath
    }

    public partial class SyntaxEditor
    {
        public SyntaxEditor()
        {
            InitializeComponent();

            Editor.KeyDown += RaiseEditorKeyDown;
        }

        public int CaretOffset
        {
            get { return Editor.CaretIndex; }
            set { Editor.CaretIndex = value; }
        }

        [CanBeNull]
        public string FileName { get; set; }

        public bool IsReadOnly
        {
            get { return Editor.IsReadOnly; }
            set { Editor.IsReadOnly = value; }
        }

        public string SelectedText => Editor.SelectedText;

        public Syntax Syntax { get; set; }

        public string Text
        {
            get { return Editor.Text; }
            set { Editor.Text = value; }
        }

        public event KeyEventHandler EditorKeyDown;

        public void LoadCssSyntax()
        {
        }

        public void LoadFromFile([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            FileName = fileName;

            try
            {
                Text = AppHost.Files.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                AppHost.MessageBox(string.Format("The file \"{1}\" could not be opened.\n\n{0}", ex.Message, fileName), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                IsReadOnly = true;
            }
        }

        public void LoadJavaScriptSyntax()
        {
        }

        public void LoadQuerySyntax(string syntax)
        {
        }

        public void LoadXhtmlSyntax()
        {
        }

        public void ReplaceSelectedText(string text)
        {
            if (Editor.SelectionStart < 0)
            {
                return;
            }

            var s = Editor.Text;

            s = s.Left(Editor.SelectionStart) + text + s.Mid(Editor.SelectionStart + Editor.SelectionLength);

            var caretIndex = Editor.SelectionStart;
            Editor.Text = s;
            Editor.CaretIndex = caretIndex;
        }

        public void SaveFile()
        {
            var fileName = FileName;

            if (!string.IsNullOrEmpty(fileName))
            {
                SaveFile(fileName);
            }
        }

        public void SaveFile([NotNull] string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            AppHost.Files.WriteAllText(fileName, Editor.Text, Encoding.UTF8);
        }

        public event TextChangedEventHandler TextChanged;

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        private void RaiseEditorKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EditorKeyDown?.Invoke(sender, e);
        }
    }
}
