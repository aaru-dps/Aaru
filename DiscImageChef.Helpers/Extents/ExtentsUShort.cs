// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ExtentsUShort.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Extent helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides extents for ushort types.
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
//     License aushort with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Extents
{
    public class ExtentsUShort
    {
        List<Tuple<ushort, ushort>> backend;

        public ExtentsUShort()
        {
            backend = new List<Tuple<ushort, ushort>>();
        }

        public ExtentsUShort(List<Tuple<ushort, ushort>> list)
        {
            backend = list.OrderBy(t => t.Item1).ToList();
        }

        public int Count { get { return backend.Count; } }

        public void Add(ushort item)
        {
            Tuple<ushort, ushort> removeOne = null;
            Tuple<ushort, ushort> removeTwo = null;
            Tuple<ushort, ushort> itemToAdd = null;

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
                        itemToAdd = new Tuple<ushort, ushort>(backend[i - 1].Item1, backend[i].Item2);
                    }
                    else
                        itemToAdd = new Tuple<ushort, ushort>(item, backend[i].Item2);

                    break;
                }

                // Expands existing extent end
                if(item == backend[i].Item2 + 1)
                {
                    removeOne = backend[i];

                    if(i < backend.Count - 1 && item == backend[i + 1].Item1 - 1)
                    {
                        removeTwo = backend[i + 1];
                        itemToAdd = new Tuple<ushort, ushort>(backend[i].Item1, backend[i + 1].Item2);
                    }
                    else
                        itemToAdd = new Tuple<ushort, ushort>(backend[i].Item1, item);

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
                backend.Add(new Tuple<ushort, ushort>(item, item));

            // Sort
            backend = backend.OrderBy(t => t.Item1).ToList();
        }

        public void Add(ushort start, ushort end)
        {
            Add(start, end, false);
        }

        public void Add(ushort start, ushort end, bool run)
        {
            ushort realEnd;
            if(run)
                realEnd = (ushort)(start + end - 1);
            else
                realEnd = end;

            // TODO: Optimize this
            for(ushort t = start; t <= realEnd; t++)
                Add(t);
        }

        public bool Contains(ushort item)
        {
            foreach(Tuple<ushort, ushort> extent in backend)
                if(item >= extent.Item1 && item <= extent.Item2)
                    return true;
            return false;
        }

        public void Clear()
        {
            backend.Clear();
        }

        public bool Remove(ushort item)
        {
            Tuple<ushort, ushort> toRemove = null;
            Tuple<ushort, ushort> toAddOne = null;
            Tuple<ushort, ushort> toAddTwo = null;

            foreach(Tuple<ushort, ushort> extent in backend)
            {
                // Extent is contained and not a border
                if(item > extent.Item1 && item < extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<ushort, ushort>(extent.Item1, (ushort)(item - 1));
                    toAddTwo = new Tuple<ushort, ushort>((ushort)(item + 1), extent.Item2);
                    break;
                }

                // Extent is left border, but not only element
                if(item == extent.Item1 && item != extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<ushort, ushort>((ushort)(item + 1), extent.Item2);
                    break;
                }

                // Extent is right border, but not only element
                if(item != extent.Item1 && item == extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<ushort, ushort>(extent.Item1, (ushort)(item - 1));
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

        public Tuple<ushort, ushort>[] ToArray()
        {
            return backend.ToArray();
        }

        public bool GetStart(ushort item, out ushort start)
        {
            start = 0;
            foreach(Tuple<ushort, ushort> extent in backend)
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
