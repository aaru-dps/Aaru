using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Metadata;
using Aaru.Localization;
using Aaru.Logging;

namespace Aaru.Core.Image;

public sealed partial class Merger
{
    /// <summary>
    ///     Works out the resume information describing the merged image. A sector taken from the secondary image
    ///     carries the secondary's state, every other sector keeps the primary's state.
    /// </summary>
    /// <param name="primaryResume">Resume information of the primary image, or null if it has none.</param>
    /// <param name="secondaryResume">Resume information of the secondary image, or null if it has none.</param>
    /// <param name="primarySectors">Number of sectors in the primary image, which is the size of the output.</param>
    /// <param name="isTape">Whether the merged image is a tape.</param>
    /// <param name="sectorsCopiedFromSecondary">Sectors that were taken from the secondary image.</param>
    /// <param name="mergedTries">Dump hardware list written to the merged image.</param>
    internal static Resume CalculateMergedResume(Resume             primaryResume, Resume secondaryResume,
                                                 ulong              primarySectors, bool isTape,
                                                 List<ulong>        sectorsCopiedFromSecondary,
                                                 List<DumpHardware> mergedTries)
    {
        var copied = new HashSet<ulong>(sectorsCopiedFromSecondary);

        ulong nextBlock = Math.Max(primaryResume?.NextBlock ?? primarySectors, secondaryResume?.NextBlock ?? 0);

        if(nextBlock > primarySectors) nextBlock = primarySectors;

        return new Resume
        {
            BadBlocks       = MergeBlockList(primaryResume?.BadBlocks,        secondaryResume?.BadBlocks,        copied),
            ConcealedBlocks = MergeBlockList(primaryResume?.ConcealedBlocks,  secondaryResume?.ConcealedBlocks,  copied),
            MissingTitleKeys =
                MergeBlockList(primaryResume?.MissingTitleKeys, secondaryResume?.MissingTitleKeys, copied),
            BadSubchannels = MergeBlockList(primaryResume?.BadSubchannels?.Select(static s => (ulong)s).ToList(),
                                            secondaryResume?.BadSubchannels?.Select(static s => (ulong)s).ToList(),
                                            copied)
                ?.Select(static s => (int)s)
                 .ToList(),
            BlankExtents  = primaryResume?.BlankExtents,
            CreationDate  = primaryResume?.CreationDate ?? DateTime.UtcNow,
            LastWriteDate = DateTime.UtcNow,
            LastBlock = primaryResume?.LastBlock ?? (primarySectors > 0 ? primarySectors - 1 : 0),
            NextBlock = nextBlock,
            Removable = primaryResume?.Removable ?? secondaryResume?.Removable ?? false,
            Tape      = isTape,
            Tries     = mergedTries
        };
    }

    /// <summary>
    ///     Merges a per-sector list (bad blocks, concealed blocks...): the primary's entries survive unless the sector
    ///     was replaced by the secondary's, in which case the secondary's entry, if any, applies instead.
    /// </summary>
    static List<ulong> MergeBlockList(List<ulong> primary, List<ulong> secondary, HashSet<ulong> copied)
    {
        if(primary is null && secondary is null) return null;

        var merged = new SortedSet<ulong>();

        if(primary != null)
        {
            foreach(ulong block in primary.Where(b => !copied.Contains(b)))
                merged.Add(block);
        }

        if(secondary != null)
        {
            foreach(ulong block in secondary.Where(copied.Contains))
                merged.Add(block);
        }

        return merged.ToList();
    }

    /// <summary>Writes the resume information of the merged image next to it.</summary>
    void WriteMergedResume(Resume resume)
    {
        string resumePath = outputImagePath + ".resume.json";

        try
        {
            using var fs = new FileStream(resumePath, FileMode.Create, FileAccess.Write);

            JsonSerializer.Serialize(fs,
                                     new ResumeJson
                                     {
                                         Resume = resume
                                     },
                                     typeof(ResumeJson),
                                     ResumeJsonContext.Default);

            UpdateStatus?.Invoke(string.Format(UI.Written_resume_file_0, resumePath));
        }
        catch(Exception ex)
        {
            ErrorMessage?.Invoke(string.Format(UI.Error_0_writing_resume_file_1, ex.Message, resumePath));
            AaruLogging.Exception(ex, UI.Error_0_writing_resume_file_1, ex.Message, resumePath);
        }
    }
}