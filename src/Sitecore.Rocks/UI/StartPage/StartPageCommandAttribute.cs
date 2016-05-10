// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true), BaseTypeRequired(typeof(ICommand), typeof(IStartPageCommand)), MeansImplicitUse]
    public class StartPageCommandAttribute : Attribute
    {
        public StartPageCommandAttribute([NotNull, Localizable(false)] string text, [NotNull] string parentName, double priority)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(parentName, nameof(parentName));

            Text = text;
            ParentName = parentName;
            Priority = priority;
        }

        [NotNull]
        public string ParentName { get; set; }

        public double Priority { get; private set; }

        [NotNull]
        public string Text { get; private set; }
    }
}
