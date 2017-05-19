// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Scramble.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Linq;
namespace DiscImageChef.Decoders.CD
{
    public static class Sector
    {
        public static readonly byte[] ScrambleTable =
        {
           0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x80, 0x00, 0x60,
           0x00, 0x28, 0x00, 0x1E, 0x80, 0x08, 0x60, 0x06, 0xA8, 0x02, 0xFE, 0x81, 0x80, 0x60, 0x60, 0x28,
           0x28, 0x1E, 0x9E, 0x88, 0x68, 0x66, 0xAE, 0xAA, 0xFC, 0x7F, 0x01, 0xE0, 0x00, 0x48, 0x00, 0x36,
           0x80, 0x16, 0xE0, 0x0E, 0xC8, 0x04, 0x56, 0x83, 0x7E, 0xE1, 0xE0, 0x48, 0x48, 0x36, 0xB6, 0x96,
           0xF6, 0xEE, 0xC6, 0xCC, 0x52, 0xD5, 0xFD, 0x9F, 0x01, 0xA8, 0x00, 0x7E, 0x80, 0x20, 0x60, 0x18,
           0x28, 0x0A, 0x9E, 0x87, 0x28, 0x62, 0x9E, 0xA9, 0xA8, 0x7E, 0xFE, 0xA0, 0x40, 0x78, 0x30, 0x22,
           0x94, 0x19, 0xAF, 0x4A, 0xFC, 0x37, 0x01, 0xD6, 0x80, 0x5E, 0xE0, 0x38, 0x48, 0x12, 0xB6, 0x8D,
           0xB6, 0xE5, 0xB6, 0xCB, 0x36, 0xD7, 0x56, 0xDE, 0xBE, 0xD8, 0x70, 0x5A, 0xA4, 0x3B, 0x3B, 0x53,
           0x53, 0x7D, 0xFD, 0xE1, 0x81, 0x88, 0x60, 0x66, 0xA8, 0x2A, 0xFE, 0x9F, 0x00, 0x68, 0x00, 0x2E,
           0x80, 0x1C, 0x60, 0x09, 0xE8, 0x06, 0xCE, 0x82, 0xD4, 0x61, 0x9F, 0x68, 0x68, 0x2E, 0xAE, 0x9C,
           0x7C, 0x69, 0xE1, 0xEE, 0xC8, 0x4C, 0x56, 0xB5, 0xFE, 0xF7, 0x00, 0x46, 0x80, 0x32, 0xE0, 0x15,
           0x88, 0x0F, 0x26, 0x84, 0x1A, 0xE3, 0x4B, 0x09, 0xF7, 0x46, 0xC6, 0xB2, 0xD2, 0xF5, 0x9D, 0x87,
           0x29, 0xA2, 0x9E, 0xF9, 0xA8, 0x42, 0xFE, 0xB1, 0x80, 0x74, 0x60, 0x27, 0x68, 0x1A, 0xAE, 0x8B,
           0x3C, 0x67, 0x51, 0xEA, 0xBC, 0x4F, 0x31, 0xF4, 0x14, 0x47, 0x4F, 0x72, 0xB4, 0x25, 0xB7, 0x5B,
           0x36, 0xBB, 0x56, 0xF3, 0x7E, 0xC5, 0xE0, 0x53, 0x08, 0x3D, 0xC6, 0x91, 0x92, 0xEC, 0x6D, 0x8D,
           0xED, 0xA5, 0x8D, 0xBB, 0x25, 0xB3, 0x5B, 0x35, 0xFB, 0x57, 0x03, 0x7E, 0x81, 0xE0, 0x60, 0x48,
           0x28, 0x36, 0x9E, 0x96, 0xE8, 0x6E, 0xCE, 0xAC, 0x54, 0x7D, 0xFF, 0x61, 0x80, 0x28, 0x60, 0x1E,
           0xA8, 0x08, 0x7E, 0x86, 0xA0, 0x62, 0xF8, 0x29, 0x82, 0x9E, 0xE1, 0xA8, 0x48, 0x7E, 0xB6, 0xA0,
           0x76, 0xF8, 0x26, 0xC2, 0x9A, 0xD1, 0xAB, 0x1C, 0x7F, 0x49, 0xE0, 0x36, 0xC8, 0x16, 0xD6, 0x8E,
           0xDE, 0xE4, 0x58, 0x4B, 0x7A, 0xB7, 0x63, 0x36, 0xA9, 0xD6, 0xFE, 0xDE, 0xC0, 0x58, 0x50, 0x3A,
           0xBC, 0x13, 0x31, 0xCD, 0xD4, 0x55, 0x9F, 0x7F, 0x28, 0x20, 0x1E, 0x98, 0x08, 0x6A, 0x86, 0xAF,
           0x22, 0xFC, 0x19, 0x81, 0xCA, 0xE0, 0x57, 0x08, 0x3E, 0x86, 0x90, 0x62, 0xEC, 0x29, 0x8D, 0xDE,
           0xE5, 0x98, 0x4B, 0x2A, 0xB7, 0x5F, 0x36, 0xB8, 0x16, 0xF2, 0x8E, 0xC5, 0xA4, 0x53, 0x3B, 0x7D,
           0xD3, 0x61, 0x9D, 0xE8, 0x69, 0x8E, 0xAE, 0xE4, 0x7C, 0x4B, 0x61, 0xF7, 0x68, 0x46, 0xAE, 0xB2,
           0xFC, 0x75, 0x81, 0xE7, 0x20, 0x4A, 0x98, 0x37, 0x2A, 0x96, 0x9F, 0x2E, 0xE8, 0x1C, 0x4E, 0x89,
           0xF4, 0x66, 0xC7, 0x6A, 0xD2, 0xAF, 0x1D, 0xBC, 0x09, 0xB1, 0xC6, 0xF4, 0x52, 0xC7, 0x7D, 0x92,
           0xA1, 0xAD, 0xB8, 0x7D, 0xB2, 0xA1, 0xB5, 0xB8, 0x77, 0x32, 0xA6, 0x95, 0xBA, 0xEF, 0x33, 0x0C,
           0x15, 0xC5, 0xCF, 0x13, 0x14, 0x0D, 0xCF, 0x45, 0x94, 0x33, 0x2F, 0x55, 0xDC, 0x3F, 0x19, 0xD0,
           0x0A, 0xDC, 0x07, 0x19, 0xC2, 0x8A, 0xD1, 0xA7, 0x1C, 0x7A, 0x89, 0xE3, 0x26, 0xC9, 0xDA, 0xD6,
           0xDB, 0x1E, 0xDB, 0x48, 0x5B, 0x76, 0xBB, 0x66, 0xF3, 0x6A, 0xC5, 0xEF, 0x13, 0x0C, 0x0D, 0xC5,
           0xC5, 0x93, 0x13, 0x2D, 0xCD, 0xDD, 0x95, 0x99, 0xAF, 0x2A, 0xFC, 0x1F, 0x01, 0xC8, 0x00, 0x56,
           0x80, 0x3E, 0xE0, 0x10, 0x48, 0x0C, 0x36, 0x85, 0xD6, 0xE3, 0x1E, 0xC9, 0xC8, 0x56, 0xD6, 0xBE,
           0xDE, 0xF0, 0x58, 0x44, 0x3A, 0xB3, 0x53, 0x35, 0xFD, 0xD7, 0x01, 0x9E, 0x80, 0x68, 0x60, 0x2E,
           0xA8, 0x1C, 0x7E, 0x89, 0xE0, 0x66, 0xC8, 0x2A, 0xD6, 0x9F, 0x1E, 0xE8, 0x08, 0x4E, 0x86, 0xB4,
           0x62, 0xF7, 0x69, 0x86, 0xAE, 0xE2, 0xFC, 0x49, 0x81, 0xF6, 0xE0, 0x46, 0xC8, 0x32, 0xD6, 0x95,
           0x9E, 0xEF, 0x28, 0x4C, 0x1E, 0xB5, 0xC8, 0x77, 0x16, 0xA6, 0x8E, 0xFA, 0xE4, 0x43, 0x0B, 0x71,
           0xC7, 0x64, 0x52, 0xAB, 0x7D, 0xBF, 0x61, 0xB0, 0x28, 0x74, 0x1E, 0xA7, 0x48, 0x7A, 0xB6, 0xA3,
           0x36, 0xF9, 0xD6, 0xC2, 0xDE, 0xD1, 0x98, 0x5C, 0x6A, 0xB9, 0xEF, 0x32, 0xCC, 0x15, 0x95, 0xCF,
           0x2F, 0x14, 0x1C, 0x0F, 0x49, 0xC4, 0x36, 0xD3, 0x56, 0xDD, 0xFE, 0xD9, 0x80, 0x5A, 0xE0, 0x3B,
           0x08, 0x13, 0x46, 0x8D, 0xF2, 0xE5, 0x85, 0x8B, 0x23, 0x27, 0x59, 0xDA, 0xBA, 0xDB, 0x33, 0x1B,
           0x55, 0xCB, 0x7F, 0x17, 0x60, 0x0E, 0xA8, 0x04, 0x7E, 0x83, 0x60, 0x61, 0xE8, 0x28, 0x4E, 0x9E,
           0xB4, 0x68, 0x77, 0x6E, 0xA6, 0xAC, 0x7A, 0xFD, 0xE3, 0x01, 0x89, 0xC0, 0x66, 0xD0, 0x2A, 0xDC,
           0x1F, 0x19, 0xC8, 0x0A, 0xD6, 0x87, 0x1E, 0xE2, 0x88, 0x49, 0xA6, 0xB6, 0xFA, 0xF6, 0xC3, 0x06,
           0xD1, 0xC2, 0xDC, 0x51, 0x99, 0xFC, 0x6A, 0xC1, 0xEF, 0x10, 0x4C, 0x0C, 0x35, 0xC5, 0xD7, 0x13,
           0x1E, 0x8D, 0xC8, 0x65, 0x96, 0xAB, 0x2E, 0xFF, 0x5C, 0x40, 0x39, 0xF0, 0x12, 0xC4, 0x0D, 0x93,
           0x45, 0xAD, 0xF3, 0x3D, 0x85, 0xD1, 0xA3, 0x1C, 0x79, 0xC9, 0xE2, 0xD6, 0xC9, 0x9E, 0xD6, 0xE8,
           0x5E, 0xCE, 0xB8, 0x54, 0x72, 0xBF, 0x65, 0xB0, 0x2B, 0x34, 0x1F, 0x57, 0x48, 0x3E, 0xB6, 0x90,
           0x76, 0xEC, 0x26, 0xCD, 0xDA, 0xD5, 0x9B, 0x1F, 0x2B, 0x48, 0x1F, 0x76, 0x88, 0x26, 0xE6, 0x9A,
           0xCA, 0xEB, 0x17, 0x0F, 0x4E, 0x84, 0x34, 0x63, 0x57, 0x69, 0xFE, 0xAE, 0xC0, 0x7C, 0x50, 0x21,
           0xFC, 0x18, 0x41, 0xCA, 0xB0, 0x57, 0x34, 0x3E, 0x97, 0x50, 0x6E, 0xBC, 0x2C, 0x71, 0xDD, 0xE4,
           0x59, 0x8B, 0x7A, 0xE7, 0x63, 0x0A, 0xA9, 0xC7, 0x3E, 0xD2, 0x90, 0x5D, 0xAC, 0x39, 0xBD, 0xD2,
           0xF1, 0x9D, 0x84, 0x69, 0xA3, 0x6E, 0xF9, 0xEC, 0x42, 0xCD, 0xF1, 0x95, 0x84, 0x6F, 0x23, 0x6C,
           0x19, 0xED, 0xCA, 0xCD, 0x97, 0x15, 0xAE, 0x8F, 0x3C, 0x64, 0x11, 0xEB, 0x4C, 0x4F, 0x75, 0xF4,
           0x27, 0x07, 0x5A, 0x82, 0xBB, 0x21, 0xB3, 0x58, 0x75, 0xFA, 0xA7, 0x03, 0x3A, 0x81, 0xD3, 0x20,
           0x5D, 0xD8, 0x39, 0x9A, 0x92, 0xEB, 0x2D, 0x8F, 0x5D, 0xA4, 0x39, 0xBB, 0x52, 0xF3, 0x7D, 0x85,
           0xE1, 0xA3, 0x08, 0x79, 0xC6, 0xA2, 0xD2, 0xF9, 0x9D, 0x82, 0xE9, 0xA1, 0x8E, 0xF8, 0x64, 0x42,
           0xAB, 0x71, 0xBF, 0x64, 0x70, 0x2B, 0x64, 0x1F, 0x6B, 0x48, 0x2F, 0x76, 0x9C, 0x26, 0xE9, 0xDA,
           0xCE, 0xDB, 0x14, 0x5B, 0x4F, 0x7B, 0x74, 0x23, 0x67, 0x59, 0xEA, 0xBA, 0xCF, 0x33, 0x14, 0x15,
           0xCF, 0x4F, 0x14, 0x34, 0x0F, 0x57, 0x44, 0x3E, 0xB3, 0x50, 0x75, 0xFC, 0x27, 0x01, 0xDA, 0x80,
           0x5B, 0x20, 0x3B, 0x58, 0x13, 0x7A, 0x8D, 0xE3, 0x25, 0x89, 0xDB, 0x26, 0xDB, 0x5A, 0xDB, 0x7B,
           0x1B, 0x63, 0x4B, 0x69, 0xF7, 0x6E, 0xC6, 0xAC, 0x52, 0xFD, 0xFD, 0x81, 0x81, 0xA0, 0x60, 0x78,
           0x28, 0x22, 0x9E, 0x99, 0xA8, 0x6A, 0xFE, 0xAF, 0x00, 0x7C, 0x00, 0x21, 0xC0, 0x18, 0x50, 0x0A,
           0xBC, 0x07, 0x31, 0xC2, 0x94, 0x51, 0xAF, 0x7C, 0x7C, 0x21, 0xE1, 0xD8, 0x48, 0x5A, 0xB6, 0xBB,
           0x36, 0xF3, 0x56, 0xC5, 0xFE, 0xD3, 0x00, 0x5D, 0xC0, 0x39, 0x90, 0x12, 0xEC, 0x0D, 0x8D, 0xC5,
           0xA5, 0x93, 0x3B, 0x2D, 0xD3, 0x5D, 0x9D, 0xF9, 0xA9, 0x82, 0xFE, 0xE1, 0x80, 0x48, 0x60, 0x36,
           0xA8, 0x16, 0xFE, 0x8E, 0xC0, 0x64, 0x50, 0x2B, 0x7C, 0x1F, 0x61, 0xC8, 0x28, 0x56, 0x9E, 0xBE,
           0xE8, 0x70, 0x4E, 0xA4, 0x34, 0x7B, 0x57, 0x63, 0x7E, 0xA9, 0xE0, 0x7E, 0xC8, 0x20, 0x56, 0x98,
           0x3E, 0xEA, 0x90, 0x4F, 0x2C, 0x34, 0x1D, 0xD7, 0x49, 0x9E, 0xB6, 0xE8, 0x76, 0xCE, 0xA6, 0xD4,
           0x7A, 0xDF, 0x63, 0x18, 0x29, 0xCA, 0x9E, 0xD7, 0x28, 0x5E, 0x9E, 0xB8, 0x68, 0x72, 0xAE, 0xA5,
           0xBC, 0x7B, 0x31, 0xE3, 0x54, 0x49, 0xFF, 0x76, 0xC0, 0x26, 0xD0, 0x1A, 0xDC, 0x0B, 0x19, 0xC7,
           0x4A, 0xD2, 0xB7, 0x1D, 0xB6, 0x89, 0xB6, 0xE6, 0xF6, 0xCA, 0xC6, 0xD7, 0x12, 0xDE, 0x8D, 0x98,
           0x65, 0xAA, 0xAB, 0x3F, 0x3F, 0x50, 0x10, 0x3C, 0x0C, 0x11, 0xC5, 0xCC, 0x53, 0x15, 0xFD, 0xCF,
           0x01, 0x94, 0x00, 0x6F, 0x40, 0x2C, 0x30, 0x1D, 0xD4, 0x09, 0x9F, 0x46, 0xE8, 0x32, 0xCE, 0x95,
           0x94, 0x6F, 0x2F, 0x6C, 0x1C, 0x2D, 0xC9, 0xDD, 0x96, 0xD9, 0xAE, 0xDA, 0xFC, 0x5B, 0x01, 0xFB,
           0x40, 0x43, 0x70, 0x31, 0xE4, 0x14, 0x4B, 0x4F, 0x77, 0x74, 0x26, 0xA7, 0x5A, 0xFA, 0xBB, 0x03,
           0x33, 0x41, 0xD5, 0xF0, 0x5F, 0x04, 0x38, 0x03, 0x52, 0x81, 0xFD, 0xA0, 0x41, 0xB8, 0x30, 0x72,
           0x94, 0x25, 0xAF, 0x5B, 0x3C, 0x3B, 0x51, 0xD3, 0x7C, 0x5D, 0xE1, 0xF9, 0x88, 0x42, 0xE6, 0xB1,
           0x8A, 0xF4, 0x67, 0x07, 0x6A, 0x82, 0xAF, 0x21, 0xBC, 0x18, 0x71, 0xCA, 0xA4, 0x57, 0x3B, 0x7E,
           0x93, 0x60, 0x6D, 0xE8, 0x2D, 0x8E, 0x9D, 0xA4, 0x69, 0xBB, 0x6E, 0xF3, 0x6C, 0x45, 0xED, 0xF3,
           0x0D, 0x85, 0xC5, 0xA3, 0x13, 0x39, 0xCD, 0xD2, 0xD5, 0x9D, 0x9F, 0x29, 0xA8, 0x1E, 0xFE, 0x88,
           0x40, 0x66, 0xB0, 0x2A, 0xF4, 0x1F, 0x07, 0x48, 0x02, 0xB6, 0x81, 0xB6, 0xE0, 0x76, 0xC8, 0x26,
           0xD6, 0x9A, 0xDE, 0xEB, 0x18, 0x4F, 0x4A, 0xB4, 0x37, 0x37, 0x56, 0x96, 0xBE, 0xEE, 0xF0, 0x4C,
           0x44, 0x35, 0xF3, 0x57, 0x05, 0xFE, 0x83, 0x00, 0x61, 0xC0, 0x28, 0x50, 0x1E, 0xBC, 0x08, 0x71,
           0xC6, 0xA4, 0x52, 0xFB, 0x7D, 0x83, 0x61, 0xA1, 0xE8, 0x78, 0x4E, 0xA2, 0xB4, 0x79, 0xB7, 0x62,
           0xF6, 0xA9, 0x86, 0xFE, 0xE2, 0xC0, 0x49, 0x90, 0x36, 0xEC, 0x16, 0xCD, 0xCE, 0xD5, 0x94, 0x5F,
           0x2F, 0x78, 0x1C, 0x22, 0x89, 0xD9, 0xA6, 0xDA, 0xFA, 0xDB, 0x03, 0x1B, 0x41, 0xCB, 0x70, 0x57,
           0x64, 0x3E, 0xAB, 0x50, 0x7F, 0x7C, 0x20, 0x21, 0xD8, 0x18, 0x5A, 0x8A, 0xBB, 0x27, 0x33, 0x5A,
           0x95, 0xFB, 0x2F, 0x03, 0x5C, 0x01, 0xF9, 0xC0, 0x42, 0xD0, 0x31, 0x9C, 0x14, 0x69, 0xCF, 0x6E,
           0xD4, 0x2C, 0x5F, 0x5D, 0xF8, 0x39, 0x82, 0x92, 0xE1, 0xAD, 0x88, 0x7D, 0xA6, 0xA1, 0xBA, 0xF8,
           0x73, 0x02, 0xA5, 0xC1, 0xBB, 0x10, 0x73, 0x4C, 0x25, 0xF5, 0xDB, 0x07, 0x1B, 0x42, 0x8B, 0x71,
           0xA7, 0x64, 0x7A, 0xAB, 0x63, 0x3F, 0x69, 0xD0, 0x2E, 0xDC, 0x1C, 0x59, 0xC9, 0xFA, 0xD6, 0xC3,
           0x1E, 0xD1, 0xC8, 0x5C, 0x56, 0xB9, 0xFE, 0xF2, 0xC0, 0x45, 0x90, 0x33, 0x2C, 0x15, 0xDD, 0xCF,
           0x19, 0x94, 0x0A, 0xEF, 0x47, 0x0C, 0x32, 0x85, 0xD5, 0xA3, 0x1F, 0x39, 0xC8, 0x12, 0xD6, 0x8D,
           0x9E, 0xE5, 0xA8, 0x4B, 0x3E, 0xB7, 0x50, 0x76, 0xBC, 0x26, 0xF1, 0xDA, 0xC4, 0x5B, 0x13, 0x7B,
           0x4D, 0xE3, 0x75, 0x89, 0xE7, 0x26, 0xCA, 0x9A, 0xD7, 0x2B, 0x1E, 0x9F, 0x48, 0x68, 0x36, 0xAE,
           0x96, 0xFC, 0x6E, 0xC1, 0xEC, 0x50, 0x4D, 0xFC, 0x35, 0x81, 0xD7, 0x20, 0x5E, 0x98, 0x38, 0x6A,
           0x92, 0xAF, 0x2D, 0xBC, 0x1D, 0xB1, 0xC9, 0xB4, 0x56, 0xF7, 0x7E, 0xC6, 0xA0, 0x52, 0xF8, 0x3D,
           0x82, 0x91, 0xA1, 0xAC, 0x78, 0x7D, 0xE2, 0xA1, 0x89, 0xB8, 0x66, 0xF2, 0xAA, 0xC5, 0xBF, 0x13,
           0x30, 0x0D, 0xD4, 0x05, 0x9F, 0x43, 0x28, 0x31, 0xDE, 0x94, 0x58, 0x6F, 0x7A, 0xAC, 0x23, 0x3D,
           0xD9, 0xD1, 0x9A, 0xDC, 0x6B, 0x19, 0xEF, 0x4A, 0xCC, 0x37, 0x15, 0xD6, 0x8F, 0x1E, 0xE4, 0x08,
           0x4B, 0x46, 0xB7, 0x72, 0xF6, 0xA5, 0x86, 0xFB, 0x22, 0xC3, 0x59, 0x91, 0xFA, 0xEC, 0x43, 0x0D,
           0xF1, 0xC5, 0x84, 0x53, 0x23, 0x7D, 0xD9, 0xE1, 0x9A, 0xC8, 0x6B, 0x16, 0xAF, 0x4E, 0xFC, 0x34,
           0x41, 0xD7, 0x70, 0x5E, 0xA4, 0x38, 0x7B, 0x52, 0xA3, 0x7D, 0xB9, 0xE1, 0xB2, 0xC8, 0x75, 0x96,
           0xA7, 0x2E, 0xFA, 0x9C, 0x43, 0x29, 0xF1, 0xDE, 0xC4, 0x58, 0x53, 0x7A, 0xBD, 0xE3, 0x31, 0x89,
           0xD4, 0x66, 0xDF, 0x6A, 0xD8, 0x2F, 0x1A, 0x9C, 0x0B, 0x29, 0xC7, 0x5E, 0xD2, 0xB8, 0x5D, 0xB2,
           0xB9, 0xB5, 0xB2, 0xF7, 0x35, 0x86, 0x97, 0x22, 0xEE, 0x99, 0x8C, 0x6A, 0xE5, 0xEF, 0x0B, 0x0C,
           0x07, 0x45, 0xC2, 0xB3, 0x11, 0xB5, 0xCC, 0x77, 0x15, 0xE6, 0x8F, 0x0A, 0xE4, 0x07, 0x0B, 0x42,
           0x87, 0x71, 0xA2, 0xA4, 0x79, 0xBB, 0x62, 0xF3, 0x69, 0x85, 0xEE, 0xE3, 0x0C, 0x49, 0xC5, 0xF6,
           0xD3, 0x06, 0xDD, 0xC2, 0xD9, 0x91, 0x9A, 0xEC, 0x6B, 0x0D, 0xEF, 0x45, 0x8C, 0x33, 0x25, 0xD5,
           0xDB, 0x1F, 0x1B, 0x48, 0x0B, 0x76, 0x87, 0x66, 0xE2, 0xAA, 0xC9, 0xBF, 0x16, 0xF0, 0x0E, 0xC4,
           0x04, 0x53, 0x43, 0x7D, 0xF1, 0xE1, 0x84, 0x48, 0x63, 0x76, 0xA9, 0xE6, 0xFE, 0xCA, 0xC0, 0x57,
           0x10, 0x3E, 0x8C, 0x10, 0x65, 0xCC, 0x2B, 0x15, 0xDF, 0x4F, 0x18, 0x34, 0x0A, 0x97, 0x47, 0x2E,
           0xB2, 0x9C, 0x75, 0xA9, 0xE7, 0x3E, 0xCA, 0x90, 0x57, 0x2C, 0x3E, 0x9D, 0xD0, 0x69, 0x9C, 0x2E,
           0xE9, 0xDC, 0x4E, 0xD9, 0xF4, 0x5A, 0xC7, 0x7B, 0x12, 0xA3, 0x4D, 0xB9, 0xF5, 0xB2, 0xC7, 0x35,
           0x92, 0x97, 0x2D, 0xAE, 0x9D, 0xBC, 0x69, 0xB1, 0xEE, 0xF4, 0x4C, 0x47, 0x75, 0xF2, 0xA7, 0x05,
           0xBA, 0x83, 0x33, 0x21, 0xD5, 0xD8, 0x5F, 0x1A, 0xB8, 0x0B, 0x32, 0x87, 0x55, 0xA2, 0xBF, 0x39,
           0xB0, 0x12, 0xF4, 0x0D, 0x87, 0x45, 0xA2, 0xB3, 0x39, 0xB5, 0xD2, 0xF7, 0x1D, 0x86, 0x89, 0xA2,
           0xE6, 0xF9, 0x8A, 0xC2, 0xE7, 0x11, 0x8A, 0x8C, 0x67, 0x25, 0xEA, 0x9B, 0x0F, 0x2B, 0x44, 0x1F,
           0x73, 0x48, 0x25, 0xF6, 0x9B, 0x06, 0xEB, 0x42, 0xCF, 0x71, 0x94, 0x24, 0x6F, 0x5B, 0x6C, 0x3B,
           0x6D, 0xD3, 0x6D, 0x9D, 0xED, 0xA9, 0x8D, 0xBE, 0xE5, 0xB0, 0x4B, 0x34, 0x37, 0x57, 0x56, 0xBE,
           0xBE, 0xF0, 0x70, 0x44, 0x24, 0x33, 0x5B, 0x55, 0xFB, 0x7F, 0x03, 0x60, 0x01, 0xE8, 0x00, 0x4E,
           0x80, 0x34, 0x60, 0x17, 0x68, 0x0E, 0xAE, 0x84, 0x7C, 0x63, 0x61, 0xE9, 0xE8, 0x4E, 0xCE, 0xB4,
           0x54, 0x77, 0x7F, 0x66, 0xA0, 0x2A, 0xF8, 0x1F, 0x02, 0x88, 0x01, 0xA6, 0x80, 0x7A, 0xE0, 0x23,
           0x08, 0x19, 0xC6, 0x8A, 0xD2, 0xE7, 0x1D, 0x8A, 0x89, 0xA7, 0x26, 0xFA, 0x9A, 0xC3, 0x2B, 0x11,
           0xDF, 0x4C, 0x58, 0x35, 0xFA, 0x97, 0x03, 0x2E, 0x81, 0xDC, 0x60, 0x59, 0xE8, 0x3A, 0xCE, 0x93,
           0x14, 0x6D, 0xCF, 0x6D, 0x94, 0x2D, 0xAF, 0x5D, 0xBC, 0x39, 0xB1, 0xD2, 0xF4, 0x5D, 0x87, 0x79,
           0xA2, 0xA2, 0xF9, 0xB9, 0x82, 0xF2, 0xE1, 0x85, 0x88, 0x63, 0x26, 0xA9, 0xDA, 0xFE, 0xDB, 0x00,
           0x5B, 0x40, 0x3B, 0x70, 0x13, 0x64, 0x0D, 0xEB, 0x45, 0x8F, 0x73, 0x24, 0x25, 0xDB, 0x5B, 0x1B,
           0x7B, 0x4B, 0x63, 0x77, 0x69, 0xE6, 0xAE, 0xCA, 0xFC, 0x57, 0x01, 0xFE, 0x80, 0x40, 0x60, 0x30,
           0x28, 0x14, 0x1E, 0x8F, 0x48, 0x64, 0x36, 0xAB, 0x56, 0xFF, 0x7E, 0xC0, 0x20, 0x50, 0x18, 0x3C,
           0x0A, 0x91, 0xC7, 0x2C, 0x52, 0x9D, 0xFD, 0xA9, 0x81, 0xBE, 0xE0, 0x70, 0x48, 0x24, 0x36, 0x9B,
           0x56, 0xEB, 0x7E, 0xCF, 0x60, 0x54, 0x28, 0x3F, 0x5E, 0x90, 0x38, 0x6C, 0x12, 0xAD, 0xCD, 0xBD,
           0x95, 0xB1, 0xAF, 0x34, 0x7C, 0x17, 0x61, 0xCE, 0xA8, 0x54, 0x7E, 0xBF, 0x60, 0x70, 0x28, 0x24,
           0x1E, 0x9B, 0x48, 0x6B, 0x76, 0xAF, 0x66, 0xFC, 0x2A, 0xC1, 0xDF, 0x10, 0x58, 0x0C, 0x3A, 0x85,
           0xD3, 0x23, 0x1D, 0xD9, 0xC9, 0x9A, 0xD6, 0xEB, 0x1E, 0xCF, 0x48, 0x54, 0x36, 0xBF, 0x56, 0xF0,
           0x3E, 0xC4, 0x10, 0x53, 0x4C, 0x3D, 0xF5, 0xD1, 0x87, 0x1C, 0x62, 0x89, 0xE9, 0xA6, 0xCE, 0xFA,
           0xD4, 0x43, 0x1F, 0x71, 0xC8, 0x24, 0x56, 0x9B, 0x7E, 0xEB, 0x60, 0x4F, 0x68, 0x34, 0x2E, 0x97,
           0x5C, 0x6E, 0xB9, 0xEC, 0x72, 0xCD, 0xE5, 0x95, 0x8B, 0x2F, 0x27, 0x5C, 0x1A, 0xB9, 0xCB, 0x32,
           0xD7, 0x55, 0x9E, 0xBF, 0x28, 0x70, 0x1E, 0xA4, 0x08, 0x7B, 0x46, 0xA3, 0x72, 0xF9, 0xE5, 0x82,
           0xCB, 0x21, 0x97, 0x58, 0x6E, 0xBA, 0xAC, 0x73, 0x3D, 0xE5, 0xD1, 0x8B, 0x1C, 0x67, 0x49, 0xEA,
           0xB6, 0xCF, 0x36, 0xD4, 0x16, 0xDF, 0x4E, 0xD8, 0x34, 0x5A, 0x97, 0x7B, 0x2E, 0xA3, 0x5C, 0x79,
           0xF9, 0xE2, 0xC2, 0xC9, 0x91, 0x96, 0xEC, 0x6E, 0xCD, 0xEC, 0x55, 0x8D, 0xFF, 0x25, 0x80, 0x1B,
           0x20, 0x0B, 0x58, 0x07, 0x7A, 0x82, 0xA3, 0x21, 0xB9, 0xD8, 0x72, 0xDA, 0xA5, 0x9B, 0x3B, 0x2B,
           0x53, 0x5F, 0x7D, 0xF8, 0x21, 0x82, 0x98, 0x61, 0xAA, 0xA8, 0x7F, 0x3E, 0xA0, 0x10, 0x78, 0x0C,
           0x22, 0x85, 0xD9, 0xA3, 0x1A, 0xF9, 0xCB, 0x02, 0xD7, 0x41, 0x9E, 0xB0, 0x68, 0x74, 0x2E, 0xA7,
           0x5C, 0x7A, 0xB9, 0xE3, 0x32, 0xC9, 0xD5, 0x96, 0xDF, 0x2E, 0xD8, 0x1C, 0x5A, 0x89, 0xFB, 0x26,
           0xC3, 0x5A, 0xD1, 0xFB, 0x1C, 0x43, 0x49, 0xF1, 0xF6, 0xC4, 0x46, 0xD3, 0x72, 0xDD, 0xE5, 0x99
        };

        public static readonly byte[] SyncMark = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };

        public static byte[] Scramble(byte[] sector)
        {
            if(sector == null || sector.Length < 2352)
                return sector;

            byte[] sync = new byte[12];
            Array.Copy(sector, 0, sync, 0, 12);

            if(!SyncMark.SequenceEqual(sync))
                return sector;

            byte[] scrambled = new byte[sector.Length];
            for(int i = 0; i < 2352; i++)
                scrambled[i] = (byte)(sector[i] ^ ScrambleTable[i]);

            if(sector.Length > 2352)
            {
                for(int i = 2352; i < sector.Length; i++)
                    scrambled[i] = sector[i];
            }

            return scrambled;
        }
    }
}