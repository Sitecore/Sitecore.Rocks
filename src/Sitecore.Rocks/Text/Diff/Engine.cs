// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using Sitecore.Rocks.Text.Diff.Structures;

namespace Sitecore.Rocks.Text.Diff
{
    public enum DiffEngineLevel
    {
        FastImperfect,

        Medium,

        SlowPerfect
    }

    public class DiffEngine
    {
        private IDiffList dest;

        private DiffEngineLevel level;

        private ArrayList matchList;

        private IDiffList source;

        private DiffStateList stateList;

        public DiffEngine()
        {
            source = null;
            dest = null;
            matchList = null;
            stateList = null;
            level = DiffEngineLevel.FastImperfect;
        }

        public ArrayList DiffReport()
        {
            var retval = new ArrayList();
            var dcount = dest.Count();
            var scount = source.Count();

            //Deal with the special case of empty files
            if (dcount == 0)
            {
                if (scount > 0)
                {
                    retval.Add(DiffResultSpan.CreateDeleteSource(0, scount));
                }
                return retval;
            }
            else
            {
                if (scount == 0)
                {
                    retval.Add(DiffResultSpan.CreateAddDestination(0, dcount));
                    return retval;
                }
            }

            matchList.Sort();
            var curDest = 0;
            var curSource = 0;
            DiffResultSpan last = null;

            //Process each match record
            foreach (DiffResultSpan drs in matchList)
            {
                if (!AddChanges(retval, curDest, drs.DestIndex, curSource, drs.SourceIndex) && (last != null))
                {
                    last.AddLength(drs.Length);
                }
                else
                {
                    retval.Add(drs);
                }
                curDest = drs.DestIndex + drs.Length;
                curSource = drs.SourceIndex + drs.Length;
                last = drs;
            }

            //Process any tail end data
            AddChanges(retval, curDest, dcount, curSource, scount);

            return retval;
        }

        public int GetSourceMatchLength(int destIndex, int sourceIndex, int maxLength)
        {
            int matchCount;

            for (matchCount = 0; matchCount < maxLength; matchCount++)
            {
                if (dest.GetByIndex(destIndex + matchCount).CompareTo(source.GetByIndex(sourceIndex + matchCount)) != 0)
                {
                    break;
                }
            }
            return matchCount;
        }

        public double ProcessDiff(IDiffList source, IDiffList destination, DiffEngineLevel level)
        {
            this.level = level;
            return ProcessDiff(source, destination);
        }

        public double ProcessDiff(IDiffList source, IDiffList destination)
        {
            var dt = DateTime.Now;
            this.source = source;
            dest = destination;
            matchList = new ArrayList();

            var dcount = dest.Count();
            var scount = this.source.Count();

            if ((dcount > 0) && (scount > 0))
            {
                stateList = new DiffStateList(dcount);
                ProcessRange(0, dcount - 1, 0, scount - 1);
            }

            var ts = DateTime.Now - dt;
            return ts.TotalSeconds;
        }

