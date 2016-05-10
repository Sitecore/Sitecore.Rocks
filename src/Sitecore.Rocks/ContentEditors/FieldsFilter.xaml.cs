// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Threading;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public partial class FieldsFilter
    {
        public delegate void FilterChangedDelegate(object sender, string filterText);

        private Timer timer;

        public FieldsFilter()
        {
            InitializeComponent();
        }

        public event FilterChangedDelegate FilterChanged;

        private void ExecuteFilter([CanBeNull] object state)
        {
            StopTimer();

            Dispatcher.Invoke(new Action(delegate
            {
                var filterChanged = FilterChanged;

                if (filterChanged != null)
                {
                    filterChanged(this, Filter.Text);
                }
            }));
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            StopTimer();

            timer = new Timer(ExecuteFilter, null, 350, int.MaxValue);
        }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }
    }
}
