// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class TasksHost
    {
        public virtual void Add([NotNull] string taskListName, [NotNull] Task task)
        {
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));
            Assert.ArgumentNotNull(task, nameof(task));

            throw new InvalidOperationException("Tasks host must be overridden.");
        }

        public virtual void Add([NotNull] string taskListName, [NotNull] IEnumerable<Task> tasks)
        {
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));
            Assert.ArgumentNotNull(tasks, nameof(tasks));

            foreach (var task in tasks)
            {
                Add(taskListName, task);
            }
        }

        public virtual void Clear([NotNull] string taskListName, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            throw new InvalidOperationException("Tasks host must be overridden.");
        }

        public virtual void Show([NotNull] string taskListName)
        {
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));

            throw new InvalidOperationException("Tasks host must be overridden.");
        }
    }
}
