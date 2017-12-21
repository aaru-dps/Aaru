// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        public static event InitProgressHandler InitProgressEvent;
        public static event UpdateProgressHandler UpdateProgressEvent;
        public static event EndProgressHandler EndProgressEvent;
        public static event InitProgressHandler2 InitProgressEvent2;
        public static event UpdateProgressHandler2 UpdateProgressEvent2;
        public static event EndProgressHandler2 EndProgressEvent2;
        public static event UpdateStatusHandler UpdateStatusEvent;

        public static void InitProgress()
        {
            InitProgressEvent?.Invoke();
        }

        public static void UpdateProgress(string text, long current, long maximum)
        {
            UpdateProgressEvent?.Invoke(string.Format(text, current, maximum), current, maximum);
        }

        public static void EndProgress()
        {
            EndProgressEvent?.Invoke();
        }

        public static void InitProgress2()
        {
            InitProgressEvent2?.Invoke();
        }

        public static void UpdateProgress2(string text, long current, long maximum)
        {
            UpdateProgressEvent2?.Invoke(string.Format(text, current, maximum), current, maximum);
        }

        public static void EndProgress2()
        {
            EndProgressEvent2?.Invoke();
        }

        public static void UpdateStatus(string text, params object[] args)
        {
            UpdateStatusEvent?.Invoke(string.Format(text, args));
        }
    }
}