// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true), UsedImplicitly]
    public class CustomToolRegistrationAttribute : RegistrationAttribute
    {
        public CustomToolRegistrationAttribute([NotNull] string name, [NotNull] Type customToolType)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(customToolType, nameof(customToolType));

            Name = name;
            CustomToolType = customToolType;
        }

        public Type CustomToolType { get; set; }

        public string Name { get; set; }

        public override void Register([NotNull] RegistrationContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Register(context.CreateKey, (x, key, value) => x.SetValue(key, value));
        }

        public void Register<T>([NotNull] Func<string, T> keyCreator, [NotNull] Action<T, string, object> valueCreator)
        {
            Assert.ArgumentNotNull(keyCreator, nameof(keyCreator));
            Assert.ArgumentNotNull(valueCreator, nameof(valueCreator));

            var keyName = CreateKeyName(Name);
            var key = keyCreator(keyName);

            valueCreator(key, string.Empty, Name);
            valueCreator(key, @"CLSID", CustomToolType.GUID.ToString(@"B"));
            valueCreator(key, @"GeneratesDesignTimeSource", 1);
            valueCreator(key, @"GeneratesSharedDesignTimeSource", 1);

            var disposable = key as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        public override void Unregister([NotNull] RegistrationContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Unregister(context.RemoveKey);
        }

        public void Unregister([NotNull] Action<string> keyRemover)
        {
            Assert.ArgumentNotNull(keyRemover, nameof(keyRemover));

            keyRemover(CreateKeyName(Name));
        }

        [NotNull]
        private static string CreateKeyName([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            return string.Format(@"Generators\{0}\{1}", @"{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}", name);
        }
    }
}
