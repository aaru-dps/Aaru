// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ExtentsUInt.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Extent helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides extents for uint types.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Aaru.CommonTypes.Extents;

/// <summary>Implements extents for <see cref="uint" /></summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class ExtentsUInt
{
    List<Tuple<uint, uint>> _backend;

    /// <summary>Initialize an empty list of extents</summary>
    public ExtentsUInt() => _backend = [];

    /// <summary>Initializes extents with an specific list</summary>
    /// <param name="list">List of extents as tuples "start, end"</param>
    public ExtentsUInt(IEnumerable<Tuple<uint, uint>> list)
    {
        _backend = [];

        // This ensure no overlapping extents are added on creation
        foreach(Tuple<uint, uint> t in list) Add(t.Item1, t.Item2);
    }

    /// <summary>Gets a count of how many extents are stored</summary>
    public int Count => _backend.Count;

    /// <summary>Adds the specified number to the corresponding extent, or creates a new one</summary>
    /// <param name="item"></param>
    public void Add(uint item)
    {
        Tuple<uint, uint> removeOne = null;
        Tuple<uint, uint> removeTwo = null;
        Tuple<uint, uint> itemToAdd = null;

        for(var i = 0; i < _backend.Count; i++)
        {
            // Already contained in an extent
            if(item >= _backend[i].Item1 && item <= _backend[i].Item2) return;

            // Expands existing extent start
            if(item == _backend[i].Item1 - 1)
            {
                removeOne = _backend[i];

                if(i > 0 && item == _backend[i - 1].Item2 + 1)
                {
                    removeTwo = _backend[i - 1];
                    itemToAdd = new Tuple<uint, uint>(_backend[i - 1].Item1, _backend[i].Item2);
                }
                else
                    itemToAdd = new Tuple<uint, uint>(item, _backend[i].Item2);

                break;
            }

            // Expands existing extent end
            if(item != _backend[i].Item2 + 1) continue;

            removeOne = _backend[i];

            if(i < _backend.Count - 1 && item == _backend[i + 1].Item1 - 1)
            {
                removeTwo = _backend[i + 1];
                itemToAdd = new Tuple<uint, uint>(_backend[i].Item1, _backend[i + 1].Item2);
            }
            else
                itemToAdd = new Tuple<uint, uint>(_backend[i].Item1, item);

            break;
        }

        if(itemToAdd != null)
        {
            _backend.Remove(removeOne);
            _backend.Remove(removeTwo);
            _backend.Add(itemToAdd);
        }
        else
            _backend.Add(new Tuple<uint, uint>(item, item));

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
    public void Add(uint start, uint end, bool run = false)
    {
        uint realEnd;

        if(run)
            realEnd = start + end - 1;
        else
            realEnd = end;

        // TODO: Optimize this
        for(uint t = start; t <= realEnd; t++) Add(t);
    }

    /// <summary>Checks if the specified item is contained by an extent on this instance</summary>
    /// <param name="item">Item to search for</param>
    /// <returns><c>true</c> if any of the extents on this instance contains the item</returns>
    public bool Contains(uint item) => _backend.Any(extent => item >= extent.Item1 && item <= extent.Item2);

    /// <summary>Removes all extents from this instance</summary>
    public void Clear() => _backend.Clear();

    /// <summary>Removes an item from the extents in this instance</summary>
    /// <param name="item">Item to remove</param>
    /// <returns><c>true</c> if the item was contained in a known extent and removed, false otherwise</returns>
    public bool Remove(uint item)
    {
        Tuple<uint, uint> toRemove = null;
        Tuple<uint, uint> toAddOne = null;
        Tuple<uint, uint> toAddTwo = null;

        foreach(Tuple<uint, uint> extent in _backend)
        {
            // Extent is contained and not a border
            if(item > extent.Item1 && item < extent.Item2)
            {
                toRemove = extent;
                toAddOne = new Tuple<uint, uint>(extent.Item1, item - 1);
                toAddTwo = new Tuple<uint, uint>(item               + 1, extent.Item2);

                break;
            }

            // Extent is left border, but not only element
            if(item == extent.Item1 && item != extent.Item2)
            {
                toRemove = extent;
                toAddOne = new Tuple<uint, uint>(item + 1, extent.Item2);

                break;
            }

            // Extent is right border, but not only element
            if(item != extent.Item1 && item == extent.Item2)
            {
                toRemove = extent;
                toAddOne = new Tuple<uint, uint>(extent.Item1, item - 1);

                break;
            }

            // Extent is only element
            if(item != extent.Item1 || item != extent.Item2) continue;

            toRemove = extent;

            break;
        }

        // Item not found
        if(toRemove == null) return false;

        _backend.Remove(toRemove);

        if(toAddOne != null) _backend.Add(toAddOne);

        if(toAddTwo != null) _backend.Add(toAddTwo);

        // Sort
        _backend = _backend.OrderBy(t => t.Item1).ToList();

        return true;
    }

    /// <summary>
    ///     Converts the list of extents to an array of <see cref="Tuple" /> where T1 is first element of the extent and
    ///     T2 is last element
    /// </summary>
    /// <returns>Array of <see cref="Tuple" /></returns>
    public Tuple<uint, uint>[] ToArray() => _backend.ToArray();

    /// <summary>Gets the first element of the extent that contains the specified item</summary>
    /// <param name="item">Item</param>
    /// <param name="start">First element of extent</param>
    /// <returns><c>true</c> if item was found in an extent, false otherwise</returns>
    public bool GetStart(uint item, out uint start)
    {
        start = 0;

        foreach(Tuple<uint, uint> extent in _backend.Where(extent => item >= extent.Item1 && item <= extent.Item2))
        {
            start = extent.Item1;

            return true;
        }

        return false;
    }
}