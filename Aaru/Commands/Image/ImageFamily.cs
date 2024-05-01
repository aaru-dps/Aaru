// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageFamily.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'image' command family.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.CommandLine;
using Aaru.Localization;

namespace Aaru.Commands.Image;

sealed class ImageFamily : Command
{
    public ImageFamily() : base("image", UI.Image_Command_Family_Description)
    {
        AddAlias("i");

        AddCommand(new ChecksumCommand());
        AddCommand(new CompareCommand());
        AddCommand(new ConvertImageCommand());
        AddCommand(new CreateSidecarCommand());
        AddCommand(new DecodeCommand());
        AddCommand(new EntropyCommand());
        AddCommand(new ImageInfoCommand());
        AddCommand(new ListOptionsCommand());
        AddCommand(new PrintHexCommand());
        AddCommand(new VerifyCommand());
    }
}