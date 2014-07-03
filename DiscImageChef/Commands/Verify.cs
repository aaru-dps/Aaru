/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Verify.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'verify' verb.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;

namespace DiscImageChef.Commands
{
    public static class Verify
    {
        public static void doVerify(VerifySubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--verify-disc={0}", options.VerifyDisc);
                Console.WriteLine("--verify-sectors={0}", options.VerifySectors);
            }
            Console.WriteLine("Verifying not yet implemented."); return;
        }
    }
}

