// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Events.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Events to glue user interface with sidecar creation.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;

namespace Aaru.Core
{
    public sealed partial class Sidecar
    {
        /// <summary>Initializes a progress indicator (e.g. makes a progress bar visible)</summary>
        public event InitProgressHandler InitProgressEvent;
        /// <summary>Updates a progress indicator with text</summary>
        public event UpdateProgressHandler UpdateProgressEvent;
        /// <summary>Uninitializes a progress indicator (e.g. adds a newline to the console)</summary>
        public event EndProgressHandler EndProgressEvent;
        /// <summary>Initializes a secondary progress indicator (e.g. makes a progress bar visible)</summary>
        public event InitProgressHandler2 InitProgressEvent2;
        /// <summary>Event raised to update the values of a determinate progress bar</summary>
        public event UpdateProgressHandler2 UpdateProgressEvent2;
        /// <summary>Event raised when the progress bar is not longer needed</summary>
        public event EndProgressHandler2 EndProgressEvent2;
        /// <summary>Updates a status indicator</summary>
        public event UpdateStatusHandler UpdateStatusEvent;

        /// <summary>Initializes a progress indicator (e.g. makes a progress bar visible)</summary>
        public void InitProgress() => InitProgressEvent?.Invoke();

        /// <summary>Updates a progress indicator with text</summary>
        public void UpdateProgress(string text, long current, long maximum) =>
            UpdateProgressEvent?.Invoke(string.Format(text, current, maximum), current, maximum);

        /// <summary>Uninitializes a progress indicator (e.g. adds a newline to the console)</summary>
        public void EndProgress() => EndProgressEvent?.Invoke();

        /// <summary>Initializes a secondary progress indicator (e.g. makes a progress bar visible)</summary>
        public void InitProgress2() => InitProgressEvent2?.Invoke();

        /// <summary>Event raised to update the values of a determinate progress bar</summary>
        public void UpdateProgress2(string text, long current, long maximum) =>
            UpdateProgressEvent2?.Invoke(string.Format(text, current, maximum), current, maximum);

        /// <summary>Event raised when the progress bar is not longer needed</summary>
        public void EndProgress2() => EndProgressEvent2?.Invoke();

        /// <summary>Updates a status indicator</summary>
        public void UpdateStatus(string text, params object[] args) =>
            UpdateStatusEvent?.Invoke(string.Format(text, args));
    }
}