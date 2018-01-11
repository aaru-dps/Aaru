/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : dosos2.h
Author(s)      : Natalia Portillo

Component      : fstester.setter

--[ Description ] -----------------------------------------------------------

Contains definitions commons to DOS and OS/2

--[ License ] ---------------------------------------------------------------
     This program is free software: you can redistribute it and/or modify
     it under the terms of the GNU General Public License as
     published by the Free Software Foundation, either version 3 of the
     License, or (at your option) any later version.

     This program is distributed in the hope that it will be useful,
     but WITHOUT ANY WARRANTY; without even the implied warraty of
     MERCHANTIBILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     GNU General Public License for more details.

     You should have received a copy of the GNU General Public License
     along with this program.  If not, see <http://www.gnu.org/licenses/>.

-----------------------------------------------------------------------------
Copyright (C) 2011-2018 Natalia Portillo
*****************************************************************************/

#if defined(__OS2__) || defined (__os2__) || defined(__DOS__) || defined (MSDOS)

#ifndef DIC_FSTESTER_SETTER_DOSOS2_H
#define DIC_FSTESTER_SETTER_DOSOS2_H

const char* archivedAttributeText = "This file has the archived attribute set.\n";
const char* systemAttributeText = "This file has the system attribute set.\n";
const char* hiddenAttributeText = "This file has the hidden attribute set.\n";
const char* readonlyAttributeText = "This file has the read-only attribute set.\n";
const char* noAttributeText = "This file has no attribute set.\n";


#endif

#endif
