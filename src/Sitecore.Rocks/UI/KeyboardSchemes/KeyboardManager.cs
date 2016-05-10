// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.KeyboardSchemes
{
    public static class KeyboardManager
    {
        public static int IsActive { get; set; }

        [NotNull]
        public static List<KeyboardShortcut> Shortcuts { get; } = new List<KeyboardShortcut>();

        public static IEnumerable<string> GetCommandNames(List<Tuple<ModifierKeys, Key>> buffer)
        {
            foreach (var shortcut in Shortcuts.Where(s => s.Keys.Any()))
            {
                var i0 = shortcut.Keys.Count() - 1;
                var i1 = buffer.Count - 1;

                while (i0 >= 0 && i1 >= 0)
                {
                    var t0 = shortcut.Keys.ElementAt(i0);
                    var t1 = buffer.ElementAt(i1);

                    if (t0.Item1 != t1.Item1 || t0.Item2 != t1.Item2)
                    {
                        break;
                    }

                    if (i0 == 0)
                    {
                        yield return shortcut.CommandName;
                    }

                    i0--;
                    i1--;
                }
            }
        }

        [NotNull]
        public static IEnumerable<KeyboardShortcut> Load([NotNull] string fileName)
        {
            var xml = File.ReadAllText(fileName);
            var root = xml.ToXElement();
            if (root == null)
            {
                return Enumerable.Empty<KeyboardShortcut>();
            }

            var result = new List<KeyboardShortcut>();

            foreach (var element in root.Elements())
            {
                var commandName = element.GetAttributeValue("Command");
                var keys = element.GetAttributeValue("Keys");

                var shortcut = new KeyboardShortcut(commandName, keys);
                result.Add(shortcut);
            }

            return result;
        }

        public static void LoadActiveScheme()
        {
            Shortcuts.Clear();

            var fileName = Path.Combine(AppHost.User.UserFolder, "KeyboardScheme\\KeyboardScheme.xml");
            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? string.Empty, "KeyboardScheme.xml");
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            Shortcuts.AddRange(Load(fileName));
        }

        public static void Save(string fileName, IEnumerable<KeyboardShortcut> shortcuts)
        {
            using (var stream = new StreamWriter(fileName))
            {
                var output = new XmlTextWriter(stream);
                output.Formatting = Formatting.Indented;

                output.WriteStartElement("Keyboard");

                foreach (var shortcut in shortcuts.OrderBy(s => s.CommandName))
                {
                    output.WriteStartElement("Shortcut");

                    output.WriteAttributeString("Command", shortcut.CommandName);
                    output.WriteAttributeString("Keys", shortcut.FormattedKeys);

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }
        }

        public static void SaveActiveScheme()
        {
            var directory = Path.Combine(AppHost.User.UserFolder, "KeyboardScheme");
            Directory.CreateDirectory(directory);

            var fileName = Path.Combine(directory, "KeyboardScheme.xml");

            Save(fileName, Shortcuts);
        }

        public static void SetKeyboardShortcut([NotNull] string commandName, [NotNull] IEnumerable<Tuple<ModifierKeys, Key>> keys)
        {
            var shortcut = Shortcuts.FirstOrDefault(s => s.CommandName == commandName);
            if (shortcut != null)
            {
                Shortcuts.Remove(shortcut);
            }

            shortcut = new KeyboardShortcut(commandName, keys);
            Shortcuts.Add(shortcut);

            SaveActiveScheme();
        }
    }
}
