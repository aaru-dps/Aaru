// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Events.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
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
            if(InitProgressEvent != null)
                InitProgressEvent();
        }

        public static void UpdateProgress(string text, long current, long maximum)
        {
            if(UpdateProgressEvent != null)
                UpdateProgressEvent(string.Format(text, current, maximum), current, maximum);
        }

        public static void EndProgress()
        {
            if(EndProgressEvent != null)
                EndProgressEvent();
        }

        public static void InitProgress2()
        {
            if(InitProgressEvent2 != null)
                InitProgressEvent2();
        }

        public static void UpdateProgress2(string text, long current, long maximum)
        {
            if(UpdateProgressEvent2 != null)
                UpdateProgressEvent2(string.Format(text, current, maximum), current, maximum);
        }

        public static void EndProgress2()
        {
            if(EndProgressEvent2 != null)
                EndProgressEvent2();
        }

        public static void UpdateStatus(string text, params object[] args)
        {
            if(UpdateStatusEvent != null)
                UpdateStatusEvent(string.Format(text, args));
        }
    }
}
