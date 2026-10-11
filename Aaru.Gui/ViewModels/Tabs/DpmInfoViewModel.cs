// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI tabs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Data Position Measurement information tab.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Image;
using Aaru.Gui.Models;
using Aaru.Localization;

namespace Aaru.Gui.ViewModels.Tabs;

public sealed class DpmInfoViewModel : ViewModelBase
{
    public DpmInfoViewModel() => Summary = [];

    public DpmInfoViewModel(DataPositionMeasurement dpm, DpmAnalysisResult analysis)
    {
        Dpm      = dpm;
        Analysis = analysis;
        Summary  = new ObservableCollection<DpmSummaryModel>(BuildSummary(analysis));
    }

    public DataPositionMeasurement               Dpm      { get; }
    public DpmAnalysisResult                     Analysis { get; }
    public ObservableCollection<DpmSummaryModel> Summary  { get; }

    /// <summary>Builds the rows that summarize a DPM analysis</summary>
    internal static IEnumerable<DpmSummaryModel> BuildSummary(DpmAnalysisResult analysis)
    {
        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_medium,
            Value = analysis.TimingUnit switch
                    {
                        1  => "CD",
                        16 => "DVD",
                        32 => "BD",
                        _  => UI.DPM_medium_unknown
                    }
        };

        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_speed,
            Value = analysis.Speed switch
                    {
                        0      => UI.DPM_speed_unknown,
                        0xFFFF => UI.DPM_speed_maximum,
                        _      => string.Format(UI.DPM_speed_0, analysis.Speed)
                    }
        };

        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_layers,
            Value = analysis.Layers.Length.ToString()
        };

        if(analysis.Layers.Length > 1)
        {
            yield return new DpmSummaryModel
            {
                Label = UI.Title_DPM_track_path,
                Value = analysis.OppositeTrackPath ? UI.DPM_track_path_opposite : UI.DPM_track_path_parallel
            };
        }

        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_entries,
            Value = string.Format(UI.DPM_0_entries_every_1_sectors, analysis.Entries, analysis.NominalSpacing)
        };

        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_sectors,
            Value = string.Format(UI.DPM_0_to_1, analysis.FirstLba, analysis.LastLba)
        };

        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_turns,
            Value = analysis.TotalTurns.ToString("F2")
        };

        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_calibrations,
            Value = analysis.Calibrations.Length.ToString()
        };

        yield return new DpmSummaryModel
        {
            Label = UI.Title_DPM_bands,
            Value = analysis.Bands.Length.ToString()
        };

        foreach(DpmEntryStatus status in Enum.GetValues<DpmEntryStatus>())
        {
            int count = analysis.StatusCounts[(int)status];

            if(count == 0) continue;

            yield return new DpmSummaryModel
            {
                Label = StatusName(status),
                Value = string.Format(UI.DPM_0_bins_1, count, (double)count / analysis.Bins.Length)
            };
        }
    }

    /// <summary>Gets the name of a bin status</summary>
    internal static string StatusName(DpmEntryStatus status)
    {
        return status switch
               {
                   DpmEntryStatus.Measured     => UI.DPM_status_measured,
                   DpmEntryStatus.Remeasured   => UI.DPM_status_remeasured,
                   DpmEntryStatus.Interpolated => UI.DPM_status_interpolated,
                   DpmEntryStatus.Unreadable   => UI.DPM_status_unreadable,
                   DpmEntryStatus.LayerBreak   => UI.DPM_status_layer_break,
                   _                           => UI.DPM_status_unknown
               };
    }
}