// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlockMap.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI custom controls.
//
// --[ Description ] ----------------------------------------------------------
//
//     Draws a block map.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Aaru.Gui.Controls
{
    public class BlockMap : ColoredGrid
    {
        ulong       _sectors;
        public uint blocksToRead;

        public uint sectorsToRead;

        public BlockMap()
        {
            ColoredSectors                   =  new ObservableCollection<ColoredBlock>();
            ColoredSectors.CollectionChanged += OnColoredSectorsChanged;
        }

        public ulong Sectors
        {
            get => _sectors;
            set
            {
                _sectors = value;
                CalculateBoundaries();
                Invalidate();
            }
        }

        public ulong SectorsPerBlock { get; private set; }

        public uint SectorsToRead
        {
            get => sectorsToRead;
            set
            {
                sectorsToRead = value;
                blocksToRead  = (uint)(sectorsToRead / SectorsPerBlock);

                if(sectorsToRead % SectorsPerBlock > 0)
                    blocksToRead++;
            }
        }

        public ObservableCollection<ColoredBlock> ColoredSectors { get; }

        void OnColoredSectorsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch(args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(object item in args.NewItems)
                    {
                        if(!(item is ColoredBlock block))
                            continue;

                        for(ulong i = 0; i < blocksToRead; i++)
                            ColoredBlocks.Add(new ColoredBlock((block.Block / SectorsPerBlock) + i, block.Color));
                    }

                    break;
                case NotifyCollectionChangedAction.Move: break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(object item in args.OldItems)
                    {
                        if(!(item is ColoredBlock block))
                            continue;

                        for(ulong i = 0; i < blocksToRead; i++)
                            ColoredBlocks.Remove(ColoredBlocks.FirstOrDefault(t => t.Block == (block.Block /
                                                                                               SectorsPerBlock) + i));
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach(object item in args.OldItems)
                    {
                        if(!(item is ColoredBlock block))
                            continue;

                        for(ulong i = 0; i < blocksToRead; i++)
                            ColoredBlocks.Remove(ColoredBlocks.FirstOrDefault(t => t.Block == (block.Block /
                                                                                               SectorsPerBlock) + i));
                    }

                    foreach(object item in args.NewItems)
                    {
                        if(!(item is ColoredBlock block))
                            continue;

                        for(ulong i = 0; i < blocksToRead; i++)
                            ColoredBlocks.Add(new ColoredBlock((block.Block / SectorsPerBlock) + i, block.Color));
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    ColoredBlocks.Clear();

                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        void CalculateBoundaries()
        {
            SectorsPerBlock = Blocks == 0 ? 0 : _sectors / Blocks;
            ColoredBlocks.Clear();

            if(SectorsPerBlock > 0)
                return;

            SectorsPerBlock = 1;

            for(ulong i = Sectors; i < Blocks; i++)
                ColoredBlocks.Add(new ColoredBlock(i, GridColor));
        }

        public void Clear() => ColoredSectors.Clear();
    }
}