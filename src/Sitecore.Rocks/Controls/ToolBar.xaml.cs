// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.UserControlExtensions;

namespace Sitecore.Rocks.Controls
{
    public partial class ToolBar
    {
        public ToolBar()
        {
            InitializeComponent();
        }

        public virtual void UpdateCommands([NotNull] object context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Bar.Items.Clear();

            var commands = Commands.CommandManager.GetCommands(context).ToList();

            string group = null;
            var index = 0;
            foreach (var c in commands.OrderBy(cmd => cmd.Group).ThenBy(cmd => cmd.SortingValue).ThenBy(cmd => cmd.Text))
            {
                var command = c;
                if (group != command.Group)
                {
                    group = command.Group;

                    if (index > 0)
                    {
                        Bar.Items.Add(new Separator());
                    }
                }

                var button = new Button
                {
                    CommandParameter = context,
                    Command = command
                };

                button.SetValue(KeyboardNavigation.TabIndexProperty, index);

                if (command.Icon == null || command.Icon == Icon.Empty)
                {
                    button.Content = command.Text;
                }
                else
                {
                    var image = new Image
                    {
                        Source = command.Icon.GetSource(),
                        Style = (Style)FindResource("ToolButtonImage")
                    };

                    image.SetValue(KeyboardNavigation.IsTabStopProperty, false);

                    button.Content = image;
                    button.ToolTip = command.Text;
                }

                Bar.Items.Add(button);

                index++;
            }
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }
    }
}
