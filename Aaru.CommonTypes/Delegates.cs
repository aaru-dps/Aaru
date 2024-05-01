// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Delegates.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Delegates to communicate with user interface.
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

// ReSharper disable UnusedType.Global

namespace Aaru.CommonTypes;

/// <summary>Initializes a progress indicator (e.g. makes a progress bar visible)</summary>
public delegate void InitProgressHandler();

/// <summary>Updates a progress indicator with text</summary>
public delegate void UpdateProgressHandler(string text, long current, long maximum);

/// <summary>Pulses a progress indicator with indeterminate boundaries</summary>
public delegate void PulseProgressHandler(string text);

/// <summary>Uninitializes a progress indicator (e.g. adds a newline to the console)</summary>
public delegate void EndProgressHandler();

/// <summary>Initializes a secondary progress indicator (e.g. makes a progress bar visible)</summary>
public delegate void InitProgressHandler2();

/// <summary>Updates a secondary progress indicator with text</summary>
public delegate void UpdateProgressHandler2(string text, long current, long maximum);

/// <summary>Pulses a secondary progress indicator with indeterminate boundaries</summary>
public delegate void PulseProgressHandler2(string text);

/// <summary>Uninitializes a secondary progress indicator (e.g. adds a newline to the console)</summary>
public delegate void EndProgressHandler2();

/// <summary>Initializes two progress indicators (e.g. makes a progress bar visible)</summary>
public delegate void InitTwoProgressHandler();

/// <summary>Updates two progress indicators with text</summary>
public delegate void UpdateTwoProgressHandler(string text, long current, long maximum, string text2, long current2,
                                              long   maximum2);

/// <summary>Pulses a progress indicator with indeterminate boundaries</summary>
public delegate void PulseTwoProgressHandler(string text, string text2);

/// <summary>Uninitializes a progress indicator (e.g. adds a newline to the console)</summary>
public delegate void EndTwoProgressHandler();

/// <summary>Updates a status indicator</summary>
public delegate void UpdateStatusHandler(string text);

/// <summary>Shows an error message</summary>
public delegate void ErrorMessageHandler(string text);

/// <summary>Initializes a block map that's going to be filled with a media scan</summary>
public delegate void InitBlockMapHandler(ulong blocks, ulong blockSize, ulong blocksToRead, ushort currentProfile);

/// <summary>Updates lists of time taken on scanning from the specified sector</summary>
/// <param name="duration">Time in milliseconds</param>
public delegate void ScanTimeHandler(ulong sector, double duration);

/// <summary>Specified a number of blocks could not be read on scan</summary>
public delegate void ScanUnreadableHandler(ulong sector);

/// <summary>Sends the speed of scanning a specific sector</summary>
public delegate void ScanSpeedHandler(ulong sector, double currentSpeed);