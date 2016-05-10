// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Text.Diff.Structures
{
    // #define USE_HASH_TABLE

    internal class DiffStateList
    {
#if USE_HASH_TABLE
    Hashtable _table;
#else

        private readonly DiffState[] array;
#endif

        public DiffStateList(int destCount)
        {
#if USE_HASH_TABLE
      _table = new Hashtable(Math.Max(9, destCount/10));
#else
            array = new DiffState[destCount];
#endif
        }

        [NotNull]
        public DiffState GetByIndex(int index)
        {
#if USE_HASH_TABLE
      DiffState retval = (DiffState) _table[index];
      if (retval == null) {
        retval = new DiffState();
        _table.Add(index, retval);
      }
#else
            var retval = array[index];
            if (retval == null)
            {
                retval = new DiffState();
                array[index] = retval;
            }
#endif
            return retval;
        }
    }
}
