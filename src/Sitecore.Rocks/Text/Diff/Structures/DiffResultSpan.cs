// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Text.Diff.Structures
{
    public class DiffResultSpan : IComparable
    {
        private const int BadIndex = -1;

        protected DiffResultSpan(DiffResultSpanStatus status, int destIndex, int sourceIndex, int length)
        {
            Status = status;
            DestIndex = destIndex;
            SourceIndex = sourceIndex;
            Length = length;
        }

        public int DestIndex { get; }

        public int Length { get; private set; }

        public DiffResultSpan Link { get; private set; }

        public int SourceIndex { get; }

        public DiffResultSpanStatus Status { get; }

        public void AddLength(int i)
        {
            Length += i;
        }

        public int CompareTo([NotNull] object obj)
        {
            Assert.ArgumentNotNull(obj, nameof(obj));

            return DestIndex.CompareTo(((DiffResultSpan)obj).DestIndex);
        }

        [NotNull]
        public static DiffResultSpan CreateAddDestination(int destIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.AddDestination, destIndex, BadIndex, length);
        }

        [NotNull]
        public static DiffResultSpan CreateDeleteSource(int sourceIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.DeleteSource, BadIndex, sourceIndex, length);
        }

        [NotNull]
        public static DiffResultSpan CreateNoChange(int destIndex, int sourceIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.NoChange, destIndex, sourceIndex, length);
        }

        [NotNull]
        public static DiffResultSpan CreateReplace(int destIndex, int sourceIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.Replace, destIndex, sourceIndex, length);
        }

        public void SetLink([NotNull] DiffResultSpan link)
        {
            Assert.ArgumentNotNull(link, nameof(link));

            Link = link;
            link.Link = this;
        }

        [NotNull]
        public override string ToString()
        {
            return string.Format(@"{0} (Dest: {1},Source: {2}) {3}", Status.ToString(), DestIndex.ToString(), SourceIndex.ToString(), Length.ToString());
        }
    }
}
