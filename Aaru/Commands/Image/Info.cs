// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'info' command.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands.Image;

sealed class ImageInfoCommand : Command
{
    const string MODULE_NAME = "Image-info command";

    public ImageInfoCommand() : base("info", UI.Image_Info_Command_Description)
    {
        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Media_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, string imagePath)
    {
        MainClass.PrintCopyright();

        if(debug)
        {
            IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(System.Console.Error)
            });

            AaruConsole.DebugWriteLineEvent += (format, objects) =>
            {
                if(objects is null)
                    stderrConsole.MarkupLine(format);
                else
                    stderrConsole.MarkupLine(format, objects);
            };
        }

        if(verbose)
        {
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };
        }

        Statistics.AddCommand("image-info");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",   imagePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}", verbose);

        var     filtersList = new FiltersList();
        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = filtersList.GetFilter(imagePath);
        });

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        try
        {
            IBaseImage imageFormat = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
                imageFormat = ImageFormat.Detect(inputFilter);
            });

            if(imageFormat == null)
            {
                AaruConsole.WriteLine(UI.Image_format_not_identified);

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            AaruConsole.WriteLine(UI.Image_format_identified_by_0_1, imageFormat.Name, imageFormat.Id);
            AaruConsole.WriteLine();

            try
            {
                ErrorNumber opened = ErrorNumber.NoData;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
                    opened = imageFormat.Open(inputFilter);
                });

                if(opened != ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine(UI.Unable_to_open_image_format);
                    AaruConsole.WriteLine(Localization.Core.Error_0, opened);

                    return (int)opened;
                }

                ImageInfo.PrintImageInfo(imageFormat);

                Statistics.AddMediaFormat(imageFormat.Format);
                Statistics.AddMedia(imageFormat.Info.MediaType, false);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine(UI.Unable_to_open_image_format);
                AaruConsole.ErrorWriteLine(Localization.Core.Error_0, ex.Message);
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Stack_trace_0, ex.StackTrace);

                return (int)ErrorNumber.CannotOpenFormat;
            }
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(string.Format(UI.Error_reading_file_0, ex.Message));
            AaruConsole.DebugWriteLine(MODULE_NAME, ex.StackTrace);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }
}