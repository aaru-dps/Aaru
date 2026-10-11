// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmGraph.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'dpm-graph' command.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Core.Devices.Dpm;
using Aaru.Localization;
using Aaru.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aaru.Commands.Image;

sealed class DpmGraphCommand : Command<DpmGraphCommand.Settings>
{
    const string MODULE_NAME = "DPM graph command";

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("dpm-graph");

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",   settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--input={0}",   Markup.Escape(settings.ImagePath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}", settings.Verbose);

        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = PluginRegister.Singleton.GetFilter(settings.ImagePath);
        });

        if(inputFilter == null)
        {
            AaruLogging.Error(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        IBaseImage inputFormat = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
            inputFormat = ImageFormat.Detect(inputFilter);
        });

        if(inputFormat == null)
        {
            AaruLogging.Error(UI.Image_format_not_identified);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        ErrorNumber opened = ErrorNumber.NoData;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
            opened = inputFormat.Open(inputFilter);
        });

        if(opened != ErrorNumber.NoError)
        {
            AaruLogging.Error(UI.Unable_to_open_image_format);
            AaruLogging.Error(Localization.Core.Error_0, opened);

            return (int)opened;
        }

        if(inputFormat is not IOpticalMediaImage opticalFormat ||
           opticalFormat.ReadDpm(out DataPositionMeasurement dpm) != ErrorNumber.NoError)
        {
            AaruLogging.WriteLine(UI.Image_has_no_DPM);

            return (int)ErrorNumber.NoData;
        }

        string prefix = Path.Combine(Path.GetDirectoryName(settings.ImagePath) ?? "",
                                     Path.GetFileNameWithoutExtension(settings.ImagePath));

        if(!DpmSidecar.Write(dpm, prefix))
        {
            AaruLogging.Error(UI.Error_writing_DPM_graph);

            return (int)ErrorNumber.WriteError;
        }

        AaruLogging.WriteLine(UI.Written_DPM_graph_to_0, Markup.Escape(prefix + ".dpm.png"));

        return (int)ErrorNumber.NoError;
    }

#region Nested type: Settings

    public class Settings : ImageFamily
    {
        [LocalizedDescription(nameof(UI.Media_image_path))]
        [CommandArgument(0, "<image-path>")]
        public string ImagePath { get; init; }
    }

#endregion
}