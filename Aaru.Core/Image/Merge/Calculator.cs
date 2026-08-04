using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Localization;

namespace Aaru.Core.Image;

public sealed partial class Merger
{
    /// <summary>
    ///     Single extent covering sectors <c>0 .. sectorCount - 1</c>, or an empty list when
    ///     <paramref name="sectorCount" /> is zero (avoids <c>0 - 1</c> unsigned underflow in callers).
    /// </summary>
    static List<Extent> CreateDefaultDumpExtents(ulong sectorCount)
    {
        if(sectorCount == 0) return new List<Extent>();

        return new List<Extent>
        {
            new Extent
            {
                Start = 0,
                End   = sectorCount - 1
            }
        };
    }

    /// <summary>
    ///     Resolves the dump tries for an image: the resume file if given, else the image's own dump hardware.
    ///     When neither provides any usable extent, every sector of the image is considered good, so a single
    ///     synthetic extent covering the whole image is returned.
    /// </summary>
    static List<DumpHardware> GetDumpTries(Resume resume, IMediaImage image)
    {
        List<DumpHardware> tries = resume != null ? resume.Tries : image.DumpHardware;

        if(tries?.Any(static h => h?.Extents?.Count > 0) == true) return tries;

        return
        [
            new DumpHardware
            {
                Extents = CreateDefaultDumpExtents(image.Info.Sectors)
            }
        ];
    }

    List<ulong> CalculateSectorsToCopy(IMediaImage primaryImage,    IMediaImage secondaryImage, Resume primaryResume,
                                       Resume      secondaryResume, List<ulong> overrideSectorsList)
    {
        List<DumpHardware> primaryTries   = GetDumpTries(primaryResume,   primaryImage);
        List<DumpHardware> secondaryTries = GetDumpTries(secondaryResume, secondaryImage);

        // Get all sectors that appear in secondaryTries but not in primaryTries
        var sectorsToCopy = new List<ulong>();

        // Iterate through all extents in secondaryTries
        foreach(DumpHardware secondaryHardware in secondaryTries)
        {
            if(secondaryHardware?.Extents == null) continue;

            foreach(Extent secondaryExtent in secondaryHardware.Extents)
            {
                // For each sector in this secondary extent
                for(ulong sector = secondaryExtent.Start; sector <= secondaryExtent.End; sector++)
                {
                    // Sectors beyond the primary image cannot exist in the output, skip them
                    if(sector >= primaryImage.Info.Sectors) continue;

                    // Check if this sector appears in any primary extent
                    var foundInPrimary = false;

                    foreach(DumpHardware primaryHardware in primaryTries)
                    {
                        if(primaryHardware?.Extents == null) continue;

                        if(primaryHardware.Extents.Any(primaryExtent =>
                                                           sector >= primaryExtent.Start &&
                                                           sector <= primaryExtent.End))
                            foundInPrimary = true;

                        if(foundInPrimary) break;
                    }

                    // If not found in primary, add to result
                    if(!foundInPrimary) sectorsToCopy.Add(sector);
                }
            }
        }

        // Primary sectors marked as bad in the resume file are valid candidates to take from the secondary,
        // as long as the secondary covers them and does not also mark them as bad
        if(primaryResume?.BadBlocks != null)
        {
            var alreadyListed = new HashSet<ulong>(sectorsToCopy);
            var secondaryBad  = secondaryResume?.BadBlocks != null ? new HashSet<ulong>(secondaryResume.BadBlocks) : [];

            foreach(ulong sector in primaryResume.BadBlocks)
            {
                if(sector >= primaryImage.Info.Sectors || sector >= secondaryImage.Info.Sectors) continue;

                if(secondaryBad.Contains(sector) || alreadyListed.Contains(sector)) continue;

                if(!secondaryTries.Any(h => h?.Extents?.Any(e => sector >= e.Start && sector <= e.End) == true))
                    continue;

                sectorsToCopy.Add(sector);
                alreadyListed.Add(sector);
            }
        }

        sectorsToCopy.AddRange(overrideSectorsList.Where(t => !sectorsToCopy.Contains(t)));

        return sectorsToCopy;
    }

