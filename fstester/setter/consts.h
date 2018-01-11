/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : consts.h
Author(s)      : Natalia Portillo

Component      : fstester.setter

--[ Description ] -----------------------------------------------------------

Constants

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

#ifndef DIC_FSTESTER_SETTER_CONSTS_H
#define DIC_FSTESTER_SETTER_CONSTS_H

extern const char *filenames[] = {
"FILNAM", "FILNAM.EXT", "FILENAME", "FILENAME.EXT", "UPPCAS", "lowcas", "UPPER.low",
"lower.UP", "CamUpr", "Dromed", "droMed", "FIL NA", " FILNA", "FILNA ", "FILE. XT",
"FILE .EXT", "FILE . XT", "Fourteen_Chars", "FifteenCharacts", "Sixteen_Characts",
"Twenty_One_Characters", "This name has thirty charactrs",
"This name has thirty one chactrs", "This name has thirty two chacters",
"This filename has fourty four characterrs",
"This filename has sixty three characters like a lazy dromedaire",
"This filename has sixty four characters like a redy lazy fox dog",
"This filename has one hundred twenty eight characters and once upon a time in a place which name you must buy the book yetnotget",
"This filename has two hundred thirty six characters and once upon a time in a place which name i have no desire to call to mind there lived not long since one of those gentlemen that keep a lance and well you know it so go and read its",
"This filename has two hundred fourty eight characters and once upon a time in a place which name i have no desire to call to mind there lived not long since one of those gentlemen that keep a lance and well you know it so go and read the book yout",
"This filename has two hundred fifty three characters and once upon a time in a place which name i have no desire to call to mind there lived not long since one of those gentlemen that keep a lance and well you know it so go and read the book as you get",
"This filename has two hundred fifty four characters and once upon a time in a place which name i have no desire to call to mind there lived not long since one of those gentlemen that keep a lance and well you know it so go and read the book as you mustd",
"This filename has two hundred fifty five characters and once upon a time in a place which name i have no desire to call to mind there lived not long since one of those gentlemen that keep a lance and well you know it so go and read the book as you mustdo",
"This filename has two hundred fifty six characters and once upon a time in a place which name i have no desire to call to mind there lived not long since one of those gentlemen that keep a lance and well you know it so go and read the book as you must get",
"?NM?E?", "N!A!M!", "NA/ME", "NA\\ME", "'QUOT'", "\"QUOT\"", "NA>ME>", "N<AME<",
"NA%%ME", "N*A*ME", "NA:ME", "N|AME|", "N.A.ME", ".NAME", "NAME.", "..NAME", "NAME..",
"N$ME", "N@ME@", "NAM#", "NA-ME", "_NAME_", 0 };

#define CLAUNIA_SIZE 7
extern const unsigned char clauniaBytes[] = { 0x43, 0x4C, 0x41, 0x55, 0x4E, 0x49, 0x41 };

#endif
