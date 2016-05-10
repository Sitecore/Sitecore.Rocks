// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    public abstract class ModeBase : IMode
    {
        private bool isReady;

        protected ModeBase([NotNull] Commandy commandy)
        {
            Debug.ArgumentNotNull(commandy, nameof(commandy));

            Commandy = commandy;
        }

        public string Alias { get; protected set; }

        [NotNull]
        public Commandy Commandy { get; }

        public bool IsReady
        {
            get { return isReady; }

            set
            {
                isReady = value;

                RaiseIsReadyChanged();
            }
        }

        public string Name { get; protected set; }

        public abstract string Watermark { get; }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public abstract void Execute(Hit hit, object parameter);

        public event EventHandler IsReadyChanged;

        public virtual IMode SwitchMode(string alias)
        {
            Assert.ArgumentNotNull(alias, nameof(alias));

            IMode mode = null;

            foreach (var d in Commandy.Modes)
            {
                if (string.Compare(d.Alias, alias, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    mode = d;
                    break;
                }
            }

            if (mode == null)
            {
                return null;
            }

            if (mode == this)
            {
                return null;
            }

            return mode;
        }

        protected void RaiseIsReadyChanged()
        {
            var e = IsReadyChanged;

            if (e != null)
            {
                e(this, EventArgs.Empty);
            }
        }
    }
}
