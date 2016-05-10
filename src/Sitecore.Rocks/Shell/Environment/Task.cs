// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell.Environment
{
    public enum TaskCategory
    {
        Error,

        Message,

        Warning
    }

    public class Task
    {
        public Task(string text, TaskCategory category, string fileName, int line, int column)
        {
            Line = line;
            Column = column;
            Text = text;
            Category = category;
            FileName = fileName;
        }

        public TaskCategory Category { get; private set; }

        public int Column { get; private set; }

        [NotNull]
        public string FileName { get; private set; }

        public int Line { get; private set; }

        [NotNull]
        public string Text { get; private set; }
    }
}
