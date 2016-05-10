// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Text.Diff.Structures
{
    internal class DiffState
    {
        private const int BadIndex = -1;

        private int length;

        public DiffState()
        {
            SetToUnkown();
        }

        public int EndIndex
        {
            get { return StartIndex + length - 1; }
        }

        public int Length
        {
            get
            {
                if (length > 0)
                {
                    return length;
                }

                return length == 0 ? 1 : 0;
            }
        }

        public int StartIndex { get; private set; }

        public DiffStatus Status
        {
            get
            {
                DiffStatus stat;
                if (length > 0)
                {
                    stat = DiffStatus.Matched;
                }
                else
                {
                    switch (length)
                    {
                        case -1:
                            stat = DiffStatus.NoMatch;
                            break;
                        default:
                            Debug.Assert(length == -2, @"Invalid status: _length < -2");
                            stat = DiffStatus.Unknown;
                            break;
                    }
                }

                return stat;
            }
        }

        public bool HasValidLength(int newStart, int newEnd, int maxPossibleDestLength)
        {
            // have unlocked match
            if (length > 0)
            {
                if ((maxPossibleDestLength < length) || (StartIndex < newStart) || (EndIndex > newEnd))
                {
                    SetToUnkown();
                }
            }

            return length != (int)DiffStatus.Unknown;
        }

        public void SetMatch(int start, int newLength)
        {
            Debug.Assert(newLength > 0, @"Length must be greater than zero");
            Debug.Assert(start >= 0, @"Start must be greater than or equal to zero");
            StartIndex = start;
            length = newLength;
        }

        public void SetNoMatch()
        {
            StartIndex = BadIndex;
            length = (int)DiffStatus.NoMatch;
        }

        protected void SetToUnkown()
        {
            StartIndex = BadIndex;
            length = (int)DiffStatus.Unknown;
        }
    }
}
