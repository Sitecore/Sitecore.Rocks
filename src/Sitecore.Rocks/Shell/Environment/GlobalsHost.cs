// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class GlobalsHost : INotifyPropertyChanged
    {
        private Language _currentLanguage;

        public GlobalsHost()
        {
            _currentLanguage = new Language(@"en");
        }

        [NotNull]
        public Language CurrentLanguage
        {
            get { return _currentLanguage; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (_currentLanguage == value)
                {
                    return;
                }

                _currentLanguage = value;
                OnPropertyChanged("CurrentLanguage");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([NotNull] string propertyName)
        {
            Debug.ArgumentNotNull(propertyName, nameof(propertyName));

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
