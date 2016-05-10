// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects
{
    public abstract class ProjectItem : ProjectItemBase
    {
        protected ProjectItem([NotNull] ProjectBase project) : base(project)
        {
            Debug.ArgumentNotNull(project, nameof(project));
        }

        public ConflictResolution ConflictResolution { get; set; }

        public bool HideFromToolbox { get; set; }

        public abstract bool IsAdded { get; }

        public bool IsConflict
        {
            get { return ConflictResolution != ConflictResolution.None; }
        }

        public abstract bool IsModified { get; }

        public abstract bool IsValid { get; }

        [NotNull]
        public abstract string Path { get; }

        public abstract void Commit([NotNull] ProcessedEventHandler callback);

        public abstract void Revert([NotNull] EventHandler<ProcessedEventArgs> callback);

        public abstract void Update([NotNull] ProcessedEventHandler callback);
    }
}
