// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ClauniaSubchannelTransform.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the Claunia Subchannel Transform algorithm.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Console;

namespace Aaru.DiscImages
{
    public sealed partial class AaruFormat
    {
        static byte[] ClauniaSubchannelTransform(byte[] interleaved)
        {
            if(interleaved == null)
                return null;

            int[] p = new int[interleaved.Length / 8];
            int[] q = new int[interleaved.Length / 8];
            int[] r = new int[interleaved.Length / 8];
            int[] s = new int[interleaved.Length / 8];
            int[] t = new int[interleaved.Length / 8];
            int[] u = new int[interleaved.Length / 8];
            int[] v = new int[interleaved.Length / 8];
            int[] w = new int[interleaved.Length / 8];

            DateTime start = DateTime.UtcNow;

            for(int i = 0; i < interleaved.Length; i += 8)
            {
                p[i / 8] =  interleaved[i] & 0x80;
                p[i / 8] += (interleaved[i + 1] & 0x80) >> 1;
                p[i / 8] += (interleaved[i + 2] & 0x80) >> 2;
                p[i / 8] += (interleaved[i + 3] & 0x80) >> 3;
                p[i / 8] += (interleaved[i + 4] & 0x80) >> 4;
                p[i / 8] += (interleaved[i + 5] & 0x80) >> 5;
                p[i / 8] += (interleaved[i + 6] & 0x80) >> 6;
                p[i / 8] += (interleaved[i + 7] & 0x80) >> 7;

                q[i / 8] =  (interleaved[i] & 0x40) << 1;
                q[i / 8] += interleaved[i + 1] & 0x40;
                q[i / 8] += (interleaved[i + 2] & 0x40) >> 1;
                q[i / 8] += (interleaved[i + 3] & 0x40) >> 2;
                q[i / 8] += (interleaved[i + 4] & 0x40) >> 3;
                q[i / 8] += (interleaved[i + 5] & 0x40) >> 4;
                q[i / 8] += (interleaved[i + 6] & 0x40) >> 5;
                q[i / 8] += (interleaved[i + 7] & 0x40) >> 6;

                r[i / 8] =  (interleaved[i]     & 0x20) << 2;
                r[i / 8] += (interleaved[i + 1] & 0x20) << 1;
                r[i / 8] += interleaved[i + 2] & 0x20;
                r[i / 8] += (interleaved[i + 3] & 0x20) >> 1;
                r[i / 8] += (interleaved[i + 4] & 0x20) >> 2;
                r[i / 8] += (interleaved[i + 5] & 0x20) >> 3;
                r[i / 8] += (interleaved[i + 6] & 0x20) >> 4;
                r[i / 8] += (interleaved[i + 7] & 0x20) >> 5;

                s[i / 8] =  (interleaved[i]     & 0x10) << 3;
                s[i / 8] += (interleaved[i + 1] & 0x10) << 2;
                s[i / 8] += (interleaved[i + 2] & 0x10) << 1;
                s[i / 8] += interleaved[i + 3] & 0x10;
                s[i / 8] += (interleaved[i + 4] & 0x10) >> 1;
                s[i / 8] += (interleaved[i + 5] & 0x10) >> 2;
                s[i / 8] += (interleaved[i + 6] & 0x10) >> 3;
                s[i / 8] += (interleaved[i + 7] & 0x10) >> 4;

                t[i / 8] =  (interleaved[i]     & 0x08) << 4;
                t[i / 8] += (interleaved[i + 1] & 0x08) << 3;
                t[i / 8] += (interleaved[i + 2] & 0x08) << 2;
                t[i / 8] += (interleaved[i + 3] & 0x08) << 1;
                t[i / 8] += interleaved[i + 4] & 0x08;
                t[i / 8] += (interleaved[i + 5] & 0x08) >> 1;
                t[i / 8] += (interleaved[i + 6] & 0x08) >> 2;
                t[i / 8] += (interleaved[i + 7] & 0x08) >> 3;

                u[i / 8] =  (interleaved[i]     & 0x04) << 5;
                u[i / 8] += (interleaved[i + 1] & 0x04) << 4;
                u[i / 8] += (interleaved[i + 2] & 0x04) << 3;
                u[i / 8] += (interleaved[i + 3] & 0x04) << 2;
                u[i / 8] += (interleaved[i + 4] & 0x04) << 1;
                u[i / 8] += interleaved[i + 5] & 0x04;
                u[i / 8] += (interleaved[i + 6] & 0x04) >> 1;
                u[i / 8] += (interleaved[i + 7] & 0x04) >> 2;

                v[i / 8] =  (interleaved[i]     & 0x02) << 6;
                v[i / 8] += (interleaved[i + 1] & 0x02) << 5;
                v[i / 8] += (interleaved[i + 2] & 0x02) << 4;
                v[i / 8] += (interleaved[i + 3] & 0x02) << 3;
                v[i / 8] += (interleaved[i + 4] & 0x02) << 2;
                v[i / 8] += (interleaved[i + 5] & 0x02) << 1;
                v[i / 8] += interleaved[i + 6] & 0x02;
                v[i / 8] += (interleaved[i + 7] & 0x02) >> 1;

                w[i / 8] =  (interleaved[i]     & 0x01) << 7;
                w[i / 8] += (interleaved[i + 1] & 0x01) << 6;
                w[i / 8] += (interleaved[i + 2] & 0x01) << 5;
                w[i / 8] += (interleaved[i + 3] & 0x01) << 4;
                w[i / 8] += (interleaved[i + 4] & 0x01) << 3;
                w[i / 8] += (interleaved[i + 5] & 0x01) << 2;
                w[i / 8] += (interleaved[i + 6] & 0x01) << 1;
                w[i / 8] += interleaved[i + 7] & 0x01;
            }

            DateTime end          = DateTime.UtcNow;
            TimeSpan deinterleave = end - start;

            byte[] sequential = new byte[interleaved.Length];
            start = DateTime.UtcNow;

            int qStart = p.Length * 1;
            int rStart = p.Length * 2;
            int sStart = p.Length * 3;
            int tStart = p.Length * 4;
            int uStart = p.Length * 5;
            int vStart = p.Length * 6;
            int wStart = p.Length * 7;

            for(int i = 0; i < p.Length; i++)
            {
                sequential[i]          = (byte)p[i];
                sequential[qStart + i] = (byte)q[i];
                sequential[rStart + i] = (byte)r[i];
                sequential[sStart + i] = (byte)s[i];
                sequential[tStart + i] = (byte)t[i];
                sequential[uStart + i] = (byte)u[i];
                sequential[vStart + i] = (byte)v[i];
                sequential[wStart + i] = (byte)w[i];
            }

            end = DateTime.UtcNow;
            TimeSpan sequentialize = end - start;

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0}ms to deinterleave subchannel.",
                                       deinterleave.TotalMilliseconds);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0}ms to sequentialize subchannel.",
                                       sequentialize.TotalMilliseconds);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0}ms to transform subchannel.",
                                       deinterleave.TotalMilliseconds + sequentialize.TotalMilliseconds);

            return sequential;
        }

        static byte[] ClauniaSubchannelUntransform(byte[] sequential)
        {
            if(sequential == null)
                return null;

            int[] p = new int[sequential.Length / 8];
            int[] q = new int[sequential.Length / 8];
            int[] r = new int[sequential.Length / 8];
            int[] s = new int[sequential.Length / 8];
            int[] t = new int[sequential.Length / 8];
            int[] u = new int[sequential.Length / 8];
            int[] v = new int[sequential.Length / 8];
            int[] w = new int[sequential.Length / 8];

            int qStart = p.Length * 1;
            int rStart = p.Length * 2;
            int sStart = p.Length * 3;
            int tStart = p.Length * 4;
            int uStart = p.Length * 5;
            int vStart = p.Length * 6;
            int wStart = p.Length * 7;

            DateTime start = DateTime.UtcNow;

            for(int i = 0; i < p.Length; i++)
            {
                p[i] = sequential[i];
                q[i] = sequential[qStart + i];
                r[i] = sequential[rStart + i];
                s[i] = sequential[sStart + i];
                t[i] = sequential[tStart + i];
                u[i] = sequential[uStart + i];
                v[i] = sequential[vStart + i];
                w[i] = sequential[wStart + i];
            }

            DateTime end             = DateTime.UtcNow;
            TimeSpan desequentialize = end - start;

            byte[] interleaved = new byte[sequential.Length];
            start = DateTime.UtcNow;

            for(int i = 0; i < interleaved.Length; i += 8)
            {
                interleaved[i]     =  (byte)((p[i / 8] & 0x80) == 0x80 ? 0x80 : 0);
                interleaved[i + 1] += (byte)((p[i / 8] & 0x40) == 0x40 ? 0x80 : 0);
                interleaved[i + 2] += (byte)((p[i / 8] & 0x20) == 0x20 ? 0x80 : 0);
                interleaved[i + 3] += (byte)((p[i / 8] & 0x10) == 0x10 ? 0x80 : 0);
                interleaved[i + 4] += (byte)((p[i / 8] & 0x08) == 0x08 ? 0x80 : 0);
                interleaved[i + 5] += (byte)((p[i / 8] & 0x04) == 0x04 ? 0x80 : 0);
                interleaved[i + 6] += (byte)((p[i / 8] & 0x02) == 0x02 ? 0x80 : 0);
                interleaved[i + 7] += (byte)((p[i / 8] & 0x01) == 0x01 ? 0x80 : 0);

                interleaved[i]     += (byte)((q[i / 8] & 0x80) == 0x80 ? 0x40 : 0);
                interleaved[i + 1] += (byte)((q[i / 8] & 0x40) == 0x40 ? 0x40 : 0);
                interleaved[i + 2] += (byte)((q[i / 8] & 0x20) == 0x20 ? 0x40 : 0);
                interleaved[i + 3] += (byte)((q[i / 8] & 0x10) == 0x10 ? 0x40 : 0);
                interleaved[i + 4] += (byte)((q[i / 8] & 0x08) == 0x08 ? 0x40 : 0);
                interleaved[i + 5] += (byte)((q[i / 8] & 0x04) == 0x04 ? 0x40 : 0);
                interleaved[i + 6] += (byte)((q[i / 8] & 0x02) == 0x02 ? 0x40 : 0);
                interleaved[i + 7] += (byte)((q[i / 8] & 0x01) == 0x01 ? 0x40 : 0);

                interleaved[i]     += (byte)((r[i / 8] & 0x80) == 0x80 ? 0x20 : 0);
                interleaved[i + 1] += (byte)((r[i / 8] & 0x40) == 0x40 ? 0x20 : 0);
                interleaved[i + 2] += (byte)((r[i / 8] & 0x20) == 0x20 ? 0x20 : 0);
                interleaved[i + 3] += (byte)((r[i / 8] & 0x10) == 0x10 ? 0x20 : 0);
                interleaved[i + 4] += (byte)((r[i / 8] & 0x08) == 0x08 ? 0x20 : 0);
                interleaved[i + 5] += (byte)((r[i / 8] & 0x04) == 0x04 ? 0x20 : 0);
                interleaved[i + 6] += (byte)((r[i / 8] & 0x02) == 0x02 ? 0x20 : 0);
                interleaved[i + 7] += (byte)((r[i / 8] & 0x01) == 0x01 ? 0x20 : 0);

                interleaved[i]     += (byte)((s[i / 8] & 0x80) == 0x80 ? 0x10 : 0);
                interleaved[i + 1] += (byte)((s[i / 8] & 0x40) == 0x40 ? 0x10 : 0);
                interleaved[i + 2] += (byte)((s[i / 8] & 0x20) == 0x20 ? 0x10 : 0);
                interleaved[i + 3] += (byte)((s[i / 8] & 0x10) == 0x10 ? 0x10 : 0);
                interleaved[i + 4] += (byte)((s[i / 8] & 0x08) == 0x08 ? 0x10 : 0);
                interleaved[i + 5] += (byte)((s[i / 8] & 0x04) == 0x04 ? 0x10 : 0);
                interleaved[i + 6] += (byte)((s[i / 8] & 0x02) == 0x02 ? 0x10 : 0);
                interleaved[i + 7] += (byte)((s[i / 8] & 0x01) == 0x01 ? 0x10 : 0);

                interleaved[i]     += (byte)((t[i / 8] & 0x80) == 0x80 ? 0x08 : 0);
                interleaved[i + 1] += (byte)((t[i / 8] & 0x40) == 0x40 ? 0x08 : 0);
                interleaved[i + 2] += (byte)((t[i / 8] & 0x20) == 0x20 ? 0x08 : 0);
                interleaved[i + 3] += (byte)((t[i / 8] & 0x10) == 0x10 ? 0x08 : 0);
                interleaved[i + 4] += (byte)((t[i / 8] & 0x08) == 0x08 ? 0x08 : 0);
                interleaved[i + 5] += (byte)((t[i / 8] & 0x04) == 0x04 ? 0x08 : 0);
                interleaved[i + 6] += (byte)((t[i / 8] & 0x02) == 0x02 ? 0x08 : 0);
                interleaved[i + 7] += (byte)((t[i / 8] & 0x01) == 0x01 ? 0x08 : 0);

                interleaved[i]     += (byte)((u[i / 8] & 0x80) == 0x80 ? 0x04 : 0);
                interleaved[i + 1] += (byte)((u[i / 8] & 0x40) == 0x40 ? 0x04 : 0);
                interleaved[i + 2] += (byte)((u[i / 8] & 0x20) == 0x20 ? 0x04 : 0);
                interleaved[i + 3] += (byte)((u[i / 8] & 0x10) == 0x10 ? 0x04 : 0);
                interleaved[i + 4] += (byte)((u[i / 8] & 0x08) == 0x08 ? 0x04 : 0);
                interleaved[i + 5] += (byte)((u[i / 8] & 0x04) == 0x04 ? 0x04 : 0);
                interleaved[i + 6] += (byte)((u[i / 8] & 0x02) == 0x02 ? 0x04 : 0);
                interleaved[i + 7] += (byte)((u[i / 8] & 0x01) == 0x01 ? 0x04 : 0);

                interleaved[i]     += (byte)((v[i / 8] & 0x80) == 0x80 ? 0x02 : 0);
                interleaved[i + 1] += (byte)((v[i / 8] & 0x40) == 0x40 ? 0x02 : 0);
                interleaved[i + 2] += (byte)((v[i / 8] & 0x20) == 0x20 ? 0x02 : 0);
                interleaved[i + 3] += (byte)((v[i / 8] & 0x10) == 0x10 ? 0x02 : 0);
                interleaved[i + 4] += (byte)((v[i / 8] & 0x08) == 0x08 ? 0x02 : 0);
                interleaved[i + 5] += (byte)((v[i / 8] & 0x04) == 0x04 ? 0x02 : 0);
                interleaved[i + 6] += (byte)((v[i / 8] & 0x02) == 0x02 ? 0x02 : 0);
                interleaved[i + 7] += (byte)((v[i / 8] & 0x01) == 0x01 ? 0x02 : 0);

                interleaved[i]     += (byte)((w[i / 8] & 0x80) == 0x80 ? 0x01 : 0);
                interleaved[i + 1] += (byte)((w[i / 8] & 0x40) == 0x40 ? 0x01 : 0);
                interleaved[i + 2] += (byte)((w[i / 8] & 0x20) == 0x20 ? 0x01 : 0);
                interleaved[i + 3] += (byte)((w[i / 8] & 0x10) == 0x10 ? 0x01 : 0);
                interleaved[i + 4] += (byte)((w[i / 8] & 0x08) == 0x08 ? 0x01 : 0);
                interleaved[i + 5] += (byte)((w[i / 8] & 0x04) == 0x04 ? 0x01 : 0);
                interleaved[i + 6] += (byte)((w[i / 8] & 0x02) == 0x02 ? 0x01 : 0);
                interleaved[i + 7] += (byte)((w[i / 8] & 0x01) == 0x01 ? 0x01 : 0);
            }

            end = DateTime.UtcNow;
            TimeSpan interleave = end - start;

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0}ms to de-sequentialize subchannel.",
                                       desequentialize.TotalMilliseconds);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0}ms to interleave subchannel.",
                                       interleave.TotalMilliseconds);

            AaruConsole.DebugWriteLine("Aaru Format plugin", "Took {0}ms to untransform subchannel.",
                                       interleave.TotalMilliseconds + desequentialize.TotalMilliseconds);

            return interleaved;
        }
    }
}