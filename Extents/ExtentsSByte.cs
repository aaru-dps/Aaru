// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ExtentsSByte.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Extent helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides extents for sbyte types.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aaru.CommonTypes.Extents
{
    /// <summary>Implements extents for <see cref="sbyte" /></summary>
    public class ExtentsSByte
    {
        List<Tuple<sbyte, sbyte>> _backend;

        /// <summary>Initialize an empty list of extents</summary>
        public ExtentsSByte() => _backend = new List<Tuple<sbyte, sbyte>>();

        /// <summary>Initializes extents with an specific list</summary>
        /// <param name="list">List of extents as tuples "start, end"</param>
        public ExtentsSByte(IEnumerable<Tuple<sbyte, sbyte>> list)
        {
            _backend = new List<Tuple<sbyte, sbyte>>();

            // This ensure no overlapping extents are added on creation
            foreach(Tuple<sbyte, sbyte> t in list)
                Add(t.Item1, t.Item2);
        }

        /// <summary>Gets a count of how many extents are stored</summary>
        public int Count => _backend.Count;

        /// <summary>Adds the specified number to the corresponding extent, or creates a new one</summary>
        /// <param name="item"></param>
        public void Add(sbyte item)
        {
            Tuple<sbyte, sbyte> removeOne = null;
            Tuple<sbyte, sbyte> removeTwo = null;
            Tuple<sbyte, sbyte> itemToAdd = null;

            for(int i = 0; i < _backend.Count; i++)
            {
                // Already contained in an extent
                if(item >= _backend[i].Item1 &&
                   item <= _backend[i].Item2)
                    return;

                // Expands existing extent start
                if(item == _backend[i].Item1 - 1)
                {
                    removeOne = _backend[i];

                    if(i    > 0 &&
                       item == _backend[i - 1].Item2 + 1)
                    {
                        removeTwo = _backend[i - 1];
                        itemToAdd = new Tuple<sbyte, sbyte>(_backend[i - 1].Item1, _backend[i].Item2);
                    }
                    else
                        itemToAdd = new Tuple<sbyte, sbyte>(item, _backend[i].Item2);

                    break;
                }

                // Expands existing extent end
                if(item != _backend[i].Item2 + 1)
                    continue;

                removeOne = _backend[i];

                if(i    < _backend.Count         - 1 &&
                   item == _backend[i + 1].Item1 - 1)
                {
                    removeTwo = _backend[i + 1];
                    itemToAdd = new Tuple<sbyte, sbyte>(_backend[i].Item1, _backend[i + 1].Item2);
                }
                else
                    itemToAdd = new Tuple<sbyte, sbyte>(_backend[i].Item1, item);

                break;
            }

            if(itemToAdd != null)
            {
                _backend.Remove(removeOne);
                _backend.Remove(removeTwo);
                _backend.Add(itemToAdd);
            }
            else
                _backend.Add(new Tuple<sbyte, sbyte>(item, item));

            // Sort
            _backend = _backend.OrderBy(t => t.Item1).ToList();
        }

        /// <summary>Adds a new extent</summary>
        /// <param name="start">First element of the extent</param>
        /// <param name="end">
        ///     Last element of the extent or if <see cref="run" /> is <c>true</c> how many elements the extent runs
        ///     for
        /// </param>
        /// <param name="run">If set to <c>true</c>, <see cref="end" /> indicates how many elements the extent runs for</param>
        public void Add(sbyte start, sbyte end, bool run = false)
        {
            sbyte realEnd;

            if(run)
                realEnd = (sbyte)(start + end - 1);
            else
                realEnd = end;

            // TODO: Optimize this
            for(sbyte t = start; t <= realEnd; t++)
                Add(t);
        }

        /// <summary>Checks if the specified item is contained by an extent on this instance</summary>
        /// <param name="item">Item to search for</param>
        /// <returns><c>true</c> if any of the extents on this instance contains the item</returns>
        public bool Contains(sbyte item) => _backend.Any(extent => item >= extent.Item1 && item <= extent.Item2);

        /// <summary>Removes all extents from this instance</summary>
        public void Clear() => _backend.Clear();

        /// <summary>Removes an item from the extents in this instance</summary>
        /// <param name="item">Item to remove</param>
        /// <returns><c>true</c> if the item was contained in a known extent and removed, false otherwise</returns>
        public bool Remove(sbyte item)
        {
            Tuple<sbyte, sbyte> toRemove = null;
            Tuple<sbyte, sbyte> toAddOne = null;
            Tuple<sbyte, sbyte> toAddTwo = null;

            foreach(Tuple<sbyte, sbyte> extent in _backend)
            {
                // Extent is contained and not a border
                if(item > extent.Item1 &&
                   item < extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<sbyte, sbyte>(extent.Item1, (sbyte)(item - 1));
                    toAddTwo = new Tuple<sbyte, sbyte>((sbyte)(item               + 1), extent.Item2);

                    break;
                }

                // Extent is left border, but not only element
                if(item == extent.Item1 &&
                   item != extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<sbyte, sbyte>((sbyte)(item + 1), extent.Item2);

                    break;
                }

                // Extent is right border, but not only element
                if(item != extent.Item1 &&
                   item == extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<sbyte, sbyte>(extent.Item1, (sbyte)(item - 1));

                    break;
                }

                // Extent is only element
                if(item != extent.Item1 ||
                   item != extent.Item2)
                    continue;

                toRemove = extent;

                break;
            }

            // Item not found
            if(toRemove == null)
                return false;

            _backend.Remove(toRemove);

            if(toAddOne != null)
                _backend.Add(toAddOne);

            if(toAddTwo != null)
                _backend.Add(toAddTwo);

            // Sort
            _backend = _backend.OrderBy(t => t.Item1).ToList();

            return true;
        }

        /// <summary>
        ///     Converts the list of extents to an array of <see cref="Tuple" /> where T1 is first element of the extent and
        ///     T2 is last element
        /// </summary>
        /// <returns>Array of <see cref="Tuple" /></returns>
        public Tuple<sbyte, sbyte>[] ToArray() => _backend.ToArray();

        /// <summary>Gets the first element of the extent that contains the specified item</summary>
        /// <param name="item">Item</param>
        /// <param name="start">First element of extent</param>
        /// <returns><c>true</c> if item was found in an extent, false otherwise</returns>
        public bool GetStart(sbyte item, out sbyte start)
        {
            start = 0;

            foreach(Tuple<sbyte, sbyte> extent in
                _backend.Where(extent => item >= extent.Item1 && item <= extent.Item2))
            {
                start = extent.Item1;

                return true;
            }

            return false;
        }
    }
}