// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Joliet.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Joliet extensions structures.
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
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    public sealed partial class ISO9660
    {
        static DecodedVolumeDescriptor DecodeJolietDescriptor(PrimaryVolumeDescriptor jolietvd)
        {
            var decodedVd = new DecodedVolumeDescriptor
            {
                SystemIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.system_id).Replace('\u0000', ' ').
                                            TrimEnd(),
                VolumeIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.volume_id).Replace('\u0000', ' ').
                                            TrimEnd(),
                VolumeSetIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.volume_set_id).
                                               Replace('\u0000', ' ').TrimEnd(),
                PublisherIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.publisher_id).Replace('\u0000', ' ').
                                               TrimEnd(),
                DataPreparerIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.preparer_id).
                                                  Replace('\u0000', ' ').TrimEnd(),
                ApplicationIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.application_id).
                                                 Replace('\u0000', ' ').TrimEnd()
            };

            if(jolietvd.creation_date[0] < 0x31 ||
               jolietvd.creation_date[0] > 0x39)
                decodedVd.CreationTime = DateTime.MinValue;
            else
                decodedVd.CreationTime = DateHandlers.Iso9660ToDateTime(jolietvd.creation_date);

            if(jolietvd.modification_date[0] < 0x31 ||
               jolietvd.modification_date[0] > 0x39)
                decodedVd.HasModificationTime = false;
            else
            {
                decodedVd.HasModificationTime = true;
                decodedVd.ModificationTime    = DateHandlers.Iso9660ToDateTime(jolietvd.modification_date);
            }

            if(jolietvd.expiration_date[0] < 0x31 ||
               jolietvd.expiration_date[0] > 0x39)
                decodedVd.HasExpirationTime = false;
            else
            {
                decodedVd.HasExpirationTime = true;
                decodedVd.ExpirationTime    = DateHandlers.Iso9660ToDateTime(jolietvd.expiration_date);
            }

            if(jolietvd.effective_date[0] < 0x31 ||
               jolietvd.effective_date[0] > 0x39)
                decodedVd.HasEffectiveTime = false;
            else
            {
                decodedVd.HasEffectiveTime = true;
                decodedVd.EffectiveTime    = DateHandlers.Iso9660ToDateTime(jolietvd.effective_date);
            }

            decodedVd.Blocks    = jolietvd.volume_space_size;
            decodedVd.BlockSize = jolietvd.logical_block_size;

            return decodedVd;
        }
    }
}