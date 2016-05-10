// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    [ExtensibilityInitialization(PreInit = @"Clear")]
    public static class PanelManager
    {
        private static readonly List<PanelDescriptor> panels = new List<PanelDescriptor>();

        [NotNull]
        public static IEnumerable<PanelDescriptor> Panels
        {
            get { return panels; }
        }

        [UsedImplicitly]
        public static void Clear()
        {
            panels.Clear();
        }

        internal static void LoadType([NotNull] Type type, [NotNull] PanelAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Debug.ArgumentNotNull(attribute, nameof(attribute));

            IPanel panel;
            try
            {
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                {
                    Trace.TraceError("Constructor not found.");
                    return;
                }

                panel = constructorInfo.Invoke(null) as IPanel;
            }
            catch
            {
                Trace.TraceError("Panel threw an exception in the constructor");
                return;
            }

            if (panel == null)
            {
                Trace.TraceError("Panel does not have a parameterless constructor");
                return;
            }

            var descriptor = new PanelDescriptor
            {
                Panel = panel,
                Name = attribute.Name,
                IsEnabled = attribute.EnabledByDefault,
                Priority = attribute.Priority
            };

            panels.Add(descriptor);
        }

        public class PanelDescriptor
        {
            public bool IsEnabled { get; set; }

            public bool IsVisible
            {
                get
                {
                    var value = AppHost.Settings.Get("ContentEditor\\Panels", Name, null);

                    if (value != null)
                    {
                        return (string)value == @"1";
                    }

                    return IsEnabled;
                }

                set { AppHost.Settings.Set("ContentEditor\\Panels", Name, value ? @"1" : @"0"); }
            }

            public string Name { get; set; }

            public IPanel Panel { get; set; }

            public double Priority { get; set; }
        }
    }
}
