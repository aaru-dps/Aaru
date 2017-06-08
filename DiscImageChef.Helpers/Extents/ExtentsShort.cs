// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ExtentsShort.cs
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
//     License ashort with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extents
{
    public class ExtentsShort
    {
        List<Tuple<short, short>> backend;

        public ExtentsShort()
        {
            backend = new List<Tuple<short, short>>();
        }

        public int Count { get { return backend.Count; } }

        public void Add(short item)
        {
            Tuple<short, short> removeOne = null;
            Tuple<short, short> removeTwo = null;
            Tuple<short, short> itemToAdd = null;

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
                        itemToAdd = new Tuple<short, short>(backend[i - 1].Item1, backend[i].Item2);
                    }
                    else
                        itemToAdd = new Tuple<short, short>(item, backend[i].Item2);

                    break;
                }

                // Expands existing extent end
                if(item == backend[i].Item2 + 1)
                {
                    removeOne = backend[i];

                    if(i < backend.Count - 1 && item == backend[i + 1].Item1 - 1)
                    {
                        removeTwo = backend[i + 1];
                        itemToAdd = new Tuple<short, short>(backend[i].Item1, backend[i + 1].Item2);
                    }
                    else
                        itemToAdd = new Tuple<short, short>(backend[i].Item1, item);

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
                backend.Add(new Tuple<short, short>(item, item));

            // Sort
            backend = backend.OrderBy(t => t.Item1).ToList();
        }

        public void Add(short start, short end)
        {
            Add(start, end, false);
        }

        public void Add(short start, short end, bool run)
        {
            short realEnd;
            if(run)
                realEnd = (short)(start + end - 1);
            else
                realEnd = end;

            // TODO: Optimize this
            for(short t = start; t <= realEnd; t++)
                Add(t);
        }

        public bool Contains(short item)
        {
            foreach(Tuple<short, short> extent in backend)
                if(item >= extent.Item1 && item <= extent.Item2)
                    return true;
            return false;
        }

        public void Clear()
        {
            backend.Clear();
        }

        public bool Remove(short item)
        {
            Tuple<short, short> toRemove = null;
            Tuple<short, short> toAddOne = null;
            Tuple<short, short> toAddTwo = null;

            foreach(Tuple<short, short> extent in backend)
            {
                // Extent is contained and not a border
                if(item > extent.Item1 && item < extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<short, short>(extent.Item1, (short)(item - 1));
                    toAddTwo = new Tuple<short, short>((short)(item + 1), extent.Item2);
                    break;
                }

                // Extent is left border, but not only element
                if(item == extent.Item1 && item != extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<short, short>((short)(item + 1), extent.Item2);
                    break;
                }

                // Extent is right border, but not only element
                if(item != extent.Item1 && item == extent.Item2)
                {
                    toRemove = extent;
                    toAddOne = new Tuple<short, short>(extent.Item1, (short)(item - 1));
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

        public Tuple<short, short>[] ToArray()
        {
            return backend.ToArray();
        }
    }
}