    List<DumpHardware> CalculateMergedDumpHardware(IMediaImage primaryImage,  IMediaImage secondaryImage,
                                                   Resume      primaryResume, Resume      secondaryResume,
                                                   List<ulong> overrideSectorsList)
    {
        InitProgress?.Invoke();
        InitProgress2?.Invoke();

        List<DumpHardware> primaryTries   = GetDumpTries(primaryResume,   primaryImage);
        List<DumpHardware> secondaryTries = GetDumpTries(secondaryResume, secondaryImage);

        var mergedHardware = new List<DumpHardware>();

        // Create a mapping of which hardware each sector belongs to
        var sectorToHardware = new Dictionary<ulong, DumpHardware>();

        // Convert override list to HashSet for O(1) lookups
        var overrideSectorsSet = new HashSet<ulong>(overrideSectorsList);

        // First, build a lookup of which hardware each sector belongs to in primary tries
        var primarySectorToHardware = new Dictionary<ulong, DumpHardware>();

        UpdateProgress?.Invoke(UI.Building_primary_hardware_lookup, 0, 1);

        var primaryHardwareIndex = 0;

        foreach(DumpHardware primaryHardware in primaryTries)
        {
            if(_aborted) break;

            if(primaryHardware?.Extents == null) continue;

            var extentIndex = 0;

            foreach(Extent extent in primaryHardware.Extents)
            {
                if(_aborted) break;

                ulong extentSize = extent.End - extent.Start + 1;

                UpdateProgress2?.Invoke(string.Format(UI.Primary_hardware_0_extent_1_2_to_3,
                                                      primaryHardwareIndex,
                                                      extentIndex,
                                                      extent.Start,
                                                      extent.End),
                                        0,
                                        (long)extentSize);

                for(ulong sector = extent.Start; sector <= extent.End; sector++)
                {
                    if(_aborted) break;

                    primarySectorToHardware[sector] = primaryHardware;

                    UpdateProgress2?.Invoke(string.Format(UI.Primary_hardware_0_extent_1_2_to_3,
                                                          primaryHardwareIndex,
                                                          extentIndex,
                                                          extent.Start,
                                                          extent.End),
                                            (long)(sector - extent.Start + 1),
                                            (long)extentSize);
                }

                extentIndex++;
            }

            primaryHardwareIndex++;
        }

        // Build a lookup of which hardware each sector belongs to in secondary tries
        var secondarySectorToHardware = new Dictionary<ulong, DumpHardware>();

        UpdateProgress?.Invoke(UI.Building_secondary_hardware_lookup, 1, 3);

        var secondaryHardwareIndex = 0;

        foreach(DumpHardware secondaryHardware in secondaryTries)
        {
            if(_aborted) break;

            if(secondaryHardware?.Extents == null) continue;

            var extentIndex = 0;

            foreach(Extent extent in secondaryHardware.Extents)
            {
                if(_aborted) break;

                ulong extentSize = extent.End - extent.Start + 1;

                UpdateProgress2?.Invoke(string.Format(UI.Secondary_hardware_0_extent_1_2_to_3,
                                                      secondaryHardwareIndex,
                                                      extentIndex,
                                                      extent.Start,
                                                      extent.End),
                                        0,
                                        (long)extentSize);

                for(ulong sector = extent.Start; sector <= extent.End; sector++)
                {
                    secondarySectorToHardware[sector] = secondaryHardware;

                    UpdateProgress2?.Invoke(string.Format(UI.Secondary_hardware_0_extent_1_2_to_3,
                                                          secondaryHardwareIndex,
                                                          extentIndex,
                                                          extent.Start,
                                                          extent.End),
                                            (long)(sector - extent.Start + 1),
                                            (long)extentSize);
                }

                extentIndex++;
            }

            secondaryHardwareIndex++;
        }

        // Now assign hardware to each sector: use primary hardware, unless sector is in override list
        UpdateProgress?.Invoke(UI.Assigning_hardware_to_sectors, 2, 5);

        var  primarySectorsList = primarySectorToHardware.Keys.Order().ToList();
        long processedSectors   = 0;

        foreach((ulong sector, DumpHardware primaryHardware) in primarySectorToHardware)
        {
            if(_aborted) break;

            UpdateProgress2?.Invoke(string.Format(UI.Processing_sector_0, sector),
                                    processedSectors,
                                    primarySectorsList.Count);

            // If this sector should be overridden, use secondary hardware instead
            if(overrideSectorsSet.Contains(sector))
            {
                if(secondarySectorToHardware.TryGetValue(sector, out DumpHardware secondaryHardware))
                    sectorToHardware[sector] = secondaryHardware;
            }
            else
            {
                // Use primary hardware
                sectorToHardware[sector] = primaryHardware;
            }

            processedSectors++;
        }

        // Also add any sectors from override list that weren't in primary
        UpdateProgress?.Invoke(UI.Processing_override_sectors, 3, 5);

        long overrideProcessed = 0;

        foreach(ulong overrideSector in overrideSectorsSet)
        {
            if(_aborted) break;

            UpdateProgress2?.Invoke(string.Format(UI.Processing_override_sector_0, overrideSector),
                                    overrideProcessed,
                                    overrideSectorsSet.Count);

            if(!sectorToHardware.ContainsKey(overrideSector) &&
               secondarySectorToHardware.TryGetValue(overrideSector, out DumpHardware secondaryHardware))
                sectorToHardware[overrideSector] = secondaryHardware;

            overrideProcessed++;
        }

        // Create extents preserving sector order, grouping contiguous sectors from same hardware
        UpdateProgress?.Invoke(UI.Sorting_sectors, 4, 5);

        var allSectors = sectorToHardware.Keys.Order().ToList();

        if(allSectors.Count == 0)
        {
            EndProgress2?.Invoke();
            EndProgress?.Invoke();

            return mergedHardware;
        }

        // Start first extent
        UpdateProgress?.Invoke(UI.Creating_hardware_extents, 5, 5);

        DumpHardware currentHardware = sectorToHardware[allSectors[0]];
        ulong        extentStart     = allSectors[0];
        ulong        extentEnd       = allSectors[0];

        for(var i = 1; i < allSectors.Count; i++)
        {
            if(_aborted) break;

            ulong        sector = allSectors[i];
            DumpHardware hw     = sectorToHardware[sector];

            UpdateProgress2?.Invoke(string.Format(UI.Processing_extent_sector_0, sector), i, allSectors.Count);

            // Check if we should continue current extent or start new one
            if(hw == currentHardware && sector == extentEnd + 1)
            {
                // Same hardware and contiguous sector, extend current extent
                extentEnd = sector;
            }
            else
            {
                // Hardware changed or gap in sectors, save current extent and start new one
                AddOrUpdateHardware(mergedHardware, currentHardware, extentStart, extentEnd);

                currentHardware = hw;
                extentStart     = sector;
                extentEnd       = sector;
            }
        }

        // Add the last extent
        AddOrUpdateHardware(mergedHardware, currentHardware, extentStart, extentEnd);

        EndProgress2?.Invoke();
        EndProgress?.Invoke();

        // Sort results by the starting LBA of the first extent of each hardware
        return mergedHardware.OrderBy(static hw => hw.Extents?.FirstOrDefault()?.Start ?? 0).ToList();
    }

    static void AddOrUpdateHardware(List<DumpHardware> mergedHardware, DumpHardware originalHardware, ulong start,
                                    ulong              end)
    {
        // Check if we already have an entry for this hardware
        DumpHardware existing = mergedHardware.FirstOrDefault(h => h.Manufacturer == originalHardware.Manufacturer &&
                                                                   h.Model        == originalHardware.Model        &&
                                                                   h.Revision     == originalHardware.Revision     &&
                                                                   h.Firmware     == originalHardware.Firmware     &&
                                                                   h.Serial       == originalHardware.Serial);

        if(existing != null)
        {
            // Add extent to existing hardware
            existing.Extents.Add(new Extent
            {
                Start = start,
                End   = end
            });
        }
        else
        {
            // Create new hardware entry
            mergedHardware.Add(new DumpHardware
            {
                Manufacturer = originalHardware.Manufacturer,
                Model        = originalHardware.Model,
                Revision     = originalHardware.Revision,
                Firmware     = originalHardware.Firmware,
                Serial       = originalHardware.Serial,
                Software     = originalHardware.Software,
                Extents =
                [
                    new Extent
                    {
                        Start = start,
                        End   = end
                    }
                ]
            });
        }
    }
}