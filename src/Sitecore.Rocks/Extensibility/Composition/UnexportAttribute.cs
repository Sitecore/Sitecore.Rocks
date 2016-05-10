// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Extensibility.Composition
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class UnexportAttribute : ExtensibilityAttribute
    {
        public UnexportAttribute([NotNull] Type type)
        {
            Type = type;
        }

        [NotNull]
        public Type Type { get; private set; }
    }
}
