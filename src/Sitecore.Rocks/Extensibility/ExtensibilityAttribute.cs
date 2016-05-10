// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Extensibility
{
    public abstract class ExtensibilityAttribute : Attribute
    {
        public virtual void Initialize([CanBeNull] Type type)
        {
        }

        public virtual void PostInitialize([CanBeNull] Type type)
        {
        }

        public virtual void PreInitialize([CanBeNull] Type type)
        {
        }
    }
}