        private bool AddChanges(ArrayList report, int curDest, int nextDest, int curSource, int nextSource)
        {
            var retval = false;
            var diffDest = nextDest - curDest;
            var diffSource = nextSource - curSource;
            var minDiff = 0;
            if (diffDest > 0)
            {
                if (diffSource > 0)
                {
                    minDiff = Math.Min(diffDest, diffSource);
                    report.Add(DiffResultSpan.CreateReplace(curDest, curSource, minDiff));
                    if (diffDest > diffSource)
                    {
                        curDest += minDiff;
                        report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest - diffSource));
                    }
                    else
                    {
                        if (diffSource > diffDest)
                        {
                            curSource += minDiff;
                            report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource - diffDest));
                        }
                    }
                }
                else
                {
                    report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest));
                }
                retval = true;
            }
            else
            {
                if (diffSource > 0)
                {
                    report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource));
                    retval = true;
                }
            }
            return retval;
        }

        private void GetLongestSourceMatch(DiffState curItem, int destIndex, int destEnd, int sourceStart, int sourceEnd)
        {
            var maxDestLength = destEnd - destIndex + 1;
            var curLength = 0;
            var curBestLength = 0;
            var curBestIndex = -1;
            var maxLength = 0;
            for (var sourceIndex = sourceStart; sourceIndex <= sourceEnd; sourceIndex++)
            {
                maxLength = Math.Min(maxDestLength, sourceEnd - sourceIndex + 1);
                if (maxLength <= curBestLength)
                {
                    //No chance to find a longer one any more
                    break;
                }
                curLength = GetSourceMatchLength(destIndex, sourceIndex, maxLength);
                if (curLength > curBestLength)
                {
                    //This is the best match so far
                    curBestIndex = sourceIndex;
                    curBestLength = curLength;
                }

                //jump over the match
                sourceIndex += curBestLength;
            }

            //DiffState cur = _stateList.GetByIndex(destIndex);
            if (curBestIndex == -1)
            {
                curItem.SetNoMatch();
            }
            else
            {
                curItem.SetMatch(curBestIndex, curBestLength);
            }
        }

        private void ProcessRange(int destStart, int destEnd, int sourceStart, int sourceEnd)
        {
            var curBestIndex = -1;
            var curBestLength = -1;
            var maxPossibleDestLength = 0;
            DiffState curItem = null;
            DiffState bestItem = null;
            for (var destIndex = destStart; destIndex <= destEnd; destIndex++)
            {
                maxPossibleDestLength = destEnd - destIndex + 1;
                if (maxPossibleDestLength <= curBestLength)
                {
                    //we won't find a longer one even if we looked
                    break;
                }
                curItem = stateList.GetByIndex(destIndex);

                if (!curItem.HasValidLength(sourceStart, sourceEnd, maxPossibleDestLength))
                {
                    //recalc new best length since it isn't valid or has never been done.
                    GetLongestSourceMatch(curItem, destIndex, destEnd, sourceStart, sourceEnd);
                }
                if (curItem.Status == DiffStatus.Matched)
                {
                    switch (level)
                    {
                        case DiffEngineLevel.FastImperfect:
                            if (curItem.Length > curBestLength)
                            {
                                //this is longest match so far
                                curBestIndex = destIndex;
                                curBestLength = curItem.Length;
                                bestItem = curItem;
                            }

                            //Jump over the match 
                            destIndex += curItem.Length - 1;
                            break;
                        case DiffEngineLevel.Medium:
                            if (curItem.Length > curBestLength)
                            {
                                //this is longest match so far
                                curBestIndex = destIndex;
                                curBestLength = curItem.Length;
                                bestItem = curItem;

                                //Jump over the match 
                                destIndex += curItem.Length - 1;
                            }
                            break;
                        default:
                            if (curItem.Length > curBestLength)
                            {
                                //this is longest match so far
                                curBestIndex = destIndex;
                                curBestLength = curItem.Length;
                                bestItem = curItem;
                            }
                            break;
                    }
                }
            }
            if (curBestIndex < 0)
            {
                //we are done - there are no matches in this span
            }
            else
            {
                var sourceIndex = bestItem.StartIndex;
                matchList.Add(DiffResultSpan.CreateNoChange(curBestIndex, sourceIndex, curBestLength));
                if (destStart < curBestIndex)
                {
                    //Still have more lower destination data
                    if (sourceStart < sourceIndex)
                    {
                        //Still have more lower source data
                        // Recursive call to process lower indexes
                        ProcessRange(destStart, curBestIndex - 1, sourceStart, sourceIndex - 1);
                    }
                }
                var upperDestStart = curBestIndex + curBestLength;
                var upperSourceStart = sourceIndex + curBestLength;
                if (destEnd > upperDestStart)
                {
                    //we still have more upper dest data
                    if (sourceEnd > upperSourceStart)
                    {
                        //set still have more upper source data
                        // Recursive call to process upper indexes
                        ProcessRange(upperDestStart, destEnd, upperSourceStart, sourceEnd);
                    }
                }
            }
        }
    }
}
