// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ExtentsULong.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
//     License aulong with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extents
{
    public class ExtentsULong
    {
        List<Tuple<ulong, ulong>> backend;

        public ExtentsULong()
        {
            backend = new List<Tuple<ulong, ulong>>();
        }

        public ExtentsULong(List<Tuple<ulong, ulong>> list)
        {
            backend = list.OrderBy(t => t.Item1).ToList();
        }

        public int Count { get { return backend.Count; } }

        public void Add(ulong item)
        {
            Tuple<ulong, ulong> removeOne = null;
            Tuple<ulong, ulong> removeTwo = null;
            Tuple<ulong, ulong> itemToAdd = null;

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
                        itemToAdd = new Tuple<ulong, ulong>(backend[i - 1].Item1, backend[i].Item2);
                    }
                    else
                        itemToAdd = new Tuple<ulong, ulong>(item, backend[i].Item2);

                    break;
                }

                // Expands existing extent end
                if(item == backend[i].Item2 + 1)
                {
                    removeOne = backend[i];

                    if(i < backend.Count - 1 && item == backend[i + 1].Item1 - 1)
                    {
                        removeTwo = backend[i + 1];
                        itemToAdd = new Tuple<ulong, ulong>(backend[i].Item1, backend[i + 1].Item2);
                    }
                    else
                        itemToAdd = new Tuple<ulong, ulong>(backend[i].Item1, item);

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
                backend.Add(new Tuple<ulong, ulong>(item, item));

            // Sort
            backend = backend.OrderBy(t => t.Item1).ToList();
        }

        public void Add(ulong start, ulong end)
        {
            Add(start, end, false);
        }

        public void Add(ulong start, ulong end, bool run)
        {
            ulong realEnd;
            if(run)
                realEnd = start + end - 1;
            else
                realEnd = end;

            // TODO: Optimize this
            for(ulong t = start; t <= realEnd; t++)
                Add(t);
        }

        public bool Contains(ulong item)
        {
            foreach(Tuple<ulong, ulong> extent in backend)
                if(item >= extent.Item1 && item <= extent.Item2)
                    return true;
            return false;
        }

        public void Clear()
        {
            backend.Clear();
        }

        public bool Remove(ulong item)
        {
            Tuple<ulong, ulong> toRemove = null;
            Tuple<ulong, ulong> toAddOne = null;
            Tuple<ulong, ulong> toAddTwo = null;

            foreach(Tuple<ulong, ulong> extent in backend)
            {
                // Extent is contained and not a border
                if(item > extent.Item1 && item < extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<ulong, ulong>(extent.Item1, item - 1);
                    toAddTwo = new Tuple<ulong, ulong>(item + 1, extent.Item2);
                    break;
                }

                // Extent is left border, but not only element
                if(item == extent.Item1 && item != extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<ulong, ulong>(item + 1, extent.Item2);
                    break;
                }

                // Extent is right border, but not only element
                if(item != extent.Item1 && item == extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<ulong, ulong>(extent.Item1, item - 1);
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

        public Tuple<ulong, ulong>[] ToArray()
        {
            return backend.ToArray();
        }

        public bool GetStart(ulong item, out ulong start)
        {
            start = 0;
            foreach(Tuple<ulong, ulong> extent in backend)
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
