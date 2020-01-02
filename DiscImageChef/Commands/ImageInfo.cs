// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ImageInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'image-info' verb.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    internal class ImageInfoCommand : Command
    {
        public ImageInfoCommand() : base("image-info",
                                         "Opens a media image and shows information about the media it represents and metadata.")
        {
            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Media image path", Name = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        static int Invoke(bool debug, bool verbose, string imagePath)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("image-info");

            DicConsole.DebugWriteLine("Analyze command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Analyze command", "--input={0}", imagePath);
            DicConsole.DebugWriteLine("Analyze command", "--verbose={0}", verbose);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");

                return(int)ErrorNumber.CannotOpenFile;
            }

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    DicConsole.WriteLine("Image format not identified.");

                    return(int)ErrorNumber.UnrecognizedFormat;
                }

                DicConsole.WriteLine("Image format identified by {0} ({1}).", imageFormat.Name, imageFormat.Id);
                DicConsole.WriteLine();

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");

                        return(int)ErrorNumber.CannotOpenFormat;
                    }

                    ImageInfo.PrintImageInfo(imageFormat);

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    DicConsole.DebugWriteLine("Image-info command", "Stack trace: {0}", ex.StackTrace);

                    return(int)ErrorNumber.CannotOpenFormat;
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                DicConsole.DebugWriteLine("Image-info command", ex.StackTrace);

                return(int)ErrorNumber.UnexpectedException;
            }

            return(int)ErrorNumber.NoError;
        }
    }
}