// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public partial class MessageText
    {
        public enum Type
        {
            Notification = 0,

            Error
        }

        public static readonly DependencyProperty MessageTypeProperty = DependencyProperty.Register(@"MessageType", typeof(Type), typeof(MessageText));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(@"Text", typeof(string), typeof(MessageText));

        public MessageText()
        {
            SetValue(MessageTypeProperty, Type.Notification);
            SetValue(TextProperty, string.Empty);

            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public Type MessageType
        {
            get { return (Type)GetValue(MessageTypeProperty); }

            set
            {
                SetValue(MessageTypeProperty, value);
                StyleMessageBox();
            }
        }

        [NotNull]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                SetValue(TextProperty, value);
                MessageTextBlock.Text = value;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            MessageTextBlock.Text = Text;
            StyleMessageBox();
        }

        private void StyleMessageBox()
        {
            // TODO: some actual formatting of the text-box depending on type.
        }
    }
}
