// /***************************************************************************
// The Disc Image Chef
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
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License auint with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Extents
{
    public class ExtentsUInt
    {
        List<Tuple<uint, uint>> backend;

        public ExtentsUInt()
        {
            backend = new List<Tuple<uint, uint>>();
        }

        public ExtentsUInt(List<Tuple<uint, uint>> list)
        {
            backend = list.OrderBy(t => t.Item1).ToList();
        }

        public int Count { get { return backend.Count; } }

        public void Add(uint item)
        {
            Tuple<uint, uint> removeOne = null;
            Tuple<uint, uint> removeTwo = null;
            Tuple<uint, uint> itemToAdd = null;

            for(int i = 0; i < backend.Count; i++)
            {
                // Already contained in an extent
                if(item >= backend[i].Item1 && item <= backend[i].Item2)
                    return;

                // Expands existing extent start
                if(item == backend[i].Item1 - 1)
                {
                    removeOne = backend[i];

                    if(i > 0 && item == backend[i - 1].Item2 + 1)
                    {
                        removeTwo = backend[i - 1];
                        itemToAdd = new Tuple<uint, uint>(backend[i - 1].Item1, backend[i].Item2);
                    }
                    else
                        itemToAdd = new Tuple<uint, uint>(item, backend[i].Item2);

                    break;
                }

                // Expands existing extent end
                if(item == backend[i].Item2 + 1)
                {
                    removeOne = backend[i];

                    if(i < backend.Count - 1 && item == backend[i + 1].Item1 - 1)
                    {
                        removeTwo = backend[i + 1];
                        itemToAdd = new Tuple<uint, uint>(backend[i].Item1, backend[i + 1].Item2);
                    }
                    else
                        itemToAdd = new Tuple<uint, uint>(backend[i].Item1, item);

                    break;
                }
            }

            if(itemToAdd != null)
            {
                backend.Remove(removeOne);
                backend.Remove(removeTwo);
                backend.Add(itemToAdd);
            }
            else
                backend.Add(new Tuple<uint, uint>(item, item));

            // Sort
            backend = backend.OrderBy(t => t.Item1).ToList();
        }

        public void Add(uint start, uint end)
        {
            Add(start, end, false);
        }

        public void Add(uint start, uint end, bool run)
        {
            uint realEnd;
            if(run)
                realEnd = start + end - 1;
            else
                realEnd = end;

            // TODO: Optimize this
            for(uint t = start; t <= realEnd; t++)
                Add(t);
        }

        public bool Contains(uint item)
        {
            foreach(Tuple<uint, uint> extent in backend)
                if(item >= extent.Item1 && item <= extent.Item2)
                    return true;
            return false;
        }

        public void Clear()
        {
            backend.Clear();
        }

        public bool Remove(uint item)
        {
            Tuple<uint, uint> toRemove = null;
            Tuple<uint, uint> toAddOne = null;
            Tuple<uint, uint> toAddTwo = null;

            foreach(Tuple<uint, uint> extent in backend)
            {
                // Extent is contained and not a border
                if(item > extent.Item1 && item < extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<uint, uint>(extent.Item1, item - 1);
                    toAddTwo = new Tuple<uint, uint>(item + 1, extent.Item2);
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
                if(item == extent.Item1 && item == extent.Item2)
                {
                    toRemove = extent;
                    break;
                }
            }

            // Item not found
            if(toRemove == null)
                return false;

            backend.Remove(toRemove);
            if(toAddOne != null)
                backend.Add(toAddOne);
            if(toAddTwo != null)
                backend.Add(toAddTwo);

            // Sort
            backend = backend.OrderBy(t => t.Item1).ToList();

            return true;
        }

        public Tuple<uint, uint>[] ToArray()
        {
            return backend.ToArray();
        }

        public bool GetStart(uint item, out uint start)
        {
            start = 0;
            foreach(Tuple<uint, uint> extent in backend)
            {
                if(item >= extent.Item1 && item <= extent.Item2)
                {
                    start = extent.Item1;
                    return true;
                }
            }
            return false;
        }
    }
}
