// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility
{
    public class ExtensibilityInitializationAttribute : ExtensibilityAttribute
    {
        private static readonly object[] empty = new object[0];

        [Localizable(false)]
        public string Init { get; set; }

        [Localizable(false)]
        public string PostInit { get; set; }

        [Localizable(false)]
        public string PreInit { get; set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            Invoke(type, Init);
        }

        public override void PostInitialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            Invoke(type, PostInit);
        }

        public override void PreInitialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            Invoke(type, PreInit);
        }

        private void Invoke([NotNull] Type type, [CanBeNull] string methodName)
        {
            Debug.ArgumentNotNull(type, nameof(type));

            if (string.IsNullOrEmpty(methodName))
            {
                return;
            }

            var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            if (methodInfo == null)
            {
                throw Exceptions.InvalidOperation(string.Format(Resources.ExtensibilityInitializationAttribute_Invoke_Method_name___0___not_found_in___1__, methodName, type.FullName));
            }

            methodInfo.Invoke(null, empty);
        }
    }
}
